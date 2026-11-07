extends SceneTree

func _initialize() -> void:
	call_deferred("_run_smoke_test")

func _run_smoke_test() -> void:
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

	print("SMOKE TEST PASSED: main scene, terrain, player and HUD initialized.")
	quit(0)
