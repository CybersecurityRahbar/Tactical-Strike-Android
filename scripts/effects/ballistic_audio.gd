extends RefCounted

# Temporary procedural audio to make the prototype audible before licensed
# recorded weapon, fly-by and material-specific impact assets are added.
static func play_at(parent: Node, position: Vector3, duration: float, amplitude: float, tone_hz: float) -> void:
	var sample_rate := 22050
	var sample_count := maxi(1, int(float(sample_rate) * duration))
	var bytes := PackedByteArray()
	bytes.resize(sample_count * 2)
	var rng := RandomNumberGenerator.new()
	rng.randomize()
	for i in range(sample_count):
		var time := float(i) / float(sample_rate)
		var envelope := exp(-time * (9.0 / maxf(duration, 0.04)))
		var noise := rng.randf_range(-1.0, 1.0)
		var tonal := sin(TAU * tone_hz * time) * 0.22
		var sample := clampf((noise * 0.78 + tonal) * envelope * amplitude, -1.0, 1.0)
		bytes.encode_s16(i * 2, int(sample * 32767.0))

	var stream := AudioStreamWAV.new()
	stream.format = AudioStreamWAV.FORMAT_16_BITS
	stream.mix_rate = sample_rate
	stream.stereo = false
	stream.data = bytes

	var player := AudioStreamPlayer3D.new()
	player.stream = stream
	player.max_distance = 180.0
	player.unit_size = 4.0
	parent.add_child(player)
	player.global_position = position
	player.finished.connect(player.queue_free)
	player.play()
