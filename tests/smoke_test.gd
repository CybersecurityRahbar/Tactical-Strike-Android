extends SceneTree

const REQUIRED_SCRIPTS: Array[String] = [
	"res://scripts/world/main_world.gd",
	"res://scripts/world/cover_piece.gd",
	"res://scripts/player/player_controller.gd",
	"res://scripts/weapons/weapon_data.gd",
	"res://scripts/weapons/weapon_controller.gd",
	"res://scripts/weapons/projectile.gd",
	"res://scripts/effects/impact_fx.gd",
	"res://scripts/effects/ballistic_audio.gd",
	"res://scripts/effects/weapon_audio.gd",
	"res://scripts/ui/virtual_joystick.gd",
	"res://scripts/ui/hud.gd"
]

func _count_mesh_instances(node: Node) -> int:
	var count := 1 if node is MeshInstance3D else 0
	for child in node.get_children():
		count += _count_mesh_instances(child)
	return count

func _initialize() -> void:
	call_deferred("_run_smoke_test")

func _run_smoke_test() -> void:
	for script_path in REQUIRED_SCRIPTS:
		var script_resource := load(script_path) as Script
		if script_resource == null:
			push_error("Smoke test failed: could not load script %s." % script_path)
			quit(1)
			return
		if not script_resource.can_instantiate():
			push_error("Smoke test failed: script cannot instantiate %s." % script_path)
			quit(1)
			return

	var packed := load("res://scenes/main.tscn") as PackedScene
	if packed == null:
		push_error("Smoke test failed: main scene could not be loaded.")
		quit(1)
		return

	var instance := packed.instantiate()
	root.add_child(instance)
	await process_frame

	if not instance.has_node("BattlefieldTerrain"):
		push_error("Smoke test failed: procedural terrain was not created.")
		quit(1)
		return
	if not instance.has_node("Player"):
		push_error("Smoke test failed: player controller was not created.")
		quit(1)
		return
	if not instance.has_node("HUD"):
		push_error("Smoke test failed: HUD was not created.")
		quit(1)
		return
	if not instance.has_node("Player/Camera3D/WeaponViewModel/ImportedWeaponModel"):
		push_error("Smoke test failed: imported first-person weapon mesh was not created.")
		quit(1)
		return
	var weapon_model := instance.get_node("Player/Camera3D/WeaponViewModel/ImportedWeaponModel")
	if _count_mesh_instances(weapon_model) == 0:
		push_error("Smoke test failed: imported weapon scene contains no visible mesh.")
		quit(1)
		return
	if not instance.has_node("HUD/Overlay/MovementJoystick") or not instance.has_node("HUD/Overlay/AimButton"):
		push_error("Smoke test failed: analog movement joystick or aim control is missing.")
		quit(1)
		return
	var player_node := instance.get_node("Player")
	var start_position: Vector3 = player_node.global_position
	player_node.call("set_mobile_move_vector", Vector2(0.0, 1.0))
	for frame in range(48):
		await physics_frame
	player_node.call("set_mobile_move_vector", Vector2.ZERO)
	if player_node.global_position.distance_to(start_position) < 0.45:
		push_error("Smoke test failed: player did not respond to movement joystick input.")
		quit(1)
		return
	if not instance.has_node("HUD/Overlay/PauseButton") or not instance.has_node("HUD/Overlay/ArmoryButton") or not instance.has_node("HUD/Overlay/SettingsButton"):
		push_error("Smoke test failed: pause, armory or settings UI is missing.")
		quit(1)
		return

	print("SMOKE TEST PASSED: terrain, imported weapon mesh, analog joystick/aim, menus and player movement initialized.")
	quit(0)
