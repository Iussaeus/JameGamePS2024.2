extends Camera3D

@onready var character: CharacterBody3D = $"%PlayerBody"

@export var camera_offset: Vector3 = Vector3(5, -20, 5)


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	look_at_from_position(character.global_position + camera_offset, character.global_position)
