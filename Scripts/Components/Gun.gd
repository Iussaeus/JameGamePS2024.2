class_name Gun extends RigidBody3D

@onready var marker: Marker3D = $"Marker3D"
@onready var _parent: Node3D = get_parent().get_parent()

@export var projectile: PackedScene
@export var projectile_speed: float = 20
@export var shooting_interval: float = 0.1 

@onready var _shoot_timer: Timer = Timer.new()


func _ready() -> void:
	add_child(_shoot_timer)
	_shoot_timer.wait_time = shooting_interval
	_shoot_timer.one_shot = true



func _process(_delta: float) -> void:
	var input_map: Array[StringName] =  InputMap.get_actions()
	for input in input_map:
		match input:
			&"left_click" when Input.is_action_pressed(input): shoot()


func shoot() -> void:
	if  _shoot_timer.is_stopped():
		_shoot_timer.start()
		var bullet: RigidBody3D = projectile.instantiate()
		_parent.add_child(bullet)

		bullet.global_position = marker.global_position
		bullet.apply_central_impulse(marker.global_basis.y.normalized() * projectile_speed)
