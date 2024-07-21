extends Node3D

@onready var ineractable: Interactable = $Interactable

var is_open = false


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	ineractable.focused.connect(_on_interactable_focused)
	ineractable.unfocused.connect(_on_interactable_unfocused)
	ineractable.interacted.connect(_on_interactable_interacted)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func open() -> void:
	print("Opening Now")


func _on_interactable_focused() -> void:
	print("Box: Focused")


func _on_interactable_unfocused() -> void:
	print("Box: Unfocused")


func _on_interactable_interacted() -> void:
	print("Box: Interacted")
