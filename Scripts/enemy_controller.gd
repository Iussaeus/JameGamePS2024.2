extends CharacterBody3D


@export var movement_speed: float = 20

@onready var navigation_agent: NavigationAgent3D = get_node("NavigationAgent3D")


func _ready() -> void:
	set_physics_process(false)
	call_deferred("set_map")

	navigation_agent.velocity_computed.connect(_on_velocity_computed)


func _physics_process(_delta: float) -> void:

	var next_path_position: Vector3 = navigation_agent.get_next_path_position()
	var new_velocity := global_position.direction_to(next_path_position) * movement_speed

	# print("next_point %v, velocity %v" % [next_path_position, new_velocity])

	if navigation_agent.avoidance_enabled:
		navigation_agent.set_velocity(new_velocity)
	else:
		_on_velocity_computed(new_velocity)

func set_movement_target(movement_target: Vector3) -> void:
	navigation_agent.set_target_position(movement_target)


func set_map() -> void:
	await get_tree().physics_frame
	set_physics_process(true)


func _on_velocity_computed(safe_velocity: Vector3) -> void:
	set_movement_target(Global.player.global_position)
	velocity = safe_velocity
	move_and_slide()
