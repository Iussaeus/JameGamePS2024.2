using Godot;
using Godot.Collections;
using Test.Scripts.Components;

public partial class Inventory : Control
{
    [Export] public Vector2 TileSize = new(64, 64);
    [Export] public Vector2 InventorySize = new(8, 4);
    [Export] public int SelectedItemZIndex = 1000;
    [Export] public int BackgroundPadding = 4;

    public ColorRect Backgroud;
    public InventoryGrid Grid;
    public Area2D InventoryArea;

    public Dictionary<Vector2, Control> InventoryItemSlots = new();
    public Dictionary<Control, Vector2> InventoryItems = new();

    private Timer _dragTimer = new();

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
        _dragTimer.OneShot = true;
        AddChild(_dragTimer);

        Globals.Instance.EmitSignal(Globals.SignalName.InventorySpawned, TileSize, InventorySize, this);

        Backgroud = GetNode<ColorRect>("Background");
        Grid = GetNode<InventoryGrid>("InventoryGrid");
        InventoryArea = GetNode<Area2D>("InventoryGrid/InventoryArea");

        Grid.PlaceTiles(InventorySize, TileSize);

        var sizeWithPadding = TileSize * InventorySize +
            new Vector2(Globals.GridPadding * (InventorySize.X + 1), Globals.GridPadding * (InventorySize.Y + 1));

        Backgroud.SetDeferred(ColorRect.PropertyName.Size, sizeWithPadding);
        Backgroud.GlobalPosition -= new Vector2(Globals.GridPadding, Globals.GridPadding);

        var inventoryRectangle = new RectangleShape2D();
        inventoryRectangle.Size = TileSize * (InventorySize - Vector2.One) + (Globals.GridPadding * (InventorySize - Vector2.One));

        InventoryArea.GetNode<CollisionShape2D>("CollisionShape2D").Shape = inventoryRectangle;
        InventoryArea.GetNode<CollisionShape2D>("CollisionShape2D").Position = sizeWithPadding / 2;
        InventoryArea.GlobalPosition = Grid.GlobalPosition + Grid.Size * 2;
        InventoryArea.AreaEntered += OnItemInsideInventory;
        InventoryArea.AreaExited += OnItemOutsideInventory;

        foreach (var node in GetTree().GetNodesInGroup("item"))
        {
            if (node is Control i) ConnectSignals(i);
        }
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("inventory"))
        {
            if (_isOpen) Close();
            else Open();
        }

        if (_isItemSelected || _isDraggingItem)
        {
            var globalPos = new Vector2();
            if (_isSelectedItemInInventory)
                globalPos = (this.GetGlobalMousePosition() - (SelectedItem.Size / 2)).Snapped(TileSize + new Vector2I(4, 4));
            else
                globalPos = (this.GetGlobalMousePosition() - (SelectedItem.Size / 2));

            SelectedItem.GlobalPosition = globalPos;
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
        if (@event.IsActionPressed("select_item") && !_isItemSelected)
        {
            _isItemSelected = true;
            SelectedItem = item;
            SelectedItem.GetNode<Sprite2D>("NinePatchRect/Sprite2D").ZIndex = SelectedItemZIndex;
            SelectedItem.GetNode<NinePatchRect>("NinePatchRect").ZIndex = SelectedItemZIndex;
            ItemPreviewPosition = SelectedItem.GlobalPosition;
            _dragTimer.Start(0.1);
            GD.Print("\nitem selected");
        }

        if (@event is InputEventMouseMotion)
            if (_isItemSelected)
                _isDraggingItem = true;

        if (Input.IsActionJustPressed("select_item") && _isItemSelected && _dragTimer.IsStopped())
        {
            if (OverlappingItems.Count == 0)
            {
                GD.Print("placed");
                AddItemToInventory(SelectedItem);
            }
            else
            {
                GD.Print("cant place");
            }
        }
    }

    private void AddItemToInventory(Control item)
    {
        var itemMinSlotId = (Vector2I)((item.GlobalPosition) / TileSize);
        var itemSlotSize = (Vector2I)(item.Size / TileSize);

        var itemMaxSlotId = (Vector2I)(itemMinSlotId + itemSlotSize - Vector2.One);
        var minInventorySlotBounds = (Vector2I)((Grid.GlobalPosition / TileSize ) - Vector2.One);
        var maxInventorySlotBounds = (Vector2I)(InventorySize + ((InventorySize * Globals.GridPadding) / TileSize) + new Vector2(2, 2));

        GD.Print($"SlotSize: {itemSlotSize}");
		GD.Print($"MinSlotId: {itemMinSlotId}");
        GD.Print($"MaxSlotId: {itemMaxSlotId}");
        GD.Print($"SlotBounds: max {maxInventorySlotBounds}min {minInventorySlotBounds}");

		if (itemMaxSlotId.X <= minInventorySlotBounds.X || itemMaxSlotId.Y <= minInventorySlotBounds.Y)
			return;
		if (itemMaxSlotId.X >= maxInventorySlotBounds.X || itemMaxSlotId.Y >= maxInventorySlotBounds.Y)
			return;
		if (itemMinSlotId.X <= minInventorySlotBounds.X || itemMinSlotId.Y <= minInventorySlotBounds.Y)
			return;
		if (itemMinSlotId.X >= maxInventorySlotBounds.X || itemMinSlotId.Y >= maxInventorySlotBounds.Y)
			return;

        if (InventoryItems.ContainsKey(item))
            RemoveItemFromInventory(item, InventoryItems[item]);

        foreach (var x_ctr in GD.Range((int)itemSlotSize.X))
        {
            foreach (var y_ctr in GD.Range((int)itemSlotSize.Y))
            {
                var keyPos = new Vector2(itemMinSlotId.X + x_ctr, itemMinSlotId.Y + y_ctr);
                InventoryItemSlots[keyPos] = item;
            }
        }

        InventoryItems[item] = itemMinSlotId;

        SelectedItem.GetNode<Sprite2D>("NinePatchRect/Sprite2D").ZIndex = 0;
        SelectedItem.GetNode<NinePatchRect>("NinePatchRect").ZIndex = 0;
        _isItemSelected = false;
        _isDraggingItem = false;
        SelectedItem = null;

        GD.Print("Item Added");
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
