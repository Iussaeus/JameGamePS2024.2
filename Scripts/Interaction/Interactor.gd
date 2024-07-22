class_name Interactor
extends Area3D

var controller: CharacterBody3D


func interact(interactable: Interactable) -> void:
	interactable.interacted.emit()


func focus(interactable: Interactable) -> void:
	interactable.focused.emit()


func unfocus(interactable: Interactable) -> void:
	interactable.unfocused.emit()


func get_closest_interactalbe() -> Interactable:
	var list := get_overlapping_areas()
	var closest_distance := INF
	var closest: Interactable = null

	for area in list:
		if area is Interactable:
			var interactable := area as Interactable
			var distance := interactable.global_position.distance_to(global_position)

			if distance < closest_distance:
				closest = interactable
				closest_distance = distance
	return closest