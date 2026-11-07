extends RefCounted

# Game-tuning values for the AK-74M first-person weapon asset.
static func all_weapons() -> Array[Dictionary]:
	return [
		{
			"id": "ak74m", "name": "AK-74M", "caliber": "5.45×39 mm",
			"model_key": "ak74m", "audio_profile": "ak",
			"muzzle_velocity": 900.0, "drag_coefficient": 0.0000075,
			"gravity_scale": 1.0, "damage": 38.0, "penetration": 0.34,
			"max_range": 650.0, "magazine": 30, "fire_interval": 0.10,
			"tracer_every": 5, "projectile_kind": "bullet"
		}
	]
