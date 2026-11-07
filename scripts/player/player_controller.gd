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
var crouched := false

func _ready() -> void:
	add_to_group("player")
	_create_input_map()
	var capsule := CapsuleShape3D.new()
	capsule.radius = 0.34
	capsule.height = 1.85
	var collider := CollisionShape3D.new()
	collider.shape = capsule
	collider.position.y = 0.93
	add_child(collider)

	camera = Camera3D.new()
	camera.name = "Camera3D"
	camera.position = Vector3(0.0, 1.62, 0.0)
	camera.current = true
	camera.fov = 78.0
	add_child(camera)

	weapon_controller = WEAPON_CONTROLLER.new()
	weapon_controller.name = "WeaponController"
	add_child(weapon_controller)
	weapon_controller.initialize(camera, self)

	if OS.has_feature("android"):
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	else:
		Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func attach_hud(hud_node: CanvasLayer) -> void:
	hud = hud_node
	if weapon_controller != null:
		weapon_controller.set_hud(hud)

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
	if event is InputEventMouseButton and event.pressed and not OS.has_feature("android"):
		if Input.mouse_mode != Input.MOUSE_MODE_CAPTURED:
			Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	if event is InputEventKey and event.pressed and event.keycode == KEY_ESCAPE:
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED:
		_apply_look(event.relative)
	elif event is InputEventScreenDrag:
		var viewport_width := get_viewport().get_visible_rect().size.x
		if event.position.x > viewport_width * 0.36:
			_apply_look(event.relative * 1.15)

func _apply_look(relative: Vector2) -> void:
	rotate_y(-relative.x * mouse_sensitivity)
	pitch = clampf(pitch - relative.y * mouse_sensitivity, -1.48, 1.48)
	camera.rotation.x = pitch

func _physics_process(delta: float) -> void:
	var input_vector := Input.get_vector("move_left", "move_right", "move_forward", "move_back")
	var local_direction := Vector3(input_vector.x, 0.0, input_vector.y)
	var direction := (global_transform.basis * local_direction).normalized()
	var is_crouching := Input.is_action_pressed("crouch")
	var speed := sprint_speed if Input.is_action_pressed("sprint") and not is_crouching else walk_speed
	if is_crouching:
		speed *= 0.52
		camera.position.y = move_toward(camera.position.y, 1.08, delta * 2.8)
	else:
		camera.position.y = move_toward(camera.position.y, 1.62, delta * 2.8)

	velocity.x = direction.x * speed
	velocity.z = direction.z * speed
	if is_on_floor():
		if Input.is_action_just_pressed("jump") and not is_crouching:
			velocity.y = jump_impulse
	else:
		velocity.y -= 21.0 * delta
	move_and_slide()
