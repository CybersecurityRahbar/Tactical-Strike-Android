extends SceneTree

const REQUIRED_SCRIPTS: Array[String] = [
	"res://scripts/world/main_world.gd",
	"res://scripts/world/cover_piece.gd",
	"res://scripts/player/player_controller.gd",
	"res://scripts/weapons/weapon_data.gd",
	"res://scripts/weapons/weapon_controller.gd",
	"res://scripts/weapons/projectile.gd",
	"res://scripts/effects/impact_fx.gd",
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
	var terrain_mesh_instance := instance.get_node("BattlefieldTerrain") as MeshInstance3D
	var terrain_arrays: Array = terrain_mesh_instance.mesh.surface_get_arrays(0)
	var terrain_tangents: PackedFloat32Array = terrain_arrays[Mesh.ARRAY_TANGENT]
	if terrain_tangents == null or terrain_tangents.is_empty():
		push_error("Smoke test failed: terrain mesh has no generated tangents for its normal map.")
		quit(1)
		return
	var terrain_material := terrain_mesh_instance.mesh.surface_get_material(0) as StandardMaterial3D
	if terrain_material == null or terrain_material.albedo_texture == null or terrain_material.normal_texture == null or not terrain_material.normal_enabled:
		push_error("Smoke test failed: Rocky Gravel diffuse and normal textures are not bound to the terrain material.")
		quit(1)
		return
	if terrain_mesh_instance.mesh.get_aabb().size.y < 12.0:
		push_error("Smoke test failed: battlefield topography does not contain enough vertical relief.")
		quit(1)
		return
	var outcrops := instance.get_node_or_null("DryGroundStones") as MultiMeshInstance3D
	if outcrops == null or outcrops.multimesh.instance_count > 700:
		push_error("Smoke test failed: ground outcrops are missing or excessively dense.")
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
	if not instance.has_node("Player/Camera3D/WeaponViewModel/AK74M_ArmsAndWeapon"):
		push_error("Smoke test failed: animated AK-74M first-person scene was not created.")
		quit(1)
		return
	var weapon_model := instance.get_node("Player/Camera3D/WeaponViewModel/AK74M_ArmsAndWeapon")
	if _count_mesh_instances(weapon_model) == 0:
		push_error("Smoke test failed: imported weapon scene contains no visible mesh.")
		quit(1)
		return
	var animation_players := weapon_model.find_children("*", "AnimationPlayer", true, false)
	if animation_players.is_empty():
		push_error("Smoke test failed: the first-person AK-74M has no AnimationPlayer.")
		quit(1)
		return
	var weapon_animation := animation_players[0] as AnimationPlayer
	for clip: String in ["Rig|AK_Idle", "Rig|AK_Draw", "Rig|AK_Walk", "Rig|AK_Run", "Rig|AK_Shot", "Rig|AK_Reload_full"]:
		if not weapon_animation.has_animation(clip):
			push_error("Smoke test failed: missing authored weapon animation " + clip)
			quit(1)
			return
	var weapon_controller := instance.get_node("Player/WeaponController")
	if weapon_controller.get("magazine_ammo") != 30 or weapon_controller.get("reserve_ammo") < 60:
		push_error("Smoke test failed: magazine/reserve ammunition was not initialized.")
		quit(1)
		return
	if not instance.has_node("HUD/Overlay/MovementJoystick") or not instance.has_node("HUD/Overlay/AimButton") or not instance.has_node("HUD/Overlay/AmmoButton") or not instance.has_node("HUD/Overlay/FireButton") or not instance.has_node("HUD/Overlay/ReloadButton"):
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
	if player_node.global_position.y < start_position.y - 1.15:
		push_error("Smoke test failed: player fell through or below the terrain collision surface.")
		quit(1)
		return
	if not instance.has_node("HUD/Overlay/PauseButton") or not instance.has_node("HUD/Overlay/ArmoryButton") or not instance.has_node("HUD/Overlay/SettingsButton"):
		push_error("Smoke test failed: pause, armory or settings UI is missing.")
		quit(1)
		return

	var screen_size: Vector2 = instance.get_viewport().get_visible_rect().size
	var joystick_control := instance.get_node("HUD/Overlay/MovementJoystick") as Control
	var fire_control := instance.get_node("HUD/Overlay/FireButton") as Control
	var aim_control := instance.get_node("HUD/Overlay/AimButton") as Control
	var joystick_rect: Rect2 = joystick_control.get_global_rect()
	var fire_rect: Rect2 = fire_control.get_global_rect()
	var aim_rect: Rect2 = aim_control.get_global_rect()
	if joystick_rect.position.x > screen_size.x * 0.30:
		push_error("Smoke test failed: movement joystick is not anchored on the left side.")
		quit(1)
		return
	if fire_rect.position.x < screen_size.x * 0.50:
		push_error("Smoke test failed: fire control is not anchored on the right side.")
		quit(1)
		return
	if fire_rect.intersects(aim_rect):
		push_error("Smoke test failed: fire and aim touch targets overlap.")
		quit(1)
		return
	if instance.get_node("HUD/Overlay").layout_direction != Control.LAYOUT_DIRECTION_LTR:
		push_error("Smoke test failed: Arabic RTL system layout must not mirror shooter controls.")
		quit(1)
		return

	var weapon_audio_paths: Array[String] = [
		"res://assets/runtime/audio/ak_C_27P.ogg",
		"res://assets/runtime/audio/ak_C_28P.ogg",
		"res://assets/runtime/audio/ak_C_29P.ogg",
		"res://assets/runtime/audio/ak_C_31P.ogg",
		"res://assets/runtime/audio/ak_C_34P.ogg",
		"res://assets/runtime/audio/ak_C_36P.ogg"
	]
	for audio_path in weapon_audio_paths:
		if not ResourceLoader.exists(audio_path):
			push_error("Smoke test failed: missing recorded AK field sample " + audio_path)
			quit(1)
			return

	if InputMap.has_action("fire") == false or InputMap.has_action("reload") == false:
		push_error("Smoke test failed: fire or reload input action was not registered.")
		quit(1)
		return

	# Check a real trigger press consumes one round, then cycle the actual ammo modes.
	set_current_scene(instance)
	var ammo_before := int(weapon_controller.get("magazine_ammo"))
	weapon_controller.call("_try_fire")
	if int(weapon_controller.get("magazine_ammo")) != ammo_before - 1:
		push_error("Smoke test failed: firing did not consume one magazine round.")
		quit(1)
		return
	weapon_controller.call("cycle_ammo_type")
	if int(weapon_controller.get("ammo_type_index")) != 1:
		push_error("Smoke test failed: tracer ammunition mode did not activate.")
		quit(1)
		return

	print("SMOKE TEST PASSED: rocky PBR terrain, AK-74M + rigged arms, idle/walk/run/shot/reload clips, separate touch zones, player motion, recorded samples, firing and ammo modes initialized.")
	quit(0)
