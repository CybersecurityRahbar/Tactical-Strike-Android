extends RefCounted

const AK_TAKES: Array[String] = [
	"res://assets/runtime/audio/ak47_27.ogg",
	"res://assets/runtime/audio/ak47_31.ogg"
]
const AK_BURST_TAKE := "res://assets/runtime/audio/ak47_36.ogg"
const AR_TAKES: Array[String] = [
	"res://assets/runtime/audio/ar15_24.ogg",
	"res://assets/runtime/audio/ar15_32.ogg"
]
const SNIPER_TAKE := "res://assets/runtime/audio/tikka_24.ogg"

static func play_fire(parent: Node, world_position: Vector3, profile: String, loudness_db := 0.0) -> void:
	var candidates: Array[String] = AK_TAKES
	var pitch := 1.0
	match profile:
		"ar15":
			candidates = AR_TAKES
		"ak_burst":
			candidates = [AK_BURST_TAKE]
		"sniper":
			candidates = [SNIPER_TAKE]
			pitch = 0.90
		"heavy":
			candidates = [AK_BURST_TAKE]
			pitch = 0.82
		_:
			candidates = AK_TAKES
	var path := candidates[randi_range(0, candidates.size() - 1)]
	var stream := load(path) as AudioStream
	if stream == null:
		push_warning("Recorded firearm audio is missing: " + path)
		return
	var player := AudioStreamPlayer3D.new()
	player.name = "RecordedFirearmReport"
	player.stream = stream
	player.volume_db = loudness_db
	player.pitch_scale = pitch * randf_range(0.985, 1.015)
	player.unit_size = 7.0
	player.max_distance = 1250.0
	player.attenuation_model = AudioStreamPlayer3D.ATTENUATION_INVERSE_DISTANCE
	player.doppler_tracking = AudioStreamPlayer3D.DOPPLER_TRACKING_DISABLED
	parent.add_child(player)
	player.global_position = world_position
	player.finished.connect(player.queue_free)
	player.play()

static func load_sand_footsteps() -> Array[AudioStream]:
	var result: Array[AudioStream] = []
	for name in ["sand_l", "sand_r", "sand_l2", "sand_r2"]:
		var stream := load("res://assets/runtime/audio/%s.ogg" % name) as AudioStream
		if stream != null:
			result.append(stream)
	return result
