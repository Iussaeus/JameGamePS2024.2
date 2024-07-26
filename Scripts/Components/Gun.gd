class_name Gun extends RigidBody3D

@onready var marker: Marker3D = $"Marker3D"
@onready var viewport: Viewport = get_parent().get_parent()
@onready var _parent: CharacterBody3D = get_parent()

@export var projectile: PackedScene
@export var projectile_speed: float = 20
@export var shooting_interval: float = 0.1 

@onready var _shoot_timer: Timer = Timer.new()


func _ready() -> void:
	add_child(_shoot_timer)
	_shoot_timer.wait_time = shooting_interval
	_shoot_timer.one_shot = true



func _process(_delta: float) -> void:
	if Input.is_action_pressed("left_click") and _parent.collision_layer == 2:
		shoot()


func shoot() -> void:
	if  _shoot_timer.is_stopped():
		_shoot_timer.start()
		var bullet: RigidBody3D = projectile.instantiate()
		viewport.add_child(bullet)

		bullet.global_position = marker.global_position
		bullet.apply_central_impulse(marker.global_basis.y * projectile_speed)
