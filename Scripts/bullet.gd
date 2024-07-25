class_name Bullet extends RigidBody3D

@export var damage: float = 20
@export var speed: float = 20
@export var life_time: float = 2
@export var debug_on: bool = false
var _life_timer: Timer


func _ready() -> void:
	contact_monitor = true
	max_contacts_reported = 100
	body_entered.connect(_on_body_entered)

	_life_timer = Timer.new()
	_life_timer.wait_time = life_time
	_life_timer.autostart = true
	_life_timer.one_shot = true
	_life_timer.timeout.connect(func () -> void: queue_free())
	add_child(_life_timer)
	

func _on_body_entered(body: Node) -> void:
	if debug_on: print("hit target ", body)
	if body.has_node("%HpComponent"):
		var hp_component: HpComponent = body.get_node("%HpComponent")
		hp_component.take_damage(damage)
	elif body.get_parent().has_node("HpComponent"):
		var hp_component: HpComponent = body.get_parent().get_node("HpComponent")	
		hp_component.take_damage(damage)
	queue_free()
