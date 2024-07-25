extends CharacterBody3D


@export var movement_speed: float = 20

@onready var navigation_agent: NavigationAgent3D = $NavigationAgent3D
@onready var danger_area: Area3D = $DangerArea3D
@onready var safe_area: Area3D = $SafeArea3D
@onready var gun: Gun = $Gun

var _player_visible: bool = false
var _player_in_safe_area: bool = false
var _player_in_danger_area: bool = false


func _ready() -> void:
	set_physics_process(false)
	call_deferred("set_map")

	navigation_agent.velocity_computed.connect(_on_velocity_computed)

	safe_area.body_entered.connect(_on_safe_area_body_entered)
	safe_area.body_exited.connect(_on_safe_area_body_exited)

	danger_area.body_entered.connect(_on_danger_area_body_entered)
	danger_area.body_exited.connect(_on_danger_area_body_exited)


func _physics_process(_delta: float) -> void:

	var next_path_position: Vector3 = navigation_agent.get_next_path_position()
	var new_velocity: Vector3

	# print("next_point %v, velocity %v" % [next_path_position, new_velocity])

	if _player_visible and _player_in_safe_area:
		gun.shoot()
		new_velocity.x = 0
		new_velocity.z = 0

	elif _player_in_danger_area:
		new_velocity.x = next_path_position.direction_to(global_position).x * movement_speed
		new_velocity.z = next_path_position.direction_to(global_position).z * movement_speed
	else:
		new_velocity = global_position.direction_to(next_path_position) * movement_speed


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
	look_at(Global.player.global_position)
	velocity = safe_velocity
	move_and_slide()


func _on_danger_area_body_entered(body: Node3D) -> void:
	if body is CharacterBody3D:
		_player_in_danger_area = true
		_player_in_safe_area = false
		print("player in danger_area")
	

func _on_danger_area_body_exited(body: Node3D) -> void:
	if body is CharacterBody3D:
		_player_in_danger_area = false
		_player_in_safe_area = true
		print("player exited danger_area")


func _on_safe_area_body_entered(body: Node3D) -> void:
	if body is CharacterBody3D:
		_player_visible = true
		_player_in_safe_area = true
		var character_body := body as CharacterBody3D
		print("I see: ", character_body)



func _on_safe_area_body_exited(body: Node3D) -> void:
	if body is CharacterBody3D:
		_player_visible = false
		_player_in_safe_area = false
		var character_body := body as CharacterBody3D
		print(character_body, "is too far")
	

