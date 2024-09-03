using Godot;
using Godot.Collections;
using Test.Entities.Helpers;
using Test.Scripts.Components;

public partial class Inventory : Control
{
    [Export] public Vector2I TileSize = new(64, 64);
    [Export] public Vector2I InventorySize = new(8, 4);
    [Export] public PackedScene DummyItem;

    public int SelectedItemZIndex = 1000;

    public ColorRect Backgroud;
    public InventoryGrid Grid;
    public Area2D InventoryArea;

    public Godot.Collections.Dictionary<Vector2I, InventoryItemUI> PositionsItems = new();
    public Godot.Collections.Dictionary<InventoryItemUI, Vector2I> ItemsPositions = new();

    public bool IsOpen;
    private bool _isItemSelected;
    private bool _isSelectedItemInInventory;
    private bool _isDraggingItem;
    private bool _isSelectedItemOverlapping;

    private Timer _dragTimer = new();
    private int[,] _inventoryMatrix = new int[0, 0];

    private InventoryItemUI _selectedItem;
    private Array<InventoryItemUI> _overlappingItems = new();

    private Vector2 _previousItemPosition;
    private InventoryItemUI _previousItem;

    private Vector2I _minInventoryBounds = new();
    private Vector2I _maxInventoryBounds = new();

    private Color _invalidColor = new(1, 0.36f, 0.36f);
    private Color _validColor = new(1, 1, 1);

    public override void _Ready()
    {
        Close();
        _dragTimer.OneShot = true;
        AddChild(_dragTimer);

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

        foreach (var node in GetTree().GetNodesInGroup("item"))
        {
            if (node is Control i) ConnectSignals(i);
        }

        _minInventoryBounds = (Vector2I)((Grid.GlobalPosition / TileSize) - Vector2.One);
        _maxInventoryBounds =
            (Vector2I)((InventorySize + (InventorySize * Globals.GridPadding) / TileSize) + new Vector2(2, 2));
        _inventoryMatrix = new int[_maxInventoryBounds.Y - 1, _maxInventoryBounds.X - 1];
        _inventoryMatrix.Initialize();
    }

    public override async void _Process(double delta)
    {
        if (Input.IsActionJustPressed("inventory"))
        {
            if (IsOpen) Close();
            else Open();
        }

        // TODO: make the selected item rotate around it's center (Optional)
        if (Input.IsActionJustPressed("interact") && _isItemSelected)
        {
            var oldPivot = _selectedItem.PivotOffset;
            _selectedItem.PivotOffset = _selectedItem.Size / 2;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            _selectedItem.Rotation += Mathf.DegToRad(90);
            GD.Print($"old:{oldPivot} new: {_selectedItem.PivotOffset}, rotation: {_selectedItem.RotationDegrees}");
        }

        if (Input.IsActionJustPressed("cancel"))
        {
            foreach (var node in Globals.Inventory.GetChildren())
            {
                if (node is InventoryItemUI i)
                {
                    PlaceItem(i);
                }
            }
        }

        if (Input.IsActionJustPressed("test"))
        {
            PrintMatrix(false);
        }

        if (Input.IsActionJustPressed("reload"))
        {
            GD.PrintRich("[color=cyan]INSTACIATED");
            var newInstance = DummyItem.Instantiate<InventoryItemUI>();
            newInstance.GlobalPosition = new Vector2(_minInventoryBounds.X, _minInventoryBounds.Y).ToGlobalSpaceSnapped();
            AddChild(newInstance);

            PlaceItem(newInstance);
        }

        if (_isItemSelected || _isDraggingItem)
        {
            var globalPos = new Vector2();
            if (_isSelectedItemInInventory)
                globalPos =
                    (this.GetGlobalMousePosition() - (_selectedItem.Size / 2)).Snapped(TileSize + new Vector2I(4, 4));
            else
                globalPos = (this.GetGlobalMousePosition() - (_selectedItem.Size / 2));

            _selectedItem.SetGlobalPosition(globalPos);
        }
    }

    public void Close()
    {
        Visible = false;
        IsOpen = false;
    }

    public void Open()
    {
        Visible = true;
        IsOpen = true;
    }

    public void ConnectSignals(Node node)
    {
        if (node is InventoryItemUI item)
        {
            item.GuiInput += @event => OnCursorOnItem(@event, item);
            item.GetNode<Area2D>("Area2D").AreaEntered += area => OnOverlapping(area, item);
            item.GetNode<Area2D>("Area2D").AreaExited += area => OnNotOverlapping(area, item);
        }
    }

    public void DisconnectSignals(InventoryItemUI item)
    {
        if (item.IsConnected(Control.SignalName.GuiInput, new Callable(this, MethodName.OnCursorOnItem)))
            item.GuiInput -= @event => OnCursorOnItem(@event, item);

        if (item.GetNode<Area2D>("Area2D")
            .IsConnected(Area2D.SignalName.AreaEntered, new Callable(this, MethodName.OnOverlapping)))
            item.GetNode<Area2D>("Area2D").AreaEntered -= area => OnOverlapping(area, item);

        if (item.GetNode<Area2D>("Area2D")
            .IsConnected(Area2D.SignalName.AreaExited, new Callable(this, MethodName.OnNotOverlapping)))
            item.GetNode<Area2D>("Area2D").AreaExited -= area => OnNotOverlapping(area, item);
    }

    private void OnOverlapping(Area2D area, InventoryItemUI item)
    {
        // GD.Print("Overlap");

        if (area.GetParent().GetParent<Control>() == _selectedItem)
            return;

        if (area == InventoryArea)
            return;

        _overlappingItems.Add(item);

        if (_selectedItem != null)
        {
            _isSelectedItemOverlapping = true;
            _selectedItem.GetNode<Sprite2D>("Sprite2D").Modulate = _invalidColor;
            _selectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = _invalidColor;
        }
    }

    private void OnNotOverlapping(Area2D area, InventoryItemUI item)
    {
        // GD.Print("no ovelap");

        if (area.GetParent().GetParent<Control>() == _selectedItem)
            return;

        if (area == InventoryArea)
            return;

        _overlappingItems.Remove(item);

        if (_overlappingItems.Count == 0 && _isItemSelected)
        {
            _isSelectedItemOverlapping = false;
            _selectedItem.GetNode<Sprite2D>("Sprite2D").Modulate = _validColor;
            _selectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = _validColor;
        }
    }

    public void OnCursorOnItem(InputEvent @event, InventoryItemUI item)
    {
        if (@event.IsActionPressed("select_item") && !_isItemSelected)
        {
            SelectItem(item);
            _dragTimer.Start(0.1);
            GD.Print("\nitem selected");
        }

        if (@event is InputEventMouseMotion)
            if (_isItemSelected)
                _isDraggingItem = true;


        if (Input.IsActionJustPressed("select_item") && _isItemSelected && _dragTimer.IsStopped())
        {
            // GD.Print($"placed: {_selectedItem.Name}, onPrev: {_previousItemPosition / TileSize}, onCurr : {_selectedItem.GlobalPosition}");
            if (IsInsideBounds(_selectedItem, _selectedItem.GlobalPosition.ToTileSpace()) && IsOutsideOtherItems(_selectedItem, _selectedItem.GlobalPosition.ToTileSpace()))
                AddInsideItem(_selectedItem, _selectedItem.GlobalPosition.ToTileSpace());
        }
    }

    private void SelectItem(InventoryItemUI item)
    {
        _isItemSelected = true;
        _selectedItem = item;
        _selectedItem.GetNode<Sprite2D>("Sprite2D").ZIndex = SelectedItemZIndex;
        _selectedItem.GetNode<NinePatchRect>("NinePatchRect").ZIndex = SelectedItemZIndex;

        _previousItemPosition = _selectedItem.GlobalPosition;
        _previousItem = _selectedItem;

        ClearMatrixPosition(item, item.GlobalPosition.ToTileSpace());
        if (_previousItem == item)
            ClearMatrixPosition(item, _previousItem.GlobalPosition.ToTileSpace());
    }

    private void AddInsideItem(InventoryItemUI item, Vector2I position)
    {
        var itemMinPosition = position;
        var itemMaxPosition = itemMinPosition + item.ItemSize - Vector2.One;

        GD.Print($"SlotSize: {item.ItemSize}");
        GD.Print($"MinSlotId: {itemMinPosition}");
        GD.Print($"MaxSlotId: {itemMaxPosition}");
        GD.Print($"SlotBounds: max {_maxInventoryBounds}min {_minInventoryBounds}");

        if (ItemsPositions.ContainsKey(item))
            RemoveItem(item, ItemsPositions[item]);

        // UpdateMatrix();

        FillMatrixPosition(item, position);

        // PrintMatrix(false);

        ItemsPositions[item] = itemMinPosition;
        PositionsItems[itemMinPosition] = item;

        _selectedItem.GetNode<Sprite2D>("Sprite2D").ZIndex = 0;
        _selectedItem.GetNode<NinePatchRect>("NinePatchRect").ZIndex = 0;

        _selectedItem.GetNode<Sprite2D>("Sprite2D").Modulate = _validColor;
        _selectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = _validColor;

        _isItemSelected = false;
        _isDraggingItem = false;
        _selectedItem = null;

        // GD.Print("Item Added\n", ItemsPositions, "\n", PositionsItems);
    }

    private void UpdateMatrix()
    {
        _inventoryMatrix.ClearMatrix();
        FillAllMatrixPositions();
    }

    private void FillMatrixPosition(InventoryItemUI item, Vector2I position)
    {
        var itemMinPosition = position;
        var itemMaxPosition = itemMinPosition + item.ItemSize - Vector2.One;

        for (var i = itemMinPosition.X - _minInventoryBounds.X; i < itemMaxPosition.X; i++)
        {
            for (var j = itemMinPosition.Y - _minInventoryBounds.Y; j < itemMaxPosition.Y; j++)
            {
                // GD.Print($"add mat idx:{i}, {j}");
                _inventoryMatrix[(int)j, (int)i] = 1;
            }
        }
    }

    private void FillAllMatrixPositions()
    {
        foreach (var pair in ItemsPositions)
        {
            FillMatrixPosition(pair.Key, pair.Value);
        }
    }

    private void ClearMatrixPosition(InventoryItemUI item, Vector2I position)
    {
        var itemMinPosition = position;
        var itemMaxPosition = itemMinPosition + item.ItemSize - Vector2.One;

        if (item != _previousItem) return;

        for (var i = itemMinPosition.X - _minInventoryBounds.X; i < itemMaxPosition.X; i++)
        {
            for (var j = itemMinPosition.Y - _minInventoryBounds.Y; j < itemMaxPosition.Y; j++)
            {
                // GD.Print($"clr mat idx:{i}, {j}");
                _inventoryMatrix[(int)j, (int)i] = 0;
            }
        }
    }

    private void PrintMatrix(bool isRaw = true)
    {
        if (isRaw)
        {
            for (int i = 0; i < _inventoryMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < _inventoryMatrix.GetLength(1); j++)
                {
                    GD.PrintRaw($"{_inventoryMatrix[i, j]} ");
                }
                GD.PrintRaw("\n");
            }
        }
        else
        {
            for (int i = 0; i < _inventoryMatrix.GetLength(0); i++)
            {
                var row = new Array<int>();
                for (int j = 0; j < _inventoryMatrix.GetLength(1); j++)
                {
                    row.Add(_inventoryMatrix[i, j]);
                }
                GD.Print(row);
            }
        }
    }

    // TODO: while dragging an object if clicked outside inventory borders object should be thrown in the enviroment
    private void RemoveItem(InventoryItemUI item, Vector2I position)
    {
        var itemMinPosition = position;
        var itemMaxPosition = itemMinPosition + item.ItemSize - Vector2.One;

        if (PositionsItems.ContainsKey(position))
        {
            PositionsItems.Remove(position);
        }

        // GD.Print("Item Removed\n", PositionsItems);
    }

    // TODO: add the functionality to add objects from outside the inventory
    // TODO: make the item to be added inside the grid
    // algo steps:
    // 1.save item size 
    // 2.find a slot where the item can fit
    // 3.put item into it's place
    // 4.add the item to the dict
    public void AddOutsideItem()
    {
    }

    // TODO: fix the weird behavior of the tall objects
    // it has plus one on the upper y and minus one on the lower one
    // same with the x axis

    // store the wanted positon
    // compare to the whole matrix 
    // if not occupied, place item, else add one to x if exhaust x, add one to y
    private void PlaceItem(InventoryItemUI item)
    {
        if (_selectedItem == null)
            SelectItem(item);
        else return;

        for (int j = _minInventoryBounds.Y + 1; j < _maxInventoryBounds.Y; j++)
        {
            for (int i = _minInventoryBounds.X + 1; i < _maxInventoryBounds.X; i++)
            {
                var newPosition = new Vector2I(i, j);
                var newPositionSnapped = new Vector2I(i, j).ToGlobalSpaceSnapped();

                if (_selectedItem != null && CanPlace(item, newPosition))
                {
                    GD.Print($"inv idx: {i}, {j}, newPosTile: {newPosition}, newPosGlo: {newPositionSnapped}");
                    _selectedItem.GlobalPosition = newPositionSnapped;
                    AddInsideItem(item, newPosition);
                    GD.PrintRich("[color=magenta]PLACED");
                    break;
                }
            }
        }
    }

    private bool IsOutsideOtherItems(InventoryItemUI item, Vector2I position)
    {
        var itemMinPosition = position;
        var itemMaxPosition = position + item.ItemSize - Vector2I.One;

        for (var i = itemMinPosition.X - _minInventoryBounds.X; i < itemMaxPosition.X; i++)
        {
            for (var j = itemMinPosition.Y - _minInventoryBounds.Y; j < itemMaxPosition.Y; j++)
            {
                if (_inventoryMatrix[(int)j, (int)i] == 1) return false;
            }
        }

        return true;
    }

    private bool IsInsideBounds(InventoryItemUI item, Vector2I position)
    {
        var itemMinPosition = position;
        var itemMaxPosition = itemMinPosition + item.ItemSize - Vector2.One;

        if (itemMaxPosition.X < _minInventoryBounds.X || itemMaxPosition.Y < _minInventoryBounds.Y)
            return false;
        if (itemMaxPosition.X > _maxInventoryBounds.X || itemMaxPosition.Y > _maxInventoryBounds.Y)
            return false;
        if (itemMinPosition.X < _minInventoryBounds.X || itemMinPosition.Y < _minInventoryBounds.Y)
            return false;
        if (itemMinPosition.X > _maxInventoryBounds.X || itemMinPosition.Y > _maxInventoryBounds.Y)
            return false;

        return true;
    }

    private bool CanPlace(InventoryItemUI item, Vector2I position)
    {
        return IsInsideBounds(item, position) && IsOutsideOtherItems(item, position);
    }

    private void OnItemOutsideInventory(Area2D area)
    {
        // GD.Print("Inside");

        _isSelectedItemInInventory = false;
    }

    private void OnItemInsideInventory(Area2D area)
    {
        // GD.Print("Outside");

        _isSelectedItemInInventory = true;
    }
}
