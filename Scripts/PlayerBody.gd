extends CharacterBody3D

@export var dash_cooldown: float = 2
@export var dash_velocity: float = 200
@export var speed: float = 5

@onready var camera: Camera3D = %Camera3D
@onready var gun: RigidBody3D = $Gun
@onready var marker: Marker3D = $Marker3D

var dash_timer: Timer
var ray_len: float = 1000
var can_dash: bool = true
var gravity: float = ProjectSettings.get_setting("physics/3d/default_gravity")


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	dash_timer = Timer.new()
	add_child(dash_timer)

	dash_timer.one_shot = true
	dash_timer.timeout.connect(_on_timer_timeout)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	gun.global_position = marker.global_position

	var mouse_pos := get_viewport().get_mouse_position()
	var from := camera.project_ray_origin(mouse_pos)
	var to := from + camera.project_ray_normal(mouse_pos) * ray_len
	var query := PhysicsRayQueryParameters3D.create(from, to)
	var direct_space_state := get_world_3d().direct_space_state

	var intersection := direct_space_state.intersect_ray(query)
	
	if Input.get_last_mouse_velocity() != Vector2.ZERO and not intersection.is_empty():
		look_at(intersection.position)

	global_rotation = Vector3(0, global_rotation.y, 0)


func _physics_process(delta: float) -> void:
	var vel := velocity

	if not is_on_floor():
		vel.y -= gravity * delta

	var input_dir := Input.get_vector("left", "right", "forward", "backward")
	var direction := Vector3(input_dir.x, 0, input_dir.y).normalized()


	if direction != Vector3.ZERO:
		vel.x = direction.x * speed
		vel.z = direction.z * speed
	else:
		vel.x = move_toward(velocity.x, 0, speed)
		vel.z = move_toward(velocity.z, 0, speed)

	if Input.is_action_just_pressed("space") and can_dash:
		can_dash = false
		dash_timer.start(dash_cooldown)
		vel = direction * dash_velocity

	velocity = vel

	move_and_slide()


func _on_timer_timeout() -> void: can_dash = true
	
