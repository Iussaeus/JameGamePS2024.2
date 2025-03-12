using Godot;
using System.Collections.Generic;
using Test.Entities.Global;
using Test.Utils.Extensions;

[Tool]
[GlobalClass]
public partial class InventoryItemUI : Control {
    [Export] public Vector2I ItemSize = new(1, 1);

    private Inventory _inventory;
    public bool InInventory;

    private CollisionShape2D _collisionShape;
    private NinePatchRect _ninePatchRect;
    private Sprite2D _sprite;
    private Area2D _area;

    private bool hasArea;
    private bool hasSprite;
    private bool hasNinePatch;

    public static Color InvalidColor = new(1, 0.36f, 0.36f);
    public static Color ValidColor = new(1, 1, 1);


    public override async void _Ready() {
        PivotOffset = Size / 2;
        ChildOrderChanged += CheckChildren;
        CheckChildren();

        _collisionShape = GetNode<CollisionShape2D>("Area2D/colision(-1pixel_all_margins)");
        _ninePatchRect = GetNode<NinePatchRect>("NinePatchRect");
        _sprite = GetNode<Sprite2D>("Sprite2D");
        _area = GetNode<Area2D>("Area2D");

        if (!Engine.IsEditorHint()) {
            SetSize();

            Visible = false;

            _inventory = await SignalBus.Instance.AwaitSignalSingle<Inventory>(SignalBus.SignalName.InventorySpawned);

            EmitSignal(SignalName.Ready);
        }
    }

    public override void _Process(double delta) {
        if (Engine.IsEditorHint()) {
            if (ItemSize < Vector2I.One)
                ItemSize = new(1, 1);
            if (ItemSize.X > 64)
                ItemSize.X = 64;
            if (ItemSize.Y > 64)
                ItemSize.Y = 64;

            if (IsInstanceValid(_ninePatchRect))
                SetSize();
            else _ninePatchRect = GetNode<NinePatchRect>("NinePatchRect");
        }
    }

    public void OnCursorOnItem(InputEvent @event) {
        // WARN: DRAGGING NOT WORKING

        // if (@event is InputEventMouseMotion)
        //     if (IsItemSelected)
        //         IsDraggingItem = true;

        if (GetParent() is Inventory i)
            _inventory = i;

        if (InInventory) {
            if (Input.IsActionJustPressed("select_item")) {
                // GD.PrintRich($"[color=red]Clicked on: {this}, selected: {_inventory.SelectedItem}, isS:{_inventory.IsSelected()}, is3D:{this.HasNode("InventoryItem3d")}, can${_inventory.CanPlace(this)}");
                if (!_inventory.IsSelected()) {
                    _inventory.SelectItem(this);
                }
                else if (!_inventory.IsItemInsideBounds(this, this.GlobalPosition.ToTileSpace())) {
                    GD.Print("throwing Item");

                    if (this.HasNode("InventoryItem3D")) {
                        _inventory.ThrowItemOutside(this);
                        _inventory.DeselectItem();
                        return;
                    }
                    _inventory.PlaceItem(this);
                }
                else if (_inventory.CanPlace(this)) {
                    GD.PrintS("Adding Item:", _inventory.SelectedItem.GlobalPosition.ToTileSpace());

                    _inventory.AddItem(this, this.GlobalPosition.ToTileSpace());
                    _inventory.DeselectItem();
                }
            }
        }
    }

    public void OnOverlapping(Area2D area) {
        if (GetParent() is Inventory i)
            _inventory = i;

        if (area == _area || area == _inventory.InventoryArea || _inventory == null || area == null)
            return;


        if (!_inventory.IsOutsideOtherItems(this)) {
            _inventory.SelectedItem.GetNode<Sprite2D>("Sprite2D").Modulate = InventoryItemUI.InvalidColor;
            _inventory.SelectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = InventoryItemUI.InvalidColor;
        }
    }

    public void OnNotOverlapping(Area2D area) {
        if (GetParent() is Inventory i)
            _inventory = i;

        if (area == _area || area == _inventory.InventoryArea || _inventory == null)
            return;

        if (_inventory.IsOutsideOtherItems(this)) {
            _inventory.SelectedItem.GetNode<Sprite2D>("Sprite2D").Modulate = InventoryItemUI.ValidColor;
            _inventory.SelectedItem.GetNode<NinePatchRect>("NinePatchRect").Modulate = InventoryItemUI.ValidColor;
        }
    }

    public Texture2D GetNPRTexture() {
        return _ninePatchRect.Texture;
    }

    public void CheckChildren() {
        hasNinePatch = false;
        hasSprite = false;
        hasArea = false;

        foreach (var child in GetChildren()) {
            switch (child) {
                case Area2D:
                    hasArea = true;
                    break;
                case Sprite2D:
                    hasSprite = true;
                    break;
                case NinePatchRect:
                    hasNinePatch = true;
                    break;
            }
        }

        if (!hasArea || !hasSprite || !hasNinePatch)
            UpdateConfigurationWarnings();
    }

    public override string[] _GetConfigurationWarnings() {
        var warnings = new List<string>();

        if (!hasArea) {
            warnings.Add(new string("There is no Area2D"));
        }
        if (!hasSprite) {
            warnings.Add(new string("There is no Sprite2D"));
        }
        if (!hasNinePatch) {
            warnings.Add(new string("There is no NinePatchRect"));
        }

        return warnings.ToArray();
    }

    public void SetSize() {
        var paddingCount = ItemSize - new Vector2(1, 1);
        var paddingAmount = paddingCount * 4;

        var actualSize = ItemSize * (Vector2I)ProjectSettings.GetSetting("application/config/tile_size") + paddingAmount;

        if (_collisionShape.Shape is RectangleShape2D rectangle)
            rectangle.SetDeferred(RectangleShape2D.PropertyName.Size, actualSize);

        SetDeferred(Control.PropertyName.Size, actualSize);
        _ninePatchRect.SetDeferred(NinePatchRect.PropertyName.Size, actualSize);
        _collisionShape.SetDeferred(CollisionShape2D.PropertyName.Position, actualSize / 2);
    }
}
