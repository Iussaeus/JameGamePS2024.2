using Godot;
using Godot.Collections;
using Test.Entities.Helpers;
using Test.Scripts.Components;

// WARNING: weird behaviour when adding an object whilst other one is selected
public partial class Inventory : Control {
	[Export] public Vector2I TileSize = new(64, 64);
	[Export] public Vector2I InventorySize = new(8, 4);
	[Export] public PackedScene DummyItem;

	public int SelectedItemZIndex = 1000;

	public ColorRect Backgroud;
	public InventoryGrid Grid;
	public Area2D InventoryArea;

	public Dictionary<InventoryItemUI, Vector2I> ItemsPositions = new();

	public bool IsOpen;
	private bool _isItemSelected;
	private bool _isSelectedItemInInventory;
	private bool _isDraggingItem;
	private bool _isSelectedItemOverlapping;

	private Timer _dragTimer = new();
	private Timer _selectTimer = new();
	private int[,] _inventoryMatrix = new int[0, 0];

	private InventoryItemUI _selectedItem;
	private Array<InventoryItemUI> _overlappingItems = new();

	private Vector2 _previousItemPosition;
	private InventoryItemUI _previousItem;

	private Vector2I _minInventoryBounds = new();
	private Vector2I _maxInventoryBounds = new();

	private Color _invalidColor = new(1, 0.36f, 0.36f);
	private Color _validColor = new(1, 1, 1);

	public override void _Ready() {
		Close();
		_dragTimer.OneShot = true;
		_selectTimer.OneShot = true;
		AddChild(_dragTimer);
		AddChild(_selectTimer);

		ChildEnteredTree += ConnectSignals;
		Globals.Instance.EmitSignal(Globals.SignalName.InventorySpawned, TileSize, InventorySize, this);

		Backgroud = GetNode<ColorRect>("Background");
		Grid = GetNode<InventoryGrid>("InventoryGrid");
		InventoryArea = GetNode<Area2D>("InventoryGrid/InventoryArea");

		Grid.PlaceTiles(InventorySize, TileSize);

		var sizeWithPadding = TileSize * InventorySize +
							  new Vector2(Globals.GridPadding * (InventorySize.X + 1),
								  Globals.GridPadding * (InventorySize.Y + 1));

		Backgroud.SetDeferred(Control.PropertyName.Size, sizeWithPadding);
		Backgroud.GlobalPosition -= new Vector2(Globals.GridPadding, Globals.GridPadding);

		var inventoryRectangle = new RectangleShape2D();
		inventoryRectangle.Size = TileSize * (InventorySize - Vector2.One) +
								  (Globals.GridPadding * (InventorySize - Vector2.One));

		InventoryArea.GetNode<CollisionShape2D>("CollisionShape2D").Shape = inventoryRectangle;
		InventoryArea.GetNode<CollisionShape2D>("CollisionShape2D").Position = sizeWithPadding / 2;
		InventoryArea.GlobalPosition = Grid.GlobalPosition + Grid.Size * 2;
		InventoryArea.AreaEntered += OnItemInsideInventory;
		InventoryArea.AreaExited += OnItemOutsideInventory;

		foreach (var node in GetTree().GetNodesInGroup("item")) {
			if (node is Control i) ConnectSignals(i);
		}

		_minInventoryBounds = Grid.GlobalPosition.ToTileSpace();
		_maxInventoryBounds = _minInventoryBounds + InventorySize - Vector2I.One;

		_inventoryMatrix = new int[InventorySize.Y, InventorySize.X];
		_inventoryMatrix.Initialize();
		// GD.Print($"matSize: {_inventoryMatrix.GetLength(0)}, {_inventoryMatrix.GetLength(1)}");

#if DEBUG
		var testVecUpLeft = new Vector2I(_minInventoryBounds.X, _minInventoryBounds.Y);
		var testVecDownRight = new Vector2I(_minInventoryBounds.X, _maxInventoryBounds.Y);
		var testVecUpRight = new Vector2I(_maxInventoryBounds.X, _minInventoryBounds.Y);
		var testVecDownLeft = new Vector2I(_maxInventoryBounds.X, _maxInventoryBounds.Y);

		var testItem = DummyItem.Instantiate<InventoryItemUI>();

		// IsItemInsideBounds tests
		this.Assert(IsItemInsideBounds(testItem, testVecUpLeft), "isItemInsideBounds failed the upper left bound");
		this.Assert(IsItemInsideBounds(testItem, testVecUpRight), "isItemInsideBounds failed the upper right bound");
		this.Assert(IsItemInsideBounds(testItem, testVecDownLeft), "isItemInsideBounds failed the down left bound");
		this.Assert(IsItemInsideBounds(testItem, testVecDownRight), "isItemInsideBounds failed the down right bound");

		this.Assert(!IsItemInsideBounds(testItem, testVecUpLeft - Vector2I.One), "isItemInsideBounds succeeded the wrong down left bound");
		this.Assert(!IsItemInsideBounds(testItem, testVecUpRight + Vector2I.One), "isItemInsideBounds succeeded the wrong down right bound");
		this.Assert(!IsItemInsideBounds(testItem, testVecDownLeft + new Vector2I(-1, 1)), "isItemInsideBounds succeeded the wrong upper left bound");
		this.Assert(!IsItemInsideBounds(testItem, testVecDownRight - Vector2I.One), "isItemInsideBounds succeeded the wrong upper right bound");

		this.Assert(!IsItemInsideBounds(testItem, testVecUpLeft with { X = testVecUpLeft.X - 1, Y = _maxInventoryBounds.Y / 2 }), "isItemInsideBounds failed the middle left bound");
		this.Assert(!IsItemInsideBounds(testItem, testVecUpRight with { X = _maxInventoryBounds.X / 2, Y = testVecUpRight.Y - 1 }), "isItemInsideBounds failed the middle up bound");
		this.Assert(!IsItemInsideBounds(testItem, testVecDownLeft with { X = _maxInventoryBounds.X / 2, Y = _maxInventoryBounds.Y + 1 }), "isItemInsideBounds failed the middle right bound");
		this.Assert(!IsItemInsideBounds(testItem, testVecDownRight with { X = _maxInventoryBounds.X + 1, Y = _maxInventoryBounds.Y / 2 }), "isItemInsideBounds failed the middle down bound");

		// IsTileInsideBounds tests
		this.Assert(IsTileInsideBounds(testVecUpLeft), "isTileInsideBounds failed the upper left bound");
		this.Assert(IsTileInsideBounds(testVecUpRight), "isTileInsideBounds failed the upper right bound");
		this.Assert(IsTileInsideBounds(testVecDownLeft), "isTileInsideBounds failed the down left bound");
		this.Assert(IsTileInsideBounds(testVecDownRight), "isTileInsideBounds failed the down right bound");

		this.Assert(!IsTileInsideBounds(testVecUpLeft - Vector2I.One), "isTileInsideBounds succeeded the wrong down left bound");
		this.Assert(!IsTileInsideBounds(testVecUpRight + Vector2I.One), "isTileInsideBounds succeeded the wrong down right bound");
		this.Assert(!IsTileInsideBounds(testVecDownLeft + new Vector2I(-1, 1)), "isTileInsideBounds succeeded the wrong upper left bound");
		this.Assert(!IsTileInsideBounds(testVecDownRight - Vector2I.One), "isTileInsideBounds succeeded the wrong upper right bound");

		this.Assert(!IsTileInsideBounds(testVecUpLeft with { X = testVecUpLeft.X - 1, Y = _maxInventoryBounds.Y / 2 }), "isTileInsideBounds failed the middle left bound");
		this.Assert(!IsTileInsideBounds(testVecUpRight with { X = _maxInventoryBounds.X / 2, Y = testVecUpRight.Y - 1 }), "isTileInsideBounds failed the middle up bound");
		this.Assert(!IsTileInsideBounds(testVecDownLeft with { X = _maxInventoryBounds.X / 2, Y = _maxInventoryBounds.Y + 1 }), "isTileInsideBounds failed the middle right bound");
		this.Assert(!IsTileInsideBounds(testVecDownRight with { X = _maxInventoryBounds.X + 1, Y = _maxInventoryBounds.Y / 2 }), "isTileInsideBounds failed the middle down bound");

		// ToGlobalSnapped and ToTileSpace tests
		for (int i = 0; i < _maxInventoryBounds.X; i++) {
			for (int j = 0; j < _maxInventoryBounds.Y; j++) {
				var position = new Vector2(i, j);
				testItem.GlobalPosition = position.ToGlobalSpaceSnapped();
				this.Assert(testItem.GlobalPosition.ToTileSpace() == position, $"them positions dont match tilePos: {position}, gloPos: {testItem.GlobalPosition.ToTileSpace()}");
			}

		}

		// FillMatrix and ClearMatrix tests
		foreach (var node in GetChildren()) {
			if (node is InventoryItemUI i) {
				i.GlobalPosition = i.GlobalPosition.Snapped(TileSize + new Vector2I(4, 4));

				FillMatrixPosition(i, i.GlobalPosition.ToTileSpace());
				CheckMatrixPosition(i, i.GlobalPosition.ToTileSpace());
				ClearMatrixPosition(i, i.GlobalPosition.ToTileSpace());
				CheckMatrixPosition(i, i.GlobalPosition.ToTileSpace(), checkZero: true);
				AddItem(i, i.GlobalPosition.ToTileSpace());
			}
		}
#endif
	}

	public void PlaceDummyItem() {
		var newInstance = DummyItem.Instantiate<InventoryItemUI>();
		AddChild(newInstance);
		PlaceItem(newInstance);
	}

	public override void _Process(double delta) {
		if (Input.IsActionJustPressed("inventory")) {
			if (IsOpen) Close();
			else Open();

		}

		// // TODO: make the selected item rotate around it's center (Optional)
		// if (Input.IsActionJustPressed("interact") && _isItemSelected) {
		// 	var oldPivot = _selectedItem.PivotOffset;
		// 	_selectedItem.Rotation += Mathf.DegToRad(90);
		// 	GD.Print($"old:{oldPivot} new: {_selectedItem.PivotOffset}, rotation: {_selectedItem.RotationDegrees}");
		// }

#if DEBUG
		if (Input.IsActionJustPressed("cancel")) {
			foreach (var node in Globals.Inventory.GetChildren()) {
				if (node is InventoryItemUI i) {
					PlaceItem(i);
				}
			}
		}

		if (Input.IsActionJustPressed("test")) {
			PrintMatrix(false);
			// GD.Print(ItemsPositions);
		}

		if (Input.IsActionJustPressed("reload")) {
			PlaceDummyItem();
		}
#endif

		if (_isItemSelected || _isDraggingItem) {
			var globalPos = new Vector2();
			if (_isSelectedItemInInventory)
				globalPos =
					(this.GetGlobalMousePosition() - (_selectedItem.Size / 2)).Snapped(TileSize + new Vector2I(4, 4));
			else
				globalPos = (this.GetGlobalMousePosition() - (_selectedItem.Size / 2));

			_selectedItem.SetGlobalPosition(globalPos);
		}
	}

	public void Close() {
		Visible = false;
		IsOpen = false;

		if (_selectedItem != null && CanPlace(_selectedItem, _selectedItem.GlobalPosition.ToTileSpace())) {
			DeselectItem(); ;
		}
		else if (_selectedItem != null && !CanPlace(_selectedItem, _selectedItem.GlobalPosition.ToTileSpace()))
			PlaceItem(_selectedItem);
	}

	public void Open() {
		Visible = true;
		IsOpen = true;
	}

	public void ConnectSignals(Node node) {
		if (node is InventoryItemUI item) {
			item.GuiInput += @event => OnCursorOnItem(@event, item);
			item.GetNode<Area2D>("Area2D").AreaEntered += area => OnOverlapping(area, item);
			item.GetNode<Area2D>("Area2D").AreaExited += area => OnNotOverlapping(area, item);
		}
	}

	public void DisconnectSignals(InventoryItemUI item) {
		if (item.IsConnected(Control.SignalName.GuiInput, new Callable(this, MethodName.OnCursorOnItem)))
			item.GuiInput -= @event => OnCursorOnItem(@event, item);

		if (item.GetNode<Area2D>("Area2D")
			.IsConnected(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnOverlapping)))
			item.GetNode<Area2D>("Area2D").AreaEntered -= area => OnOverlapping(area, item);

		if (item.GetNode<Area2D>("Area2D")
			.IsConnected(Area2D.SignalName.AreaExited, new Callable(this, MethodName.OnNotOverlapping)))
			item.GetNode<Area2D>("Area2D").AreaExited -= area => OnNotOverlapping(area, item);
	}

	private void OnOverlapping(Area2D area, InventoryItemUI item) {
		// GD.Print("Overlap");

		if (area.GetParent().GetParent<Control>() == _selectedItem)
			return;

		if (area == InventoryArea)
			return;

		_overlappingItems.Add(item);

		if (_selectedItem != null) {
			_isSelectedItemOverlapping = true;
			_selectedItem.GetNode<Sprite2D>("Sprite2D").Modulate = _invalidColor;
			_selectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = _invalidColor;
		}
	}

	private void OnNotOverlapping(Area2D area, InventoryItemUI item) {
		// GD.Print("no ovelap");

		if (area.GetParent().GetParent<Control>() == _selectedItem)
			return;

		if (area == InventoryArea)
			return;

		_overlappingItems.Remove(item);

		if (_overlappingItems.Count == 0 && _isItemSelected) {
			_isSelectedItemOverlapping = false;
			_selectedItem.GetNode<Sprite2D>("Sprite2D").Modulate = _validColor;
			_selectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = _validColor;
		}
	}

	public void OnCursorOnItem(InputEvent @event, InventoryItemUI item) {

		if (!item.HasNode("InventoryItem3d"))
			_selectTimer.Stop();

		if (@event.IsActionPressed("select_item") && !_isItemSelected && _selectTimer.IsStopped()) {
			SelectItem(item);

			_dragTimer.Start(0.1);
		}

		if (@event is InputEventMouseMotion)
			if (_isItemSelected)
				_isDraggingItem = true;

		if (Input.IsActionJustPressed("select_item") && _isItemSelected && _dragTimer.IsStopped()) {
			if (CanPlace(_selectedItem, _selectedItem.GlobalPosition.ToTileSpace())) {
				GD.Print("Adding Item");
				AddItem(_selectedItem, _selectedItem.GlobalPosition.ToTileSpace());
			}
			else if (!_isSelectedItemInInventory) {
				GD.Print("throwing Item");
				ThrowItemOutside(_selectedItem);
			}
		}
	}

	// NOTE:: timer is a weird thing in this func
	// TODO: fix the select thing
	private void SelectItem(InventoryItemUI item) {
		// GD.PrintRich("[color=magenta]SELECTED");

		if (item.HasNode("InventoryItem3d"))
			_selectTimer.Start(2.5);

		_isItemSelected = true;
		_selectedItem = item;
		_selectedItem.GetNode<Sprite2D>("Sprite2D").ZIndex = SelectedItemZIndex;
		_selectedItem.GetNode<NinePatchRect>("NinePatchRect").ZIndex = SelectedItemZIndex;

		_previousItemPosition = _selectedItem.GlobalPosition;
		_previousItem = _selectedItem;

		ClearMatrixPosition(_selectedItem, _selectedItem.GlobalPosition.ToTileSpace());

		if (_previousItem == _selectedItem) {
			ClearMatrixPosition(_previousItem, _previousItem.GlobalPosition.ToTileSpace());
		}

		// GD.PrintRich("[color=magenta]END SELECTED");
		// GD.Print($"selected: {_selectedItem.Name}, onPrev: {_previousItemPosition.ToTileSpace()}, onCurr : {_selectedItem.GlobalPosition.ToTileSpace()}");
	}

	private void AddItem(InventoryItemUI item, Vector2I position) {
		// GD.PrintRich("[color=magenta]BEGIN ADD");
		if (!CanPlace(item, position)) return;

		if (_selectedItem == null) SelectItem(item);

		var itemMinPosition = position;
		var itemMaxPosition = itemMinPosition + item.ItemSize - Vector2.One;

		// GD.Print($"ItemSize: {item.ItemSize}");
		// GD.Print($"ItemMinPosition: {itemMinPosition}");
		// GD.Print($"ItemMaxPositon: {itemMaxPosition}");
		// GD.Print($"InventoryBounds: max {_maxInventoryBounds}, min {_minInventoryBounds}");

		if (ItemsPositions.ContainsKey(item))
			RemoveItem(item, ItemsPositions[item]);

		FillMatrixPosition(item, position);

		ItemsPositions[item] = itemMinPosition;
		// PositionsItems[itemMinPosition] = item;

		_selectedItem.GetNode<Sprite2D>("Sprite2D").ZIndex = 0;
		_selectedItem.GetNode<NinePatchRect>("NinePatchRect").ZIndex = 0;

		_selectedItem.GetNode<Sprite2D>("Sprite2D").Modulate = _validColor;
		_selectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = _validColor;

		DeselectItem();

		// GD.Print("Item Added\n", ItemsPositions, "\n");
		// GD.PrintRich("[color=magenta]END ADD");
	}

	// NOTE: never used 
	private void UpdateMatrix() {
		_inventoryMatrix.ClearMatrix();
		FillAllMatrixPositions();
	}

	private void FillMatrixPosition(InventoryItemUI item, Vector2I position) {
		var itemMinPosition = position - _minInventoryBounds;
		var itemMaxPosition = itemMinPosition + item.ItemSize;

		// GD.PrintRich($"[color=cyan] fill: minPos{itemMinPosition} maxPos{itemMaxPosition}");

		for (var i = itemMinPosition.X; i < itemMaxPosition.X; i++) {
			for (var j = itemMinPosition.Y; j < itemMaxPosition.Y; j++) {
				// GD.PrintS("Fill idx", i, j, IsTileInsideBounds(new Vector2I(i, j) + _minInventoryBounds));
				// GD.PrintRich($"[color=cyan]add mat idx:{i}, {j}");
				if (IsTileInsideBounds(new Vector2I(i, j) + _minInventoryBounds)) _inventoryMatrix[j, i] = 1;
			}
		}
	}

	// NOTE: never used
	private void FillAllMatrixPositions() {
		foreach (var pair in ItemsPositions) {
			FillMatrixPosition(pair.Key, pair.Value);
		}
	}

	private void ClearMatrixPosition(InventoryItemUI item, Vector2I position) {
		var itemMinPosition = position - _minInventoryBounds;
		var itemMaxPosition = itemMinPosition + item.ItemSize;

		// GD.PrintRich($"[color=cyan] clr: minPos{itemMinPosition} maxPos{itemMaxPosition}");

		for (var i = itemMinPosition.X; i < itemMaxPosition.X; i++) {
			for (var j = itemMinPosition.Y; j < itemMaxPosition.Y; j++) {
				// GD.PrintS("Clear idx", i, j, IsTileInsideBounds(new Vector2I(i, j) + _minInventoryBounds));
				// GD.PrintRich($"[color=cyan]clr mat idx:{i}, {j}");
				if (IsTileInsideBounds(new Vector2I(i, j) + _minInventoryBounds))
					_inventoryMatrix[j, i] = 0;
			}
		}
	}

#if DEBUG
	private void CheckMatrixPosition(InventoryItemUI item, Vector2I position, bool checkZero = false) {
		var itemMinPosition = position - _minInventoryBounds;
		var itemMaxPosition = itemMinPosition + item.ItemSize;

		// GD.PrintRich($"item: {item.Name}");
		// GD.PrintRich($"itemMinPos: {itemMinPosition}");
		// GD.PrintRich($"itemMaxPos: {itemMaxPosition}");

		for (var i = itemMinPosition.X; i < itemMaxPosition.X; i++) {
			for (var j = itemMinPosition.Y; j < itemMaxPosition.Y; j++) {
				// GD.PrintRich($"InvMat[{i}, {j}]: {_inventoryMatrix[j, i]}, isTileInside: {IsTileInsideBounds(new Vector2I(i, j) + _minInventoryBounds)}");
				if (!checkZero)
					this.Assert(_inventoryMatrix[j, i] == 1, $"invMat[{i}, {j}] is not 1 as it should");
				else this.Assert(_inventoryMatrix[j, i] == 0, $"invMat[{i}, {j}] is not 0 as it should");
			}
		}
		// PrintMatrix(isRaw: false);
	}
#endif

	private void PrintMatrix(bool isRaw = true) {
		if (isRaw) {
			for (int i = 0; i < _inventoryMatrix.GetLength(0); i++) {
				for (int j = 0; j < _inventoryMatrix.GetLength(1); j++) {
					GD.PrintRaw($"{_inventoryMatrix[i, j]} ");
				}
				GD.PrintRaw("\n");
			}
		}
		else {
			for (int i = 0; i < _inventoryMatrix.GetLength(0); i++) {
				var row = new Array<int>();
				for (int j = 0; j < _inventoryMatrix.GetLength(1); j++) {
					row.Add(_inventoryMatrix[i, j]);
				}
				GD.Print(row);
			}
		}
	}

	private void RemoveItem(InventoryItemUI item, Vector2I position) {
		if (ItemsPositions.ContainsKey(item)) {
			ClearMatrixPosition(item, position);
			ItemsPositions.Remove(item);
		}
		// GD.Print("Item Removed\n", PositionsItems);
	}

	public void PlaceItem(InventoryItemUI item) {
		// GD.PrintRich("[color=magenta]BEGING PLACED");

		if (_selectedItem == null) SelectItem(item);

		for (int j = _minInventoryBounds.Y; j < _maxInventoryBounds.Y + 1; j++) {
			for (int i = _minInventoryBounds.X; i < _maxInventoryBounds.X + 1; i++) {
				var newPosition = new Vector2I(i, j);
				var newPositionSnapped = new Vector2I(i, j).ToGlobalSpaceSnapped();

				if (_selectedItem != null && CanPlace(item, newPosition)) {
					// GD.Print($"inv idx: {i}, {j}, newPosTile: {newPosition}, newPosGlo: {newPositionSnapped}");
					_selectedItem.GlobalPosition = newPositionSnapped;
					AddItem(item, newPosition);
					// GD.PrintRich("[color=magenta]PLACED");
					break;
				}
			}
		}
	}

	private bool IsOutsideOtherItems(InventoryItemUI item, Vector2I position) {
		var itemMinPosition = position - _minInventoryBounds;
		var itemMaxPosition = itemMinPosition + item.ItemSize;

		// GD.PrintRich($"[color=yellow]item:{item.Name}");
		// GD.PrintRich($"[color=yellow]minPos: {itemMinPosition}, maxPos: {itemMaxPosition}");
		// GD.PrintRich($"[color=yellow]minBounds: {_minInventoryBounds}, maxBounds: {_maxInventoryBounds}");

		for (var i = itemMinPosition.X; i < itemMaxPosition.X; i++) {
			for (var j = itemMinPosition.Y; j < itemMaxPosition.Y; j++) {
				if (IsTileInsideBounds(new Vector2I(i, j) + _minInventoryBounds))
					if (_inventoryMatrix[j, i] == 1) {
						// GD.PrintRich($"[color=yellow]InvMat[{j}, {i}] = {_inventoryMatrix[j, i]}");
						return false;
					}
			}
		}

		return true;
	}

	private bool IsItemInsideBounds(InventoryItemUI item, Vector2I position) {
		var itemMinPosition = position;
		var itemMaxPosition = itemMinPosition + item.ItemSize - Vector2I.One;

		if (itemMinPosition.X < _minInventoryBounds.X || itemMinPosition.Y < _minInventoryBounds.Y) {
			// GD.PrintRich("[color=red]OUTSIDE");
			// GD.PrintRich($"[color=yellow]minPos: {itemMinPosition}");
			// GD.PrintRich($"[color=yellow]minBounds: {_minInventoryBounds}");
			return false;
		}
		if (itemMinPosition.X > _maxInventoryBounds.X || itemMinPosition.Y > _maxInventoryBounds.Y) {
			// GD.PrintRich("[color=red]OUTSIDE");
			// GD.PrintRich($"[color=yellow]minPos: {itemMinPosition}");
			// GD.PrintRich($"[color=yellow]maxBounds: {_maxInventoryBounds}");
			return false;
		}
		if (itemMaxPosition.X < _minInventoryBounds.X || itemMaxPosition.Y < _minInventoryBounds.Y) {
			// GD.PrintRich("[color=red]OUTSIDE");
			// GD.PrintRich($"[color=yellow]maxPos: {itemMaxPosition}");
			// GD.PrintRich($"[color=yellow]minBounds: {_minInventoryBounds}");
			return false;
		}
		if (itemMaxPosition.X > _maxInventoryBounds.X || itemMaxPosition.Y > _maxInventoryBounds.Y) {
			// GD.PrintRich("[color=red]OUTSIDE");
			// GD.PrintRich($"[color=yellow]maxPos: {itemMaxPosition}");
			// GD.PrintRich($"[color=yellow]maxBounds: {_maxInventoryBounds}");
			return false;
		}

		// GD.Print("[color=red]INSIDE");
		// GD.PrintRich($"[color=yellow]minPos: {itemMinPosition}, maxPos: {itemMaxPosition}");
		// GD.PrintRich($"[color=yellow]minBounds: {_minInventoryBounds}, maxBounds: {_maxInventoryBounds}");
		return true;
	}

	private bool IsTileInsideBounds(Vector2I position) {
		if (position.X < _minInventoryBounds.X || position.Y < _minInventoryBounds.Y) {
			return false;
		}
		if (position.X > _maxInventoryBounds.X || position.Y > _maxInventoryBounds.Y) {
			return false;
		}

		return true;
	}

	private bool IsWholeItemInsideBounds(InventoryItemUI item, Vector2I position) {
		var itemMinPosition = position;
		var itemMaxPosition = itemMinPosition + item.ItemSize - Vector2I.One;

		for (var i = itemMinPosition.X; i < itemMaxPosition.X; i++) {
			for (var j = itemMinPosition.Y; j < itemMaxPosition.Y; j++) {
				if (IsTileInsideBounds(new Vector2I(i, j))) continue;
				else return false;
			}
		}

		return true;
	}

	private bool CanPlace(InventoryItemUI item, Vector2I position) {
		// if (IsItemInsideBounds(item, position) && IsOutsideOtherItems(item, position))
		// 	GD.PrintRich("[color=red]CAN PLACE");
		// else GD.PrintRich("[color=red]CAN'T PLACE");
		return IsItemInsideBounds(item, position) && IsOutsideOtherItems(item, position);
	}

	// FIX: fix the weird long items behaviour
	// FIX: after throwing an item one it is needed to click one time to be able to selecte other items
	// FIX: check if the entire item is outside of the inventory
	// FIX: cant place item added from outside
	private void ThrowItemOutside(InventoryItemUI item) {
		if (!item.HasNode("InventoryItem3d"))
			return;

		if (ItemsPositions.ContainsKey(item)) {
			var position = ItemsPositions[item];
			RemoveItem(item, position);
		}

		var item3D = item.GetNode<InventoryItem3D>("InventoryItem3d");
		item.Reparent(Globals.World);
		item3D.Enable();
		item3D.GlobalPosition = Globals.Player.GlobalPosition + (-Globals.Player.Transform.Basis.Z * 2);
		item3D.ApplyImpulse(-Globals.Player.Transform.Basis.Z * 10);
	}

	private void DeselectItem() {
		_isItemSelected = false;
		_isDraggingItem = false;
		_selectedItem = null;
	}

	private void OnItemOutsideInventory(Area2D area) {
		if (_selectedItem == area.GetParent()) {
			GD.Print(area.GetParent().Name, " Outside");

			_isSelectedItemInInventory = false;
		}
	}

	private void OnItemInsideInventory(Area2D area) {
		if (_selectedItem == area.GetParent()) {
			GD.Print(area.GetParent().Name, " Inside");

			_isSelectedItemInInventory = true;
		}
	}
}
