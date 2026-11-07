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

func initialize(camera_ref: Camera3D, shooter_ref: Node3D) -> void:
	camera = camera_ref
	shooter = shooter_ref
	weapons = WEAPON_DATA.all_weapons()
	magazine_ammo = int(weapons[0].magazine)
	_update_hud()

func set_hud(hud_ref: CanvasLayer) -> void:
	hud = hud_ref
	_update_hud()

func _process(delta: float) -> void:
	fire_cooldown = maxf(0.0, fire_cooldown - delta)
	if Input.is_action_just_pressed("weapon_next"):
		_select_weapon((current_index + 1) % weapons.size())
	if Input.is_action_just_pressed("reload"):
		_reload()
	if Input.is_action_just_pressed("toggle_tracer"):
		tracer_mode = not tracer_mode
		_update_hud()
	for i in range(weapons.size()):
		if Input.is_action_just_pressed("weapon_%d" % (i + 1)):
			_select_weapon(i)
	if Input.is_action_pressed("fire"):
		_try_fire()

func _try_fire() -> void:
	if fire_cooldown > 0.0 or magazine_ammo <= 0:
		if magazine_ammo <= 0 and hud != null:
			hud.set_message("EMPTY — press R to reload")
		return
	var spec: Dictionary = weapons[current_index]
	fire_cooldown = float(spec.fire_interval)
	magazine_ammo -= 1
	total_shots += 1
	var automatic_tracer := int(spec.tracer_every) > 0 and total_shots % int(spec.tracer_every) == 0
	var use_tracer := tracer_mode or automatic_tracer or str(spec.projectile_kind) == "rocket"
	var forward := -camera.global_transform.basis.z.normalized()
	var muzzle := camera.global_position + camera.global_transform.basis * Vector3(0.18, -0.12, -0.48)
	var projectile := PROJECTILE.new()
	get_tree().current_scene.add_child(projectile)
	projectile.setup(muzzle, forward * float(spec.muzzle_velocity), spec, shooter, use_tracer)
	BALLISTIC_AUDIO.play_at(get_tree().current_scene, muzzle, 0.105, 0.75, 185.0)
	_spawn_muzzle_flash(muzzle)
	_update_hud()

func _spawn_muzzle_flash(_world_position: Vector3) -> void:
	var flash := MeshInstance3D.new()
	var sphere := SphereMesh.new()
	sphere.radius = 0.105
	sphere.height = 0.21
	flash.mesh = sphere
	var material := StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color = Color(1.0, 0.30, 0.055, 1.0)
	material.emission_enabled = true
	material.emission = Color(1.0, 0.15, 0.02)
	material.emission_energy_multiplier = 2.0
	flash.material_override = material
	camera.add_child(flash)
	flash.position = Vector3(0.18, -0.12, -0.56)
	var timer := Timer.new()
	timer.one_shot = true
	timer.wait_time = 0.045
	timer.timeout.connect(flash.queue_free)
	flash.add_child(timer)
	timer.start()

func _select_weapon(index: int) -> void:
	if index < 0 or index >= weapons.size():
		return
	current_index = index
	magazine_ammo = int(weapons[current_index].magazine)
	_update_hud()

func _reload() -> void:
	magazine_ammo = int(weapons[current_index].magazine)
	_update_hud()
	if hud != null:
		hud.set_message("MAGAZINE READY")

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
