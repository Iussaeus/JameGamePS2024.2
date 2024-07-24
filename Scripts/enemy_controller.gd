extends CharacterBody3D


@export var movement_speed: float = 20
@onready var navigation_agent: NavigationAgent3D = get_node("NavigationAgent3D")

func _ready() -> void:
	set_physics_process(false)
	call_deferred("set_map")


func _physics_process(_delta: float) -> void:

	set_movement_target(Global.player.global_position)

	var next_path_position: Vector3 = navigation_agent.get_next_path_position()
	var new_velocity: Vector3 = (global_position - next_path_position).normalized() * movement_speed
	if navigation_agent.avoidance_enabled:
		navigation_agent.set_velocity(new_velocity)
	else:
		velocity = new_velocity 
		move_and_slide()

func set_movement_target(movement_target: Vector3) -> void:
	navigation_agent.set_target_position(movement_target)


func set_map() -> void:
	await get_tree().physics_frame
	set_physics_process(true)
