class_name Gun extends RigidBody3D

@onready var marker: Marker3D = $"Marker3D"
@onready var viewport: Viewport = get_parent().get_parent()
@onready var _parent: CharacterBody3D = get_parent()

@export var projectile: PackedScene
@export var projectile_speed: float = 20
@export var shooting_interval: float


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("left_click") and _parent.collision_layer == 2:
		shoot()


func shoot() -> void:
	var bullet: RigidBody3D = projectile.instantiate()
	viewport.add_child(bullet)

	bullet.global_position = marker.global_position
	bullet.apply_central_impulse(marker.global_basis.y * projectile_speed)
