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
	"res://scripts/ui/hud.gd"
]

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
	if not instance.has_node("Player/Camera3D/WeaponViewModel"):
		push_error("Smoke test failed: first-person weapon view model was not created.")
		quit(1)
		return
	if not instance.has_node("HUD/Overlay/PauseButton") or not instance.has_node("HUD/Overlay/ArmoryButton") or not instance.has_node("HUD/Overlay/SettingsButton"):
		push_error("Smoke test failed: pause, armory or settings UI is missing.")
		quit(1)
		return

	print("SMOKE TEST PASSED: scripts, battlefield, player, visible weapon model and game UI initialized.")
	quit(0)
