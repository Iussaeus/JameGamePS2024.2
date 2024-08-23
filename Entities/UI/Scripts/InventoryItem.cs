using Godot;
using Test.Scripts.Components;

[Tool]
[GlobalClass]
public partial class InventoryItem : Control
{
    [Export] public Vector2 ItemSize = new(1, 1);

    [Export]
    public bool UpdateSize
    {
        get => false;
        set
        {
            if (Engine.IsEditorHint())
                SetSizeEditor();
        }
    }

    [Export] private NinePatchRect NinePatchRectEditor;
    private CollisionShape2D collisionShape;
    private NinePatchRect ninePatchRect;

    public override void _Ready()
    {
		collisionShape = GetNode<CollisionShape2D>("NinePatchRect/Sprite2D/Area2D/colision(-1pixel_all_margins)");
		ninePatchRect = GetNode<NinePatchRect>("NinePatchRect");

        if (!Engine.IsEditorHint())
        {
            SetSize();
        }
    }

    public async void SetSize()
    {
        await ToSignal(Globals.Instance, Globals.SignalName.InventorySpawned);

        var paddingCount = ItemSize - new Vector2(1, 1);
        var paddingAmount = paddingCount * Globals.GridPadding;

        var actualSize = ItemSize * (Vector2I)ProjectSettings.GetSetting("application/config/tile_size") + paddingAmount;

        var rectangleShape = new RectangleShape2D();
        rectangleShape.Size = actualSize;

        SetDeferred(Control.PropertyName.Size, actualSize);
        ninePatchRect.SetDeferred(NinePatchRect.PropertyName.Size, actualSize);
        collisionShape.SetDeferred(CollisionShape2D.PropertyName.Shape, rectangleShape);
        collisionShape.SetDeferred(CollisionShape2D.PropertyName.Position, actualSize / 2);
    }

    public void SetSizeEditor()
    {
        var paddingCount = ItemSize - new Vector2(1, 1);
        var paddingAmount = paddingCount * 4;

        var actualSize = ItemSize * (Vector2I)ProjectSettings.GetSetting("application/config/tile_size") + paddingAmount;

        SetDeferred(Control.PropertyName.Size, actualSize);
        NinePatchRectEditor.SetDeferred(NinePatchRect.PropertyName.Size, actualSize);
    }
}
