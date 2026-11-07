extends Control

signal value_changed(value: Vector2)

@export var outer_radius := 74.0
@export var knob_radius := 31.0
@export var deadzone := 0.10

var _active_touch := -1
var _mouse_active := false
var _value := Vector2.ZERO

func _ready() -> void:
	custom_minimum_size = Vector2(182.0, 182.0)
	mouse_filter = Control.MOUSE_FILTER_STOP
	focus_mode = Control.FOCUS_NONE
	queue_redraw()

func _gui_input(event: InputEvent) -> void:
	if event is InputEventScreenTouch:
		if event.pressed and _active_touch == -1:
			_active_touch = event.index
			_update_from_position(event.position)
			accept_event()
		elif not event.pressed and event.index == _active_touch:
			_active_touch = -1
			_reset_value()
			accept_event()
	elif event is InputEventScreenDrag and event.index == _active_touch:
		_update_from_position(event.position)
		accept_event()
	elif event is InputEventMouseButton and event.button_index == MOUSE_BUTTON_LEFT:
		if event.pressed:
			_mouse_active = true
			_update_from_position(event.position)
		else:
			_mouse_active = false
			_reset_value()
		accept_event()
	elif event is InputEventMouseMotion and _mouse_active:
		_update_from_position(event.position)
		accept_event()

func _update_from_position(pointer: Vector2) -> void:
	var center := size * 0.5
	var delta := pointer - center
	var max_distance := outer_radius * 0.78
	if delta.length() > max_distance:
		delta = delta.normalized() * max_distance
	var normalized := Vector2(delta.x, -delta.y) / max_distance
	if normalized.length() < deadzone:
		normalized = Vector2.ZERO
	_value = normalized.limit_length(1.0)
	value_changed.emit(_value)
	queue_redraw()

func _reset_value() -> void:
	_value = Vector2.ZERO
	value_changed.emit(_value)
	queue_redraw()

func _draw() -> void:
	var center := size * 0.5
	draw_circle(center, outer_radius + 4.0, Color(0.025, 0.032, 0.028, 0.28))
	draw_circle(center, outer_radius, Color(0.12, 0.15, 0.13, 0.62))
	draw_arc(center, outer_radius, 0.0, TAU, 64, Color(0.82, 0.70, 0.48, 0.62), 2.5, true)
	draw_arc(center, outer_radius * 0.67, 0.0, TAU, 48, Color(0.76, 0.68, 0.50, 0.20), 1.5, true)
	var knob_center := center + Vector2(_value.x, -_value.y) * outer_radius * 0.66
	draw_circle(knob_center, knob_radius + 2.0, Color(0.025, 0.028, 0.025, 0.58))
	draw_circle(knob_center, knob_radius, Color(0.42, 0.36, 0.25, 0.82))
	draw_arc(knob_center, knob_radius, 0.0, TAU, 48, Color(0.96, 0.88, 0.68, 0.88), 2.0, true)
