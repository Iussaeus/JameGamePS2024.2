extends Area3D

@onready var controller: CharacterBody3D = get_parent()

var cached_closest: Interactable


func _process(_delta: float) -> void:
	var new_closest = get_closest_interactable()

	if new_closest != cached_closest:
		if is_instance_valid(cached_closest):
			unfocus(cached_closest)
		if new_closest != null:
			focus(new_closest)
		cached_closest = new_closest


func _input(event: InputEvent) -> void:
	if event.is_action_pressed("interact") and cached_closest != null:
		interact(cached_closest)


func _on_area_exited(area: Interactable) -> void:
	if cached_closest == area:
		unfocus(cached_closest)
		cached_closest = null


func interact(interactable: Interactable) -> void:
	print("Player: Interacted")
	interactable.interacted.emit()


func focus(interactable: Interactable) -> void:
	print("Player: Focused")
	interactable.focused.emit()


func unfocus(interactable: Interactable) -> void:
	print("Player: Unfocused")
	interactable.unfocused.emit()


func get_closest_interactable() -> Interactable:
	var list                  = get_overlapping_areas()
	var closest_distance      = INF
	var closest: Interactable = null

	for area in list:
		if area is Interactable:
			var interactable = area as Interactable
			var distance     = interactable.global_position.distance_to(global_position)

			if distance < closest_distance:
				closest = interactable
				closest_distance = distance
	return closest

