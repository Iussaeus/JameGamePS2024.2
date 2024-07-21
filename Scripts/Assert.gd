extends Node
func assert(truthy: bool, message: String) -> void:
	if cond:
		return
	print_debug("Assertion Failed:", msg)
	get_tree().paused = true