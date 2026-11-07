extends CanvasLayer

signal weapon_selected(index: int)
signal night_toggle_requested
signal sensitivity_adjust_requested(amount: float)

var root: Control
var status_label: Label
var weapon_label: Label
var message_label: Label
var message_timer := 0.0
var pause_panel: PanelContainer
var settings_panel: PanelContainer
var armory_panel: PanelContainer

const WEAPON_NAMES: Array[String] = [
	"AK-74", "M4A1", "PKM", "RPG-7", "SVD Dragunov", "DShK 12.7", "M2 .50"
]

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	root = Control.new()
	root.name = "Overlay"
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter = Control.MOUSE_FILTER_PASS
	add_child(root)
	_create_header()
	_create_crosshair()
	_create_panels()
	_create_top_buttons()
	if _is_touch_device():
		_create_mobile_controls()

func _is_touch_device() -> bool:
	return OS.get_name() == "Android" or OS.get_name() == "iOS" or DisplayServer.is_touchscreen_available()

func _make_panel_style() -> StyleBoxFlat:
	var style := StyleBoxFlat.new()
	style.bg_color = Color(0.035, 0.047, 0.043, 0.88)
	style.border_color = Color(0.68, 0.54, 0.32, 0.82)
	style.set_border_width_all(1)
	style.set_corner_radius_all(7)
	style.content_margin_left = 12.0
	style.content_margin_right = 12.0
	style.content_margin_top = 8.0
	style.content_margin_bottom = 8.0
	return style

func _style_button(button: Button, accent := false) -> void:
	button.focus_mode = Control.FOCUS_NONE
	button.add_theme_font_size_override("font_size", 16)
	button.add_theme_color_override("font_color", Color(0.94, 0.91, 0.82))
	button.add_theme_color_override("font_hover_color", Color(1.0, 0.96, 0.83))
	var normal := StyleBoxFlat.new()
	normal.bg_color = Color(0.10, 0.13, 0.11, 0.88) if not accent else Color(0.34, 0.19, 0.10, 0.94)
	normal.border_color = Color(0.66, 0.54, 0.34, 0.92)
	normal.set_border_width_all(1)
	normal.set_corner_radius_all(6)
	button.add_theme_stylebox_override("normal", normal)
	var hover := normal.duplicate() as StyleBoxFlat
	hover.bg_color = Color(0.18, 0.22, 0.17, 0.96) if not accent else Color(0.48, 0.28, 0.12, 0.98)
	button.add_theme_stylebox_override("hover", hover)
	button.add_theme_stylebox_override("pressed", hover)

func _create_header() -> void:
	var header := PanelContainer.new()
	header.name = "BattleStatusPanel"
	header.position = Vector2(16.0, 14.0)
	header.add_theme_stylebox_override("panel", _make_panel_style())
	root.add_child(header)
	var column := VBoxContainer.new()
	column.add_theme_constant_override("separation", 3)
	header.add_child(column)
	status_label = Label.new()
	status_label.name = "BattleStatus"
	status_label.text = "PLATEAU SECTOR 01  |  DAY"
	status_label.add_theme_font_size_override("font_size", 15)
	status_label.add_theme_color_override("font_color", Color(0.93, 0.90, 0.82))
	column.add_child(status_label)
	weapon_label = Label.new()
	weapon_label.name = "WeaponStatus"
	weapon_label.text = "AK-74  |  5.45×39 mm  |  30/30"
	weapon_label.add_theme_font_size_override("font_size", 14)
	weapon_label.add_theme_color_override("font_color", Color(0.93, 0.72, 0.42))
	column.add_child(weapon_label)
	message_label = Label.new()
	message_label.name = "CombatMessage"
	message_label.add_theme_font_size_override("font_size", 14)
	message_label.add_theme_color_override("font_color", Color(1.0, 0.82, 0.55))
	column.add_child(message_label)

func _create_crosshair() -> void:
	var crosshair := Label.new()
	crosshair.name = "Crosshair"
	crosshair.text = "＋"
	crosshair.add_theme_font_size_override("font_size", 25)
	crosshair.add_theme_color_override("font_color", Color(0.96, 0.94, 0.83, 0.92))
	crosshair.set_anchors_and_offsets_preset(Control.PRESET_CENTER)
	crosshair.position = Vector2(-9.0, -17.0)
	root.add_child(crosshair)

func _create_top_buttons() -> void:
	_make_top_button("PauseButton", "Ⅱ", -18.0, _toggle_pause)
	_make_top_button("ArmoryButton", "ARMORY", -91.0, _show_armory)
	_make_top_button("SettingsButton", "⚙", -196.0, _show_settings)

func _make_top_button(node_name: String, caption: String, right_offset: float, callback: Callable) -> void:
	var button := Button.new()
	button.name = node_name
	button.text = caption
	button.anchor_left = 1.0
	button.anchor_right = 1.0
	button.anchor_top = 0.0
	button.anchor_bottom = 0.0
	var width := 94.0 if caption == "ARMORY" else 48.0
	button.offset_left = right_offset - width
	button.offset_right = right_offset
	button.offset_top = 14.0
	button.offset_bottom = 58.0
	_style_button(button, caption == "Ⅱ")
	button.pressed.connect(callback)
	root.add_child(button)

func _create_panels() -> void:
	pause_panel = _new_center_panel("PausePanel", Vector2(410.0, 290.0))
	var pause_column := _panel_column(pause_panel, "TACTICAL PAUSE")
	_add_menu_button(pause_column, "RESUME MISSION", _resume, true)
	_add_menu_button(pause_column, "SETTINGS", _show_settings)
	_add_menu_button(pause_column, "WEAPON LOADOUT", _show_armory)
	pause_panel.visible = false

	settings_panel = _new_center_panel("SettingsPanel", Vector2(430.0, 300.0))
	var settings_column := _panel_column(settings_panel, "GAME SETTINGS")
	_add_menu_button(settings_column, "NIGHT / DAY", func(): night_toggle_requested.emit())
	_add_menu_button(settings_column, "LOOK SENSITIVITY  −", func(): sensitivity_adjust_requested.emit(-0.00025))
	_add_menu_button(settings_column, "LOOK SENSITIVITY  +", func(): sensitivity_adjust_requested.emit(0.00025))
	_add_menu_button(settings_column, "BACK", _back_to_previous)
	settings_panel.visible = false

	armory_panel = _new_center_panel("ArmoryPanel", Vector2(460.0, 490.0))
	var armory_column := _panel_column(armory_panel, "WEAPON LOADOUT")
	for index in range(WEAPON_NAMES.size()):
		var selected_index := index
		_add_menu_button(armory_column, "%d   %s" % [index + 1, WEAPON_NAMES[index]], func(): _select_weapon(selected_index))
	_add_menu_button(armory_column, "CLOSE", _back_to_previous)
	armory_panel.visible = false

func _new_center_panel(node_name: String, panel_size: Vector2) -> PanelContainer:
	var panel := PanelContainer.new()
	panel.name = node_name
	panel.anchor_left = 0.5
	panel.anchor_right = 0.5
	panel.anchor_top = 0.5
	panel.anchor_bottom = 0.5
	panel.offset_left = -panel_size.x * 0.5
	panel.offset_right = panel_size.x * 0.5
	panel.offset_top = -panel_size.y * 0.5
	panel.offset_bottom = panel_size.y * 0.5
	panel.add_theme_stylebox_override("panel", _make_panel_style())
	root.add_child(panel)
	return panel

func _panel_column(panel: PanelContainer, title: String) -> VBoxContainer:
	var column := VBoxContainer.new()
	column.add_theme_constant_override("separation", 8)
	panel.add_child(column)
	var heading := Label.new()
	heading.text = title
	heading.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	heading.add_theme_font_size_override("font_size", 21)
	heading.add_theme_color_override("font_color", Color(0.94, 0.76, 0.48))
	column.add_child(heading)
	return column

func _add_menu_button(parent: VBoxContainer, caption: String, callback: Callable, accent := false) -> void:
	var button := Button.new()
	button.text = caption
	button.custom_minimum_size = Vector2(0.0, 40.0)
	_style_button(button, accent)
	button.pressed.connect(callback)
	parent.add_child(button)

func _show_settings() -> void:
	_hide_menus()
	settings_panel.visible = true

func _show_armory() -> void:
	_hide_menus()
	armory_panel.visible = true

func _toggle_pause() -> void:
	if pause_panel.visible:
		_resume()
	else:
		_hide_menus()
		pause_panel.visible = true
		get_tree().paused = true

func _resume() -> void:
	get_tree().paused = false
	_hide_menus()

func _hide_menus() -> void:
	pause_panel.visible = false
	settings_panel.visible = false
	armory_panel.visible = false

func _back_to_previous() -> void:
	_hide_menus()
	if get_tree().paused:
		pause_panel.visible = true

func _select_weapon(index: int) -> void:
	weapon_selected.emit(index)
	_hide_menus()
	if get_tree().paused:
		pause_panel.visible = true

func _create_mobile_controls() -> void:
	_make_touch_button("MoveForward", "▲", "move_forward", 46.0, 184.0, 68.0, 62.0, false, false)
	_make_touch_button("MoveBack", "▼", "move_back", 46.0, 42.0, 68.0, 62.0, false, false)
	_make_touch_button("MoveLeft", "◀", "move_left", 0.0, 113.0, 68.0, 62.0, false, false)
	_make_touch_button("MoveRight", "▶", "move_right", 92.0, 113.0, 68.0, 62.0, false, false)
	_make_touch_button("FireButton", "FIRE", "fire", 30.0, 38.0, 128.0, 96.0, true, true)
	_make_touch_button("SprintButton", "RUN", "sprint", 176.0, 42.0, 82.0, 60.0, false, true)
	_make_touch_button("CrouchButton", "CROUCH", "crouch", 270.0, 42.0, 92.0, 60.0, false, true)
	_make_touch_button("JumpButton", "JUMP", "jump", 176.0, 112.0, 82.0, 58.0, false, true)
	_make_touch_button("ReloadButton", "RELOAD", "reload", 30.0, 150.0, 94.0, 52.0, false, true)
	_make_touch_button("NextWeaponButton", "NEXT", "weapon_next", 136.0, 150.0, 82.0, 52.0, false, true)

func _make_touch_button(node_name: String, caption: String, action: String, left: float, bottom: float, width: float, height: float, accent: bool, align_right: bool) -> void:
	if not InputMap.has_action(action):
		InputMap.add_action(action)
	var button := Button.new()
	button.name = node_name
	button.text = caption
	button.anchor_top = 1.0
	button.anchor_bottom = 1.0
	button.offset_top = -bottom - height
	button.offset_bottom = -bottom
	if align_right:
		button.anchor_left = 1.0
		button.anchor_right = 1.0
		button.offset_left = -left - width
		button.offset_right = -left
	else:
		button.anchor_left = 0.0
		button.anchor_right = 0.0
		button.offset_left = left
		button.offset_right = left + width
	_style_button(button, accent)
	button.add_theme_font_size_override("font_size", 15 if width < 100.0 else 18)
	button.modulate = Color(1.0, 1.0, 1.0, 0.82)
	button.button_down.connect(func(): Input.action_press(action))
	button.button_up.connect(func(): Input.action_release(action))
	root.add_child(button)

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
		message_timer = 2.6
