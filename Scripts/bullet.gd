class_name Bullet extends RigidBody3D

@export var damage: float = 20
@export var speed: float = 20


func _ready() -> void:
	contact_monitor = true
	max_contacts_reported = 100
	body_exited.connect(_on_body_entered)


func _on_body_entered(body: Node) -> void:
	print("hit target ", body)
	if body.has_node("%HpComponent"):
		var hp_component: HpComponent = body.get_node("%HpComponent")
		hp_component.take_damage(damage)
	elif body.get_parent().has_node("HpComponent"):
		var hp_component: HpComponent = body.get_parent().get_node("HpComponent")	
		hp_component.take_damage(damage)
	queue_free()