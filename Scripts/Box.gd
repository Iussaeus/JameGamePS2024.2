extends Node3D

@export var debug_on: bool = false
@onready var ineractable: Interactable = $RigidBody3D/Interactable

var is_open: bool = false


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	ineractable.focused.connect(_on_interactable_focused)
	ineractable.unfocused.connect(_on_interactable_unfocused)
	ineractable.interacted.connect(_on_interactable_interacted)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass


func open() -> void:
	if debug_on: print("Opening Now")


func _on_interactable_focused() -> void:
	if debug_on: print("Box: Focused")


func _on_interactable_unfocused() -> void:
	if debug_on: print("Box: Unfocused")


func _on_interactable_interacted() -> void:
	if debug_on: print("Box: Interacted")
