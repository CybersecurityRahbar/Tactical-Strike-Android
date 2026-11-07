extends RefCounted

# These are configurable game-tuning values, not exact real-world ballistic tables.
static func all_weapons() -> Array[Dictionary]:
	return [
		{
			"id": "ak74", "name": "AK-74", "caliber": "5.45×39 mm",
			"muzzle_velocity": 790.0, "drag_coefficient": 0.0000075,
			"gravity_scale": 1.0, "damage": 38.0, "penetration": 0.34,
			"max_range": 560.0, "magazine": 30, "fire_interval": 0.105,
			"tracer_every": 5, "projectile_kind": "bullet"
		},
		{
			"id": "m4a1", "name": "M4A1", "caliber": "5.56×45 mm",
			"muzzle_velocity": 850.0, "drag_coefficient": 0.0000068,
			"gravity_scale": 1.0, "damage": 35.0, "penetration": 0.32,
			"max_range": 540.0, "magazine": 30, "fire_interval": 0.095,
			"tracer_every": 5, "projectile_kind": "bullet"
		},
		{
			"id": "pkm", "name": "PKM", "caliber": "7.62×54R",
			"muzzle_velocity": 825.0, "drag_coefficient": 0.0000058,
			"gravity_scale": 1.0, "damage": 58.0, "penetration": 0.52,
			"max_range": 720.0, "magazine": 100, "fire_interval": 0.095,
			"tracer_every": 4, "projectile_kind": "bullet"
		},
		{
			"id": "rpg7", "name": "RPG-7", "caliber": "Rocket",
			"muzzle_velocity": 118.0, "drag_coefficient": 0.00013,
			"gravity_scale": 1.0, "damage": 175.0, "penetration": 0.95,
			"max_range": 330.0, "magazine": 1, "fire_interval": 1.1,
			"tracer_every": 1, "projectile_kind": "rocket", "explosion_radius": 7.5
		},
		{
			"id": "svd", "name": "SVD Dragunov", "caliber": "7.62×54R",
			"muzzle_velocity": 830.0, "drag_coefficient": 0.0000052,
			"gravity_scale": 1.0, "damage": 88.0, "penetration": 0.58,
			"max_range": 850.0, "magazine": 10, "fire_interval": 0.75,
			"tracer_every": 0, "projectile_kind": "bullet"
		},
		{
			"id": "dshk", "name": "DShK", "caliber": "12.7×108 mm",
			"muzzle_velocity": 850.0, "drag_coefficient": 0.0000037,
			"gravity_scale": 1.0, "damage": 125.0, "penetration": 0.88,
			"max_range": 1000.0, "magazine": 50, "fire_interval": 0.13,
			"tracer_every": 3, "projectile_kind": "bullet"
		},
		{
			"id": "m2", "name": "M2 Heavy", "caliber": ".50 BMG",
			"muzzle_velocity": 890.0, "drag_coefficient": 0.0000034,
			"gravity_scale": 1.0, "damage": 118.0, "penetration": 0.86,
			"max_range": 1000.0, "magazine": 50, "fire_interval": 0.12,
			"tracer_every": 3, "projectile_kind": "bullet"
		}
	]
