extends CharacterBody3D

const WEAPON_CONTROLLER = preload("res://scripts/weapons/weapon_controller.gd")

@export var walk_speed := 5.6
@export var sprint_speed := 7.4
@export var jump_impulse := 4.2
@export var mouse_sensitivity := 0.0021

var camera: Camera3D
var weapon_controller: Node3D
var hud: CanvasLayer
var pitch := 0.0
var body_collider: CollisionShape3D
var is_crouched := false

func _ready() -> void:
	add_to_group("player")
	floor_snap_length = 0.32
	floor_max_angle = deg_to_rad(48.0)
	_create_input_map()
	var capsule := CapsuleShape3D.new()
	capsule.radius = 0.34
	capsule.height = 1.85
	body_collider = CollisionShape3D.new()
	body_collider.name = "BodyCollider"
	body_collider.shape = capsule
	body_collider.position.y = 0.93
	add_child(body_collider)
	camera = Camera3D.new()
	camera.name = "Camera3D"
	camera.position = Vector3(0.0, 1.62, 0.0)
	camera.current = true
	camera.fov = 78.0
	camera.near = 0.06
	add_child(camera)
	weapon_controller = WEAPON_CONTROLLER.new()
	weapon_controller.name = "WeaponController"
	add_child(weapon_controller)
	weapon_controller.initialize(camera, self)
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE if _is_touch_device() else Input.MOUSE_MODE_CAPTURED

func _is_touch_device() -> bool:
	return OS.get_name() == "Android" or OS.get_name() == "iOS" or DisplayServer.is_touchscreen_available()

func attach_hud(hud_node: CanvasLayer) -> void:
	hud = hud_node
	if weapon_controller != null:
		weapon_controller.set_hud(hud)

func adjust_sensitivity(amount: float) -> void:
	mouse_sensitivity = clampf(mouse_sensitivity + amount, 0.0008, 0.006)

func _create_input_map() -> void:
	_bind_key("move_forward", KEY_W)
	_bind_key("move_back", KEY_S)
	_bind_key("move_left", KEY_A)
	_bind_key("move_right", KEY_D)
	_bind_key("sprint", KEY_SHIFT)
	_bind_key("jump", KEY_SPACE)
	_bind_key("crouch", KEY_C)
	_bind_key("reload", KEY_R)
	_bind_key("weapon_next", KEY_Q)
	_bind_key("toggle_tracer", KEY_T)
	for i in range(7):
		_bind_key("weapon_%d" % (i + 1), KEY_1 + i)
	if not InputMap.has_action("fire"):
		InputMap.add_action("fire")
	var mouse_event := InputEventMouseButton.new()
	mouse_event.button_index = MOUSE_BUTTON_LEFT
	InputMap.action_add_event("fire", mouse_event)

func _bind_key(action: String, key_code: int) -> void:
	if not InputMap.has_action(action):
		InputMap.add_action(action)
	var event := InputEventKey.new()
	event.physical_keycode = key_code
	InputMap.action_add_event(action, event)

func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and not _is_touch_device():
		if Input.mouse_mode != Input.MOUSE_MODE_CAPTURED:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	if event is InputEventKey and event.pressed and event.keycode == KEY_ESCAPE:
		if get_tree().paused:
			get_tree().paused = false
		else:
			get_tree().paused = true
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		_apply_look(event.relative)
	elif event is InputEventScreenDrag and _is_touch_device():
		var viewport_size := get_viewport().get_visible_rect().size
		if event.position.x > viewport_size.x * 0.40 and event.position.y > 76.0 and event.position.y < viewport_size.y - 226.0:
			_apply_look(event.relative * 1.1)

func _apply_look(relative: Vector2) -> void:
	rotate_y(-relative.x * mouse_sensitivity)
	pitch = clampf(pitch - relative.y * mouse_sensitivity, -1.46, 1.46)
	camera.rotation.x = pitch

func _set_crouch(enabled: bool) -> void:
	if is_crouched == enabled:
		return
	is_crouched = enabled
	var capsule := body_collider.shape as CapsuleShape3D
	if capsule != null:
		capsule.height = 1.24 if is_crouched else 1.85
		body_collider.position.y = 0.62 if is_crouched else 0.93
	camera.position.y = 1.08 if is_crouched else 1.62

func _physics_process(delta: float) -> void:
	_set_crouch(Input.is_action_pressed("crouch"))
	var input_vector := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var local_direction := Vector3(input_vector.x, 0.0, input_vector.y)
	var direction := (global_transform.basis * local_direction).normalized()
	var speed := sprint_speed if Input.is_action_pressed("sprint") and not is_crouched else walk_speed
	if is_crouched:
		speed *= 0.52
	velocity.x = direction.x * speed
	velocity.z = direction.z * speed
	if is_on_floor():
		if Input.is_action_just_pressed("jump") and not is_crouched:
			velocity.y = jump_impulse
	else:
		velocity.y -= 21.0 * delta
	move_and_slide()
