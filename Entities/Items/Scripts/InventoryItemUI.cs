using Godot;

[Tool]
public partial class InventoryItemUI : Control
{
    [Export] public Vector2I ItemSize = new(1, 1);

    private CollisionShape2D _collisionShape;
    private NinePatchRect _ninePatchRect;

    public override void _Ready()
    {
        _collisionShape = GetNode<CollisionShape2D>("NinePatchRect/Sprite2D/Area2D/colision(-1pixel_all_margins)");
        _ninePatchRect = GetNode<NinePatchRect>("NinePatchRect");

        if (!Engine.IsEditorHint())
        {
            SetSize();
        }
    }
    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
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

    public void SetSize()
    {
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
