extends CanvasLayer

var status_label: Label
var weapon_label: Label
var message_label: Label
var message_timer := 0.0

func _ready() -> void:
	var root := Control.new()
	root.name = "Overlay"
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(root)

	status_label = Label.new()
	status_label.position = Vector2(22.0, 18.0)
	status_label.add_theme_font_size_override("font_size", 18)
	status_label.add_theme_color_override("font_color", Color(0.92, 0.88, 0.78))
	root.add_child(status_label)

	weapon_label = Label.new()
	weapon_label.position = Vector2(22.0, 48.0)
	weapon_label.add_theme_font_size_override("font_size", 17)
	weapon_label.add_theme_color_override("font_color", Color(1.0, 0.82, 0.56))
	root.add_child(weapon_label)

	message_label = Label.new()
	message_label.position = Vector2(22.0, 82.0)
	message_label.add_theme_font_size_override("font_size", 15)
	message_label.add_theme_color_override("font_color", Color(0.88, 0.89, 0.78))
	root.add_child(message_label)

	var crosshair := Label.new()
	crosshair.text = "+"
	crosshair.add_theme_font_size_override("font_size", 24)
	crosshair.add_theme_color_override("font_color", Color(0.93, 0.90, 0.78, 0.85))
	crosshair.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	crosshair.position = Vector2(-7.0, -16.0)
	root.add_child(crosshair)

	if OS.has_feature("android"):
		_create_mobile_controls(root)

func _process(delta: float) -> void:
	if message_timer > 0.0:
		message_timer -= delta
		if message_timer <= 0.0 and message_label != null:
			message_label.text = ""

func set_battle_status(value: String) -> void:
	if status_label != null:
		status_label.text = value

func set_weapon_info(name: String, caliber: String, ammo: int, capacity: int, mode: String) -> void:
	if weapon_label != null:
		weapon_label.text = "%s  |  %s  |  %02d/%02d  |  %s" % [name, caliber, ammo, capacity, mode]

func set_message(value: String) -> void:
	if message_label != null:
		message_label.text = value
		message_timer = 2.2

func _create_mobile_controls(root: Control) -> void:
	var screen := get_viewport().get_visible_rect().size
	_make_hold_button(root, "↑", "move_forward", Vector2(112.0, screen.y - 220.0), Vector2(72.0, 66.0))
	_make_hold_button(root, "↓", "move_back", Vector2(112.0, screen.y - 76.0), Vector2(72.0, 66.0))
	_make_hold_button(root, "←", "move_left", Vector2(34.0, screen.y - 148.0), Vector2(72.0, 66.0))
	_make_hold_button(root, "→", "move_right", Vector2(190.0, screen.y - 148.0), Vector2(72.0, 66.0))
	_make_hold_button(root, "FIRE", "fire", Vector2(screen.x - 150.0, screen.y - 145.0), Vector2(118.0, 92.0))
	_make_hold_button(root, "SPRINT", "sprint", Vector2(screen.x - 285.0, screen.y - 82.0), Vector2(110.0, 58.0))
	_make_press_button(root, "RELOAD", "reload", Vector2(screen.x - 155.0, screen.y - 245.0), Vector2(112.0, 54.0))
	_make_press_button(root, "NEXT", "weapon_next", Vector2(screen.x - 280.0, screen.y - 245.0), Vector2(112.0, 54.0))

func _make_hold_button(root: Control, caption: String, action: String, button_position: Vector2, button_size: Vector2) -> void:
	if not InputMap.has_action(action):
		InputMap.add_action(action)
	var button := Button.new()
	button.text = caption
	button.position = button_position
	button.custom_minimum_size = button_size
	button.modulate = Color(1.0, 1.0, 1.0, 0.78)
	button.button_down.connect(func(): Input.action_press(action))
	button.button_up.connect(func(): Input.action_release(action))
	root.add_child(button)

func _make_press_button(root: Control, caption: String, action: String, button_position: Vector2, button_size: Vector2) -> void:
	var button := Button.new()
	button.text = caption
	button.position = button_position
	button.custom_minimum_size = button_size
	button.modulate = Color(1.0, 1.0, 1.0, 0.82)
	button.button_down.connect(func(): Input.action_press(action))
	button.button_up.connect(func(): Input.action_release(action))
	root.add_child(button)
