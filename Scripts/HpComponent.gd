class_name HpComponent extends Node3D

@export var _hp: float = 100


func take_damage(damage: float) -> void:
	_hp -= damage
	print("took %.1f damage, current hp - %.1f" % [damage, _hp] )
