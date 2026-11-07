extends Node3D

const PLAYER_SCRIPT = preload("res://scripts/player/player_controller.gd")
const HUD_SCRIPT = preload("res://scripts/ui/hud.gd")
const COVER_SCRIPT = preload("res://scripts/world/cover_piece.gd")

const MAP_SIZE := 700.0
const TERRAIN_DIVISIONS := 100

var terrain_noise := FastNoiseLite.new()
var sun: DirectionalLight3D
var world_environment: WorldEnvironment
var sky_material: ProceduralSkyMaterial
var hud: CanvasLayer
var player: CharacterBody3D
var night_mode := false

var earth_material: StandardMaterial3D
var sandbag_material: StandardMaterial3D
var brick_material: StandardMaterial3D
var stone_material: StandardMaterial3D
var dark_material: StandardMaterial3D
var soldier_material: StandardMaterial3D

func _ready() -> void:
	_setup_noise()
	_create_materials()
	_build_environment()
	_build_terrain()
	_build_cover_layout()
	_scatter_ground_details()
	_place_distant_defenders()
	_spawn_player()
	_create_hud()

func _setup_noise() -> void:
	terrain_noise.seed = 20261107
	terrain_noise.noise_type = FastNoiseLite.TYPE_SIMPLEX
	terrain_noise.frequency = 0.012
	terrain_noise.fractal_octaves = 4
	terrain_noise.fractal_lacunarity = 2.0
	terrain_noise.fractal_gain = 0.48

func _create_materials() -> void:
	earth_material = _make_material(Color(0.34, 0.27, 0.19))
	sandbag_material = _make_material(Color(0.47, 0.40, 0.29))
	brick_material = _make_material(Color(0.39, 0.29, 0.23))
	stone_material = _make_material(Color(0.37, 0.36, 0.32))
	dark_material = _make_material(Color(0.20, 0.19, 0.16))
	soldier_material = _make_material(Color(0.18, 0.22, 0.17))

func _make_material(color: Color) -> StandardMaterial3D:
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 1.0
	material.metallic = 0.0
	return material

func _build_environment() -> void:
	world_environment = WorldEnvironment.new()
	world_environment.name = "WorldEnvironment"
	var environment := Environment.new()
	environment.background_mode = Environment.BG_SKY
	var sky := Sky.new()
	sky_material = ProceduralSkyMaterial.new()
	sky_material.sky_top_color = Color(0.27, 0.34, 0.39)
	sky_material.sky_horizon_color = Color(0.72, 0.61, 0.45)
	sky_material.ground_bottom_color = Color(0.20, 0.18, 0.14)
	sky_material.ground_horizon_color = Color(0.55, 0.45, 0.33)
	sky.sky_material = sky_material
	environment.sky = sky
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_SKY
	environment.ambient_light_energy = 0.75
	environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	world_environment.environment = environment
	add_child(world_environment)

	sun = DirectionalLight3D.new()
	sun.name = "Sun"
	sun.rotation_degrees = Vector3(-42.0, -28.0, 0.0)
	sun.light_energy = 1.15
	sun.shadow_enabled = true
	add_child(sun)

func _terrain_height(x: float, z: float) -> float:
	var rolling := terrain_noise.get_noise_2d(x, z) * 2.4
	var broad := sin(x * 0.008) * 1.5 + cos(z * 0.010) * 1.4
	var ridge_one := 5.2 * exp(-pow((z - (52.0 + sin(x * 0.018) * 18.0)) / 42.0, 2.0))
	var ridge_two := 6.0 * exp(-pow((z - (-72.0 + sin(x * 0.014) * 24.0)) / 48.0, 2.0))
	var low_saddle := -1.4 * exp(-pow((z + 5.0) / 30.0, 2.0))
	var height := rolling + broad + ridge_one + ridge_two + low_saddle
	var attacker_trench: float = absf(z - (205.0 + sin(x * 0.055) * 2.2))
	var defender_trench: float = absf(z - (-205.0 + sin(x * 0.045) * 2.4))
	if attacker_trench < 3.0:
		height -= (1.0 - attacker_trench / 3.0) * 1.0
	if defender_trench < 3.0:
		height -= (1.0 - defender_trench / 3.0) * 1.15
	return height

func _build_terrain() -> void:
	var tool := SurfaceTool.new()
	tool.begin(Mesh.PRIMITIVE_TRIANGLES)
	tool.set_material(earth_material)
	var half := MAP_SIZE * 0.5
	var step := MAP_SIZE / float(TERRAIN_DIVISIONS)

	for ix in range(TERRAIN_DIVISIONS):
		for iz in range(TERRAIN_DIVISIONS):
			var x0 := -half + float(ix) * step
			var x1 := x0 + step
			var z0 := -half + float(iz) * step
			var z1 := z0 + step
			var p00 := Vector3(x0, _terrain_height(x0, z0), z0)
			var p10 := Vector3(x1, _terrain_height(x1, z0), z0)
			var p01 := Vector3(x0, _terrain_height(x0, z1), z1)
			var p11 := Vector3(x1, _terrain_height(x1, z1), z1)
			tool.add_vertex(p00)
			tool.add_vertex(p01)
			tool.add_vertex(p10)
			tool.add_vertex(p10)
			tool.add_vertex(p01)
			tool.add_vertex(p11)

	tool.generate_normals()
	var terrain_mesh := tool.commit()
	var terrain := MeshInstance3D.new()
	terrain.name = "BattlefieldTerrain"
	terrain.mesh = terrain_mesh
	add_child(terrain)

	var body := StaticBody3D.new()
	body.name = "TerrainCollision"
	var shape := CollisionShape3D.new()
	shape.shape = terrain_mesh.create_trimesh_shape()
	body.add_child(shape)
	add_child(body)

func _build_cover_layout() -> void:
	# Forward attacker cover, separated from the main defensive line by a long open approach.
	_build_trench_line(205.0, -46.0, 46.0, 27, "AttackerForwardTrench")
	_build_trench_line(-205.0, -64.0, 64.0, 35, "DefenderMainTrench")
	_build_trench_line(-258.0, -38.0, 38.0, 22, "DefenderFallbackTrench")

	# Irregular stone and brick firing positions on the rolling approach.
	for i in range(12):
		var x := -95.0 + float(i) * 17.0
		var z := 45.0 + sin(float(i) * 0.82) * 18.0
		_add_cover("ApproachStoneCover", Vector3(x, _terrain_height(x, z) + 0.65, z), Vector3(2.8, 1.3, 1.0), stone_material, 2200.0, 0.82)

	_add_house(Vector3(-47.0, 0.0, -250.0), Vector2(14.0, 11.0), brick_material, "WestDefenderHouse")
	_add_house(Vector3(53.0, 0.0, -272.0), Vector2(16.0, 12.0), stone_material, "EastDefenderHouse")

	# Short walls and staggered barriers around the two defensive buildings.
	for i in range(8):
		var x := -78.0 + float(i) * 4.0
		_add_cover("BrickDefensiveWall", Vector3(x, _terrain_height(x, -242.0) + 0.7, -242.0), Vector3(3.7, 1.4, 0.55), brick_material, 1500.0, 0.72)
	for i in range(9):
		var x := 32.0 + float(i) * 4.0
		_add_cover("StoneDefensiveWall", Vector3(x, _terrain_height(x, -286.0) + 0.75, -286.0), Vector3(3.5, 1.5, 0.7), stone_material, 2400.0, 0.95)

func _build_trench_line(center_z: float, x_start: float, x_end: float, count: int, prefix: String) -> void:
	for i in range(count):
		var t := float(i) / float(maxi(count - 1, 1))
		var x: float = lerpf(x_start, x_end, t)
		var z := center_z + sin(float(i) * 0.43) * 1.5
		var ground := _terrain_height(x, z)
		for row in range(2):
			var offset_x: float = x + (0.45 if row == 1 else 0.0)
			var offset_z: float = z + (0.12 if row == 1 else 0.0)
			_add_cover(prefix + "_Sandbag", Vector3(offset_x, ground + 0.30 + float(row) * 0.48, offset_z), Vector3(1.9, 0.56, 0.82), sandbag_material, 580.0, 0.22)

func _add_house(center: Vector3, footprint: Vector2, material: StandardMaterial3D, house_name: String) -> void:
	var x := center.x
	var z := center.z
	var ground := _terrain_height(x, z)
	var width := footprint.x
	var depth := footprint.y
	var wall_height := 3.2
	var wall_y := ground + wall_height * 0.5
	_add_cover(house_name + "_NorthWall", Vector3(x, wall_y, z - depth * 0.5), Vector3(width, wall_height, 0.55), material, 3600.0, 1.05)
	_add_cover(house_name + "_SouthWall", Vector3(x, wall_y, z + depth * 0.5), Vector3(width, wall_height, 0.55), material, 3600.0, 1.05)
	_add_cover(house_name + "_WestWall", Vector3(x - width * 0.5, wall_y, z), Vector3(0.55, wall_height, depth), material, 3600.0, 1.05)
	_add_cover(house_name + "_EastWall", Vector3(x + width * 0.5, wall_y, z), Vector3(0.55, wall_height, depth), material, 3600.0, 1.05)
	_add_cover(house_name + "_Roof", Vector3(x, ground + wall_height + 0.22, z), Vector3(width + 0.5, 0.45, depth + 0.5), dark_material, 4200.0, 1.2)

func _add_cover(node_name: String, position: Vector3, size: Vector3, material: StandardMaterial3D, durability: float, armor_rating: float) -> StaticBody3D:
	var body := StaticBody3D.new()
	body.name = node_name
	body.position = position
	body.collision_layer = 1
	body.collision_mask = 1
	body.set_script(COVER_SCRIPT)
	body.set("integrity", durability)
	body.set("armor_rating", armor_rating)
	body.add_to_group("destructible_cover")

	var visual := MeshInstance3D.new()
	var box := BoxMesh.new()
	box.size = size
	visual.mesh = box
	visual.material_override = material
	body.add_child(visual)

	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = size
	collision.shape = shape
	body.add_child(collision)
	add_child(body)
	return body

func _scatter_ground_details() -> void:
	var rng := RandomNumberGenerator.new()
	rng.seed = 741207
	var rock_mesh := SphereMesh.new()
	rock_mesh.radius = 0.48
	rock_mesh.height = 0.72
	rock_mesh.radial_segments = 6
	rock_mesh.rings = 3
	var multi := MultiMesh.new()
	multi.transform_format = MultiMesh.TRANSFORM_3D
	multi.mesh = rock_mesh
	multi.instance_count = 160
	for i in range(multi.instance_count):
		var x := rng.randf_range(-325.0, 325.0)
		var z := rng.randf_range(-325.0, 325.0)
		var y := _terrain_height(x, z) + 0.14
		var scale := Vector3(rng.randf_range(0.28, 1.1), rng.randf_range(0.18, 0.55), rng.randf_range(0.30, 0.9))
		var basis := Basis().scaled(scale)
		multi.set_instance_transform(i, Transform3D(basis, Vector3(x, y, z)))
	var rocks := MultiMeshInstance3D.new()
	rocks.name = "DryGroundStones"
	rocks.multimesh = multi
	rocks.material_override = stone_material
	add_child(rocks)

func _place_distant_defenders() -> void:
	var positions: Array[Vector2] = [
		Vector2(-54.0, -198.0), Vector2(-39.0, -199.0), Vector2(-17.0, -203.0),
		Vector2(6.0, -200.0), Vector2(27.0, -207.0), Vector2(47.0, -204.0),
		Vector2(62.0, -211.0), Vector2(-62.0, -256.0), Vector2(22.0, -261.0)
	]
	for i in range(positions.size()):
		var point := positions[i]
		var soldier := Node3D.new()
		soldier.name = "DistantDefender_%02d" % i
		soldier.position = Vector3(point.x, _terrain_height(point.x, point.y), point.y)
		var torso := MeshInstance3D.new()
		var capsule := CapsuleMesh.new()
		capsule.radius = 0.22
		capsule.height = 1.12
		torso.mesh = capsule
		torso.position.y = 0.82
		torso.material_override = soldier_material
		soldier.add_child(torso)
		var head := MeshInstance3D.new()
		var sphere := SphereMesh.new()
		sphere.radius = 0.16
		sphere.height = 0.32
		head.mesh = sphere
		head.position.y = 1.48
		head.material_override = soldier_material
		soldier.add_child(head)
		add_child(soldier)

func _spawn_player() -> void:
	player = PLAYER_SCRIPT.new()
	player.name = "Player"
	add_child(player)
	player.global_position = Vector3(0.0, _terrain_height(0.0, 292.0), 292.0)

func _create_hud() -> void:
	hud = HUD_SCRIPT.new()
	hud.name = "HUD"
	add_child(hud)
	hud.set_battle_status("PLATEAU SECTOR 01  |  DAY  |  DEFENDERS: DISTANT")
	if player != null and player.has_method("attach_hud"):
		player.attach_hud(hud)

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_N:
			_set_night_mode(not night_mode)

func _set_night_mode(enabled: bool) -> void:
	night_mode = enabled
	if sun != null:
		sun.light_energy = 0.14 if night_mode else 1.15
	if world_environment != null:
		world_environment.environment.ambient_light_energy = 0.24 if night_mode else 0.75
	if sky_material != null:
		sky_material.sky_top_color = Color(0.012, 0.018, 0.035) if night_mode else Color(0.27, 0.34, 0.39)
		sky_material.sky_horizon_color = Color(0.055, 0.065, 0.09) if night_mode else Color(0.72, 0.61, 0.45)
	if hud != null:
		hud.set_battle_status("PLATEAU SECTOR 01  |  NIGHT" if night_mode else "PLATEAU SECTOR 01  |  DAY")
