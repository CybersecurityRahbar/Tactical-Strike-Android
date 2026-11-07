extends Node3D

const WEAPON_DATA = preload("res://scripts/weapons/weapon_data.gd")
const PROJECTILE = preload("res://scripts/weapons/projectile.gd")
const WEAPON_AUDIO = preload("res://scripts/effects/weapon_audio.gd")

var camera: Camera3D
var shooter: Node3D
var hud: CanvasLayer
var weapons: Array[Dictionary] = []
var model_paths: Dictionary = {}
var current_index := 0
var magazine_ammo := 0
var fire_cooldown := 0.0
var total_shots := 0
var tracer_mode := false
var aiming := false
var weapon_view_model: Node3D
var imported_weapon: Node3D
var recoil_amount := 0.0
var aim_blend := 0.0
var view_model_base := Vector3(0.34, -0.27, -0.58)
var view_model_ads := Vector3(0.04, -0.205, -0.73)
var sway_time := 0.0
var current_fire_profile := ""

func initialize(camera_ref: Camera3D, shooter_ref: Node3D) -> void:
	camera = camera_ref
	shooter = shooter_ref
	_read_model_manifest()
	var all_weapons: Array[Dictionary] = WEAPON_DATA.all_weapons()
	# Do not pretend that an unavailable PKM, RPG, SVD or heavy-gun model is present.
	# The first playable asset pass exposes only the two weapons with actual imported meshes.
	for spec in all_weapons:
		var model_key := str(spec.get("model_key", ""))
		if model_paths.has(model_key) and ResourceLoader.exists(str(model_paths[model_key])):
			weapons.append(spec)
	if weapons.is_empty():
		push_error("No imported first-person weapon models were prepared for this build.")
		return
	magazine_ammo = int(weapons[0].magazine)
	current_fire_profile = str(weapons[0].audio_profile)
	_build_weapon_view_model()
	_update_hud()

func _read_model_manifest() -> void:
	var path := "res://assets/runtime/weapon_manifest.json"
	if not FileAccess.file_exists(path):
		push_error("Weapon model manifest missing. External asset preparation did not run.")
		return
	var file := FileAccess.open(path, FileAccess.READ)
	if file == null:
		push_error("Could not open weapon model manifest.")
		return
	var parsed = JSON.parse_string(file.get_as_text())
	if not parsed is Dictionary:
		push_error("Weapon model manifest is invalid JSON.")
		return
	model_paths = parsed

func set_hud(hud_ref: CanvasLayer) -> void:
	hud = hud_ref
	_update_hud()

func set_aiming(enabled: bool) -> void:
	aiming = enabled

func _process(delta: float) -> void:
	fire_cooldown = maxf(0.0, fire_cooldown - delta)
	recoil_amount = move_toward(recoil_amount, 0.0, delta * 2.5)
	aim_blend = move_toward(aim_blend, 1.0 if aiming else 0.0, delta * 7.0)
	sway_time += delta
	if weapon_view_model != null:
		var target_position := view_model_base.lerp(view_model_ads, aim_blend)
		var sway := Vector3(sin(sway_time * 2.1) * 0.004, cos(sway_time * 4.2) * 0.004, 0.0)
		weapon_view_model.position = target_position + sway + Vector3(0.0, recoil_amount * 0.16, recoil_amount)
		weapon_view_model.rotation.x = -recoil_amount * 0.44
	if weapons.is_empty():
		return
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
	current_fire_profile = str(weapons[current_index].audio_profile)
	_build_weapon_view_model()
	_update_hud()

func reload_weapon() -> void:
	if weapons.is_empty():
		return
	magazine_ammo = int(weapons[current_index].magazine)
	_update_hud()
	if hud != null:
		hud.set_message("MAGAZINE READY")

func _try_fire() -> void:
	if weapons.is_empty():
		return
	if fire_cooldown > 0.0 or magazine_ammo <= 0:
		if magazine_ammo <= 0 and hud != null:
			hud.set_message("EMPTY — RELOAD")
		return
	var spec: Dictionary = weapons[current_index]
	fire_cooldown = float(spec.fire_interval)
	magazine_ammo -= 1
	total_shots += 1
	recoil_amount = 0.12 if aiming else 0.085
	if is_instance_valid(shooter) and shooter.has_method("add_weapon_recoil"):
		shooter.call("add_weapon_recoil", 0.018 if aiming else 0.012)
	var automatic_tracer := int(spec.tracer_every) > 0 and total_shots % int(spec.tracer_every) == 0
	var use_tracer := tracer_mode or automatic_tracer
	var forward := -camera.global_transform.basis.z.normalized()
	var muzzle := camera.global_position + camera.global_transform.basis * Vector3(0.27, -0.19, -1.14)
	var projectile := PROJECTILE.new()
	get_tree().current_scene.add_child(projectile)
	projectile.setup(muzzle, forward * float(spec.muzzle_velocity), spec, shooter, use_tracer)
	WEAPON_AUDIO.play_fire(get_tree().current_scene, muzzle, str(spec.audio_profile), -1.0 if not aiming else -2.5)
	_spawn_muzzle_light(muzzle)
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
	var key := str(weapons[current_index].model_key)
	var resource_path := str(model_paths.get(key, ""))
	var loaded = load(resource_path)
	if loaded is PackedScene:
		var instance := (loaded as PackedScene).instantiate()
		if not instance is Node3D:
			instance.queue_free()
			push_error("Imported weapon scene root is not Node3D: " + resource_path)
			return
		imported_weapon = instance as Node3D
	elif loaded is Mesh:
		var mesh_instance := MeshInstance3D.new()
		mesh_instance.mesh = loaded as Mesh
		imported_weapon = mesh_instance
	else:
		push_error("Imported weapon asset is neither a PackedScene nor a Mesh: " + resource_path)
		return
	imported_weapon.name = "ImportedWeaponModel"
	weapon_view_model.add_child(imported_weapon)
	_fit_weapon_to_view_model(imported_weapon)
	imported_weapon.position += Vector3(0.0, 0.0, -0.02)

func _fit_weapon_to_view_model(model: Node3D) -> void:
	var points: Array[Vector3] = []
	_collect_mesh_corners(model, Transform3D.IDENTITY, points)
	if points.is_empty():
		push_error("Imported weapon model has no visible MeshInstance3D children.")
		return
	var bounds := _bounds_from_points(points)
	# Orient the model's longest authored axis along the camera's forward view axis.
	if bounds.size.x >= bounds.size.z and bounds.size.x >= bounds.size.y:
		model.rotation_degrees.y = 90.0
	elif bounds.size.y >= bounds.size.z and bounds.size.y >= bounds.size.x:
		model.rotation_degrees.x = 90.0
	points.clear()
	_collect_mesh_corners(model, Transform3D.IDENTITY, points)
	bounds = _bounds_from_points(points)
	var longest := maxf(bounds.size.x, maxf(bounds.size.y, bounds.size.z))
	if longest <= 0.001:
		push_error("Imported weapon mesh has invalid dimensions.")
		return
	var target_length := 0.94
	var scale_factor := target_length / longest
	model.scale = Vector3.ONE * scale_factor
	model.position = -bounds.get_center() * scale_factor
	model.position.y -= 0.015

func _collect_mesh_corners(node: Node, parent_transform: Transform3D, points: Array[Vector3]) -> void:
	var local_transform := parent_transform
	if node is Node3D:
		local_transform = parent_transform * (node as Node3D).transform
	if node is MeshInstance3D:
		var mesh_instance := node as MeshInstance3D
		if mesh_instance.mesh != null:
			var box := mesh_instance.mesh.get_aabb()
			for ix in range(2):
				for iy in range(2):
					for iz in range(2):
						var corner := Vector3(
							box.position.x + box.size.x * float(ix),
							box.position.y + box.size.y * float(iy),
							box.position.z + box.size.z * float(iz)
						)
						points.append(local_transform * corner)
	for child in node.get_children():
		_collect_mesh_corners(child, local_transform, points)

func _bounds_from_points(points: Array[Vector3]) -> AABB:
	var bounds := AABB(points[0], Vector3.ZERO)
	for i in range(1, points.size()):
		bounds = bounds.expand(points[i])
	return bounds

func _spawn_muzzle_light(world_position: Vector3) -> void:
	var light := OmniLight3D.new()
	light.name = "MuzzleFlashLight"
	light.light_color = Color(1.0, 0.58, 0.28)
	light.light_energy = 1.35
	light.omni_range = 2.2
	light.shadow_enabled = false
	get_tree().current_scene.add_child(light)
	light.global_position = world_position
	var timer := Timer.new()
	timer.one_shot = true
	timer.wait_time = 0.035
	timer.timeout.connect(light.queue_free)
	light.add_child(timer)
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
		"AIM" if aiming else ("TRACER" if tracer_mode else "STANDARD")
	)
