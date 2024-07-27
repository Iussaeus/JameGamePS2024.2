extends Control

@export var TileSize: Vector2 = Vector2(144, 144)
@export var InventoryDimensions: Vector2 = Vector2(8, 4) # 1152/144, 576/144
@export var SelectedItemZIndex: int = 1000

@onready var InventoryPanel: ColorRect = $BackGround
@onready var InventoryGrids: Sprite2D = $InventoryGrid
@onready var Inventory: Area2D = $InventoryGrid/InventoryArea

var InventoryItemslots: Dictionary = {}
var InventoryItems: Dictionary = {}

var isItemSelected: bool = false
var IsSelectedItemInsideInventory: bool
var selectedItem: Control
var IsDraggingItem: bool = false
var CursorItemDragOffset: Vector2 = Vector2(-8, -8)
var OverlappingWithItems: Array
var ItemPrevPosition: Vector2

var InvalidColor: Color = Color(1, 0.36, 0.36, 1)
var ValidColor: Color =  Color(1, 1, 1, 1)

var is_open: bool = false
func open() -> void:
	visible = true
	is_open = true
	
func close() -> void:
	visible = false
	is_open = false

func _process(delta: float) -> void:
	if Input.is_action_just_pressed("inventory"):
		if is_open:
			close()
		else:
			open()
			
	if IsDraggingItem:
		selectedItem.position = (self.get_global_mouse_position() + CursorItemDragOffset).snapped(TileSize)

func _ready() -> void:
	close()
	
	# Onready ColorRect, Sprite2D, Area2D
	InventoryPanel.size = Vector2(TileSize.x * InventoryDimensions.x, TileSize.y * InventoryDimensions.y)
	InventoryGrids.region_enabled = true
	InventoryGrids.region_rect = Rect2(0, 0, InventoryDimensions.x * TileSize.x, InventoryDimensions.y * TileSize.y)
	
	Inventory.connect("area_entered", Callable(self, "item_inside_inventory"))
	Inventory.connect("area_exited", Callable(self, "item_goes_outside_inventory"))
	
	# Adding signal connections to items
	for item in get_tree().get_nodes_in_group("item"):
		add_signal_connections(item)

func add_signal_connections(item: Control) -> void:
	item.connect("gui_input", Callable(self, "cursor_in_item").bind(item))
	item.get_node("NinePatchRect/Sprite2D/Area2D").connect("area_entered", Callable(self, "overlapping_with_other_item").bind(item))
	item.get_node("NinePatchRect/Sprite2D/Area2D").connect("area_exited", Callable(self, "not_overlapping_with_other_item").bind(item))
	
func remove_signal_connections(item: Control) -> void:
	if item.is_connected("gui_input", Callable(self, "cursor_in_item").bind(item)):
		item.disconnect("gui_input", Callable(self, "cursor_in_item").bind(item))
	
	if item.get_node("NinePatchRect/Sprite2D/Area2D").is_connected("area_entered", Callable(self, "overlapping_with_other_item").bind(item)):
		item.get_node("NinePatchRect/Sprite2D/Area2D").disconnect("area_entered", Callable(self, "overlapping_with_other_item").bind(item))
		
	if item.get_node("NinePatchRect/Sprite2D/Area2D").is_connected("area_exited", Callable(self, "not_overlapping_with_other_item").bind(item)):
		item.get_node("NinePatchRect/Sprite2D/Area2D").disconnect("area_exited", Callable(self, "not_overlapping_with_other_item").bind(item))

func cursor_in_item(event: InputEvent, item: Control) -> void:
	if event.is_action("select_item"):
		isItemSelected = true
		selectedItem = item
		selectedItem.get_node("NinePatchRect/Sprite2D").z_index = SelectedItemZIndex
		selectedItem.get_node("NinePatchRect").z_index = SelectedItemZIndex
		ItemPrevPosition = selectedItem.position
		
	if event is InputEventMouseMotion:
		if isItemSelected:
			IsDraggingItem = true
			
	if event.is_action_released("select_item"):
		selectedItem.get_node("NinePatchRect/Sprite2D").z_index = 0
		# (currently sprite is null, but after adding the sprite, ninepatch will be obsolete/removed)
		selectedItem.get_node("NinePatchRect").z_index = 0
		if OverlappingWithItems.size() > 0:
			selectedItem.position = ItemPrevPosition
			selectedItem.get_node("NinePatchRect/Sprite2D").modulate = ValidColor
			# (currently sprite is null, but after adding the sprite, ninepatch will be obsolete/removed)
			selectedItem.get_node("NinePatchRect").modulate = ValidColor
		else:
			if IsSelectedItemInsideInventory:
				if not add_item_to_inventory(selectedItem):
					selectedItem.position = ItemPrevPosition
		isItemSelected = false
		IsDraggingItem = false
		selectedItem = null
		 
func overlapping_with_other_item(area: Area2D, item: Control) -> void:
	print("overlap")
	if area.get_parent().get_parent() == selectedItem:
		return
	if area == Inventory:
		return
	OverlappingWithItems.append(item)
	if selectedItem:
		selectedItem.get_node("NinePatchRect/Sprite2D").modulate = InvalidColor
		# (currently sprite is null, but after adding the sprite, ninepatch will be obsolete/removed)
		selectedItem.get_node("NinePatchRect").modulate = InvalidColor
		
func not_overlapping_with_other_item(area: Area2D, item: Control) -> void:
	print("not_overlap")
	if area.get_parent().get_parent() == selectedItem:
		return
	if area == Inventory:
		return
	OverlappingWithItems.erase(item)
	if OverlappingWithItems.size() == 0 and isItemSelected:
		selectedItem.get_node("NinePatchRect/Sprite2D").modulate = ValidColor
		# (currently sprite is null, but after adding the sprite, ninepatch will be obsolete/removed)
		selectedItem.get_node("NinePatchRect").modulate = ValidColor
		
func item_inside_inventory(area: Area2D) -> void:
	print("inside")
	IsSelectedItemInsideInventory = true
	
func item_goes_outside_inventory(area: Area2D) -> void:
	print("outside")
	IsSelectedItemInsideInventory = false
	
func add_item_to_inventory(item: Control) -> bool:
	var slotID: Vector2 = item.position / TileSize
	var ItemSlotSize: Vector2 = item.size / TileSize
	
	var ItemMaxSlotID: Vector2 = slotID + ItemSlotSize - Vector2(1, 1)
	var InventorySlotBounds: Vector2 = InventoryDimensions - Vector2(1, 1)
	
	if ItemMaxSlotID.x > InventorySlotBounds.x:
		return false
	if ItemMaxSlotID.y > InventorySlotBounds.y:
		return false
		
	if InventoryItems.has(item):
		remove_item_in_inventory_slot(item, InventoryItems[item])
	
	for y_ctr in range(ItemSlotSize.y):
		for x_ctr in range(ItemSlotSize.x):
			InventoryItemslots[Vector2(slotID.x + x_ctr, slotID.y + y_ctr)] = item
			
	InventoryItems[item] = slotID
	return true

func remove_item_in_inventory_slot(item: Control, ExistingSlotID: Vector2) -> void:
	var ItemSlotSize: Vector2 = item.size / TileSize
	
	for y_Ctr in range(ItemSlotSize.y):
		for x_Ctr in range(ItemSlotSize.x):
			if InventoryItemslots.has(Vector2(ExistingSlotID.x + x_Ctr, ExistingSlotID.y + y_Ctr)):
				InventoryItemslots.erase(Vector2(ExistingSlotID.x + x_Ctr, ExistingSlotID.y + y_Ctr))
