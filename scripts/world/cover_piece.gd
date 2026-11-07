extends StaticBody3D

@export var integrity: float = 600.0
@export var armor_rating: float = 0.25

func apply_ballistic_damage(damage: float, penetration: float, _caliber: String = "") -> void:
	var effectiveness := clampf(penetration / maxf(armor_rating, 0.08), 0.08, 1.0)
	integrity -= damage * effectiveness
	if integrity <= 0.0:
		queue_free()
