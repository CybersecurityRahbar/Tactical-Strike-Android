extends Node3D

const WEAPON_DATA = preload("res://scripts/weapons/weapon_data.gd")
const PROJECTILE = preload("res://scripts/weapons/projectile.gd")
const BALLISTIC_AUDIO = preload("res://scripts/effects/ballistic_audio.gd")

var camera: Camera3D
var shooter: Node3D
var hud: CanvasLayer
var weapons: Array[Dictionary] = []
var current_index := 0
var magazine_ammo := 0
var fire_cooldown := 0.0
var total_shots := 0
var tracer_mode := false
var weapon_view_model: Node3D
var recoil_amount := 0.0
var view_model_base := Vector3(0.34, -0.29, -0.58)

func initialize(camera_ref: Camera3D, shooter_ref: Node3D) -> void:
	camera = camera_ref
	shooter = shooter_ref
	weapons = WEAPON_DATA.all_weapons()
	if weapons.is_empty():
		return
	magazine_ammo = int(weapons[0].magazine)
	_build_weapon_view_model()
	_update_hud()

func set_hud(hud_ref: CanvasLayer) -> void:
	hud = hud_ref
	_update_hud()

func _process(delta: float) -> void:
	fire_cooldown = maxf(0.0, fire_cooldown - delta)
	recoil_amount = move_toward(recoil_amount, 0.0, delta * 2.8)
	if weapon_view_model != null:
		weapon_view_model.position = view_model_base + Vector3(0.0, recoil_amount * 0.28, recoil_amount)
	if Input.is_action_just_pressed("weapon_next"):
		select_weapon((current_index + 1) % weapons.size())
	if Input.is_action_just_pressed("reload"):
		reload_weapon()
	if Input.is_action_just_pressed("toggle_tracer"):
		tracer_mode = not tracer_mode
		_update_hud()
	for i in range(weapons.size()):
		if Input.is_action_just_pressed("weapon_%d" % (i + 1)):
			select_weapon(i)
	if Input.is_action_pressed("fire"):
		_try_fire()

func select_weapon(index: int) -> void:
	if index < 0 or index >= weapons.size():
		return
	current_index = index
	magazine_ammo = int(weapons[current_index].magazine)
	_build_weapon_view_model()
	_update_hud()

func reload_weapon() -> void:
	magazine_ammo = int(weapons[current_index].magazine)
	_update_hud()
	if hud != null:
		hud.set_message("MAGAZINE READY")

func _try_fire() -> void:
	if fire_cooldown > 0.0 or magazine_ammo <= 0:
		if magazine_ammo <= 0 and hud != null:
			hud.set_message("EMPTY — RELOAD")
		return
	var spec: Dictionary = weapons[current_index]
	fire_cooldown = float(spec.fire_interval)
	magazine_ammo -= 1
	total_shots += 1
	recoil_amount = 0.075
	var automatic_tracer := int(spec.tracer_every) > 0 and total_shots % int(spec.tracer_every) == 0
	var use_tracer := tracer_mode or automatic_tracer or str(spec.projectile_kind) == "rocket"
	var forward := -camera.global_transform.basis.z.normalized()
	var muzzle := camera.global_position + camera.global_transform.basis * Vector3(0.34, -0.30, -1.30)
	var projectile := PROJECTILE.new()
	get_tree().current_scene.add_child(projectile)
	projectile.setup(muzzle, forward * float(spec.muzzle_velocity), spec, shooter, use_tracer)
	BALLISTIC_AUDIO.play_at(get_tree().current_scene, muzzle, 0.105, 0.75, 185.0)
	_spawn_muzzle_flash()
	_update_hud()

func _build_weapon_view_model() -> void:
	if camera == null or weapons.is_empty():
		return
	if is_instance_valid(weapon_view_model):
		weapon_view_model.queue_free()
	weapon_view_model = Node3D.new()
	weapon_view_model.name = "WeaponViewModel"
	weapon_view_model.position = view_model_base
	camera.add_child(weapon_view_model)
	var metal := StandardMaterial3D.new()
	metal.albedo_color = Color(0.105, 0.115, 0.105)
	metal.metallic = 0.62
	metal.roughness = 0.48
	var dark_metal := StandardMaterial3D.new()
	dark_metal.albedo_color = Color(0.045, 0.052, 0.049)
	dark_metal.metallic = 0.38
	dark_metal.roughness = 0.72
	var furniture := StandardMaterial3D.new()
	furniture.albedo_color = Color(0.18, 0.145, 0.095)
	furniture.roughness = 0.92
	var cloth := StandardMaterial3D.new()
	cloth.albedo_color = Color(0.16, 0.19, 0.145)
	cloth.roughness = 1.0
	var weapon_id := str(weapons[current_index].id)
	var is_launcher := weapon_id == "rpg7"
	var is_heavy := weapon_id == "dshk" or weapon_id == "m2" or weapon_id == "pkm"
	var barrel_length := 0.82 if is_heavy else 0.68
	if is_launcher:
		barrel_length = 0.94
	_add_box(weapon_view_model, "Receiver", Vector3(0.19, 0.17, 0.40), Vector3(0.0, 0.0, 0.02), metal)
	_add_box(weapon_view_model, "TopCover", Vector3(0.17, 0.045, 0.37), Vector3(0.0, 0.105, -0.015), dark_metal)
	_add_box(weapon_view_model, "Handguard", Vector3(0.135, 0.12, barrel_length * 0.62), Vector3(0.0, -0.005, -0.28), furniture)
	_add_box(weapon_view_model, "Barrel", Vector3(0.055, 0.055, barrel_length), Vector3(0.0, 0.025, -0.34 - barrel_length * 0.46), metal)
	_add_box(weapon_view_model, "Muzzle", Vector3(0.075, 0.075, 0.10), Vector3(0.0, 0.025, -0.39 - barrel_length), dark_metal)
	_add_box(weapon_view_model, "Stock", Vector3(0.15, 0.13, 0.34), Vector3(0.0, -0.01, 0.36), furniture)
	_add_box(weapon_view_model, "Magazine", Vector3(0.13, 0.25, 0.16), Vector3(0.0, -0.205, 0.055), dark_metal, Vector3(-9.0, 0.0, 0.0))
	_add_box(weapon_view_model, "PistolGrip", Vector3(0.09, 0.19, 0.12), Vector3(0.055, -0.17, 0.20), furniture, Vector3(-12.0, 0.0, -7.0))
	_add_box(weapon_view_model, "RearSight", Vector3(0.065, 0.07, 0.045), Vector3(0.0, 0.16, 0.08), dark_metal)
	_add_box(weapon_view_model, "FrontSight", Vector3(0.045, 0.085, 0.045), Vector3(0.0, 0.085, -0.62), dark_metal)
	_add_box(weapon_view_model, "LeftSleeve", Vector3(0.17, 0.16, 0.42), Vector3(-0.22, -0.20, -0.25), cloth, Vector3(-12.0, -8.0, -16.0))
	_add_box(weapon_view_model, "RightSleeve", Vector3(0.15, 0.15, 0.34), Vector3(0.23, -0.19, 0.10), cloth, Vector3(8.0, 10.0, 18.0))
	_add_box(weapon_view_model, "LeftGlove", Vector3(0.13, 0.10, 0.16), Vector3(-0.12, -0.12, -0.38), dark_metal)
	_add_box(weapon_view_model, "RightGlove", Vector3(0.12, 0.11, 0.15), Vector3(0.12, -0.10, 0.05), dark_metal)
	if is_launcher:
		_add_box(weapon_view_model, "LauncherTube", Vector3(0.22, 0.22, 0.98), Vector3(0.0, 0.07, -0.50), dark_metal)
	elif weapon_id == "svd":
		_add_box(weapon_view_model, "Optic", Vector3(0.10, 0.11, 0.25), Vector3(0.0, 0.22, -0.03), dark_metal)
	elif is_heavy:
		_add_box(weapon_view_model, "TopRail", Vector3(0.07, 0.035, 0.42), Vector3(0.0, 0.14, -0.22), dark_metal)

func _add_box(parent: Node3D, node_name: String, size: Vector3, position: Vector3, material: Material, rotation := Vector3.ZERO) -> void:
	var mesh_instance := MeshInstance3D.new()
	mesh_instance.name = node_name
	var box := BoxMesh.new()
	box.size = size
	mesh_instance.mesh = box
	mesh_instance.position = position
	mesh_instance.rotation_degrees = rotation
	mesh_instance.material_override = material
	parent.add_child(mesh_instance)

func _spawn_muzzle_flash() -> void:
	var flash := MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 0.095
	sphere.height = 0.19
	flash.mesh = sphere
	var material := StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color = Color(1.0, 0.30, 0.055, 1.0)
	material.emission_enabled = true
	material.emission = Color(1.0, 0.15, 0.02)
	material.emission_energy_multiplier = 2.0
	flash.material_override = material
	camera.add_child(flash)
	flash.position = Vector3(0.34, -0.30, -1.30)
	var timer := Timer.new()
	timer.one_shot = true
	timer.wait_time = 0.045
	timer.timeout.connect(flash.queue_free)
	flash.add_child(timer)
	timer.start()

func _update_hud() -> void:
	if hud == null or weapons.is_empty():
		return
	var spec: Dictionary = weapons[current_index]
	hud.set_weapon_info(
		str(spec.name),
		str(spec.caliber),
		magazine_ammo,
		int(spec.magazine),
		"TRACER" if tracer_mode else "STANDARD"
	)
