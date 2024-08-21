using Godot;
using Godot.Collections;

public partial class Inventory : Control
{
	[Export] public Vector2 TileSize = new(64, 64);
	[Export] public Vector2 InventorySize = new(8, 4);
	[Export] public int SelectedItemZIndex = 1000;
	[Export] public int BackgroundPadding = 4;

	public ColorRect Backgroud;
	public InventoryGrid Grid;
	public Area2D InventoryArea;

	public Dictionary<Vector2, Control> InventoryItemSlots;
	public Dictionary<Control, Vector2> InventoryItems;

	private Vector2 _previousMousePos = new();
	private Vector2 _currentMouse;
	private Vector2 _posDelta;

	private bool _isItemSelected;
	private bool _isSelectedItemInInventory;
	private bool _isDraggingItem;
	private bool _isOpen;

	private Control SelectedItem;
	private Array<Control> OverlappingItems = new();
	private Vector2 ItemPreviewPosition;

	private Color InvalidColor = new(1, 0.36f, 0.36f, 1);
	private Color ValidColor = new(1, 1, 1, 1);

	public override void _Ready()
	{
		Close();
		Backgroud = GetNode<ColorRect>("Background");
		Grid = GetNode<InventoryGrid>("InventoryGrid");
		InventoryArea = GetNode<Area2D>("InventoryGrid/InventoryArea");

		Grid.PlaceTiles(InventorySize, TileSize);

		// TODO: fix the paddig in between tiles in the grid
		Backgroud.SetDeferred(ColorRect.PropertyName.Size, TileSize * InventorySize + new Vector2(BackgroundPadding * (InventorySize.X + 1), BackgroundPadding * (InventorySize.Y + 1)));
		Backgroud.GlobalPosition -= new Vector2(BackgroundPadding, BackgroundPadding);

		// Grid.RegionEnabled = true;
		// Grid.RegionRect = new(0, 0,
		// 		InventorySize.X * TileSize.X,
		// 		TileSize.Y * InventorySize.Y);
		InventoryArea.AreaEntered += OnItemInsideInventory;
		InventoryArea.AreaExited += OnItemOutsideInventory;

		foreach (var node in GetTree().GetNodesInGroup("item"))
		{
			if (node is Control i) ConnectSignals(i);
		}
		_previousMousePos = this.GetGlobalMousePosition();
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("inventory"))
		{
			if (_isOpen) Close();
			else Open();
		}
		if (_posDelta != Vector2.Zero || _previousMousePos != _currentMouse) _previousMousePos = _currentMouse;  

		_currentMouse = this.GetGlobalMousePosition();
		_posDelta = _currentMouse - _previousMousePos;

		// TODO: fix the x weirdness
		if (_isDraggingItem)
		{
			var globalPos = (_currentMouse - (SelectedItem.Size / 2)).Snapped(TileSize + new Vector2I(4, 4));
			SelectedItem.GlobalPosition = globalPos;
			if (_posDelta != Vector2.Zero)
				GD.Print($"curr {_currentMouse}\n prev {_previousMousePos}\n delta {_posDelta}\n actual {SelectedItem.GlobalPosition}\n xDelta {_currentMouse.X - _previousMousePos.X}");
		}
	}

	public void Close()
	{
		Visible = false;
		_isOpen = false;
	}

	public void Open()
	{
		Visible = true;
		_isOpen = true;
	}

	public void ConnectSignals(Control item)
	{
		item.GuiInput += @event => OnCursorOnItem(@event, item);
		item.GetNode<Area2D>("NinePatchRect/Sprite2D/Area2D").AreaEntered += area => OnOverlapping(area, item);
		item.GetNode<Area2D>("NinePatchRect/Sprite2D/Area2D").AreaExited += area => OnNotOverlapping(area, item);
	}

	public void DisconnectSignals(Control item)
	{
		if (item.IsConnected(Control.SignalName.GuiInput, new Callable(this, MethodName.OnCursorOnItem)))
			item.GuiInput -= @event => OnCursorOnItem(@event, item);

		if (item.GetNode<Area2D>("NinePakktchRect/Sprite2D/Area2D")
				.IsConnected(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnOverlapping)))
			item.GetNode<Area2D>("NinePatchRect/Sprite2D/Area2D").AreaEntered -= area => OnOverlapping(area, item);

		if (item.GetNode<Area2D>("NinePatchRect/Sprite2D/Area2D")
				.IsConnected(Area2D.SignalName.AreaExited, new Callable(this, MethodName.OnNotOverlapping)))
			item.GetNode<Area2D>("NinePatchRect/Sprite2D/Area2D").AreaExited -= area => OnNotOverlapping(area, item);
	}

	private void OnOverlapping(Area2D area, Control item)
	{
		GD.Print("Overlap");

		if (area.GetParent().GetParent<Control>() == SelectedItem)
			return;

		if (area == InventoryArea)
			return;

		OverlappingItems.Add(item);

		if (SelectedItem != null)
		{
			SelectedItem.GetNode<Sprite2D>("NinePatchRect/Sprite2D").Modulate = InvalidColor;
			SelectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = InvalidColor;
		}
	}

	private void OnNotOverlapping(Area2D area, Control item)
	{
		GD.Print("no ovelap");

		if (area.GetParent().GetParent<Control>() == SelectedItem)
			return;

		if (area == InventoryArea)
			return;

		OverlappingItems.Remove(item);

		if (OverlappingItems.Count == 0 && _isItemSelected)
		{
			SelectedItem.GetNode<Sprite2D>("NinePatchRect/Sprite2D").Modulate = ValidColor;
			SelectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = ValidColor;
		}
	}

	public void OnCursorOnItem(InputEvent @event, Control item)
	{
		if (@event.IsAction("select_item"))
		{
			_isItemSelected = true;
			SelectedItem = item;
			SelectedItem.GetNode<Sprite2D>("NinePatchRect/Sprite2D").ZIndex = SelectedItemZIndex;
			SelectedItem.GetNode<NinePatchRect>("NinePatchRect").ZIndex = SelectedItemZIndex;
			ItemPreviewPosition = SelectedItem.GlobalPosition;
			_isDraggingItem = /* _isDraggingItem == true ? false : */ true;
			GD.Print("\nitem selected");
		}

		// if (@event is InputEventMouseMotion)
		//     if (_isItemSelected)

		if (@event.IsActionReleased("select_item"))
		{
			SelectedItem.GetNode<Sprite2D>("NinePatchRect/Sprite2D").ZIndex = 0;
			SelectedItem.GetNode<NinePatchRect>("NinePatchRect").ZIndex = 0;

			if (OverlappingItems.Count == 0)
			{
				GD.Print("placed");
				// SelectedItem.GlobalPosition = ItemPreviewPosition;
				// SelectedItem.GetNode<Sprite2D>("NinePatchRect/Sprite2D").Modulate = ValidColor;
				// SelectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = ValidColor;
			}
			else
			{
				if (_isSelectedItemInInventory)
					if (!AddItemToInventory(SelectedItem))
					{
						// SelectedItem.GlobalPosition = ItemPreviewPosition;
						GD.Print("placedAddInv");
					}

			}
			_isItemSelected = false;
			_isDraggingItem = false;
			SelectedItem = null;
		}
	}

	private bool AddItemToInventory(Control item)
	{
		var slotId = item.GlobalPosition / TileSize;
		var itemSlotSize = item.Size / TileSize;

		GD.Print($"SlotId: {slotId}");
		GD.Print($"SlotSize: {itemSlotSize}");

		var itemMaxSlotId = slotId + itemSlotSize - new Vector2(1, 1);
		var inventorySlotBounds = InventorySize - new Vector2(1, 1);

		if (itemMaxSlotId.X > inventorySlotBounds.X)
			return false;
		if (itemMaxSlotId.Y > inventorySlotBounds.Y)
			return false;

		if (InventoryItems.ContainsKey(item))
			RemoveItemFromInventory(item, InventoryItems[item]);

		foreach (var x_ctr in GD.Range((int)itemSlotSize.X))
		{
			foreach (var y_ctr in GD.Range((int)itemSlotSize.Y))
			{
				var keyPos = new Vector2(slotId.X + x_ctr, slotId.Y + y_ctr);
				InventoryItemSlots[keyPos] = item;
			}
		}

		InventoryItems[item] = slotId;
		GD.Print("Item Added");

		return true;
	}

	private void RemoveItemFromInventory(Control item, Vector2 slotId)
	{
		var itemSlotSize = item.Size / TileSize;

		GD.Print("Item Removed");

		foreach (var x_ctr in GD.Range((int)itemSlotSize.X))
		{
			foreach (var y_ctr in GD.Range((int)itemSlotSize.Y))
			{
				var keyPos = new Vector2(slotId.X + x_ctr, slotId.Y + y_ctr);
				if (InventoryItemSlots.ContainsKey(keyPos))
					InventoryItemSlots.Remove(keyPos);
			}
		}
	}

	private void OnItemOutsideInventory(Area2D area)
	{
		GD.Print("Inside");

		_isSelectedItemInInventory = false;
	}

	private void OnItemInsideInventory(Area2D area)
	{

		GD.Print("Outside");

		_isSelectedItemInInventory = true;
	}

}
