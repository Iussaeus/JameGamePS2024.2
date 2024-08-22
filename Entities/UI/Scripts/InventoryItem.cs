using Godot;
using Test.Scripts.Components;

public partial class InventoryItem : Control
{
	[Export] public Vector2 ItemSize = new(1, 1);

	private CollisionShape2D collisionShape;
	private NinePatchRect ninePatchRect;

	public override async void _Ready()
	{
		collisionShape = GetNode<CollisionShape2D>("NinePatchRect/Sprite2D/Area2D/colision(-1pixel_all_margins)");
		ninePatchRect = GetNode<NinePatchRect>("NinePatchRect");

		await ToSignal(Globals.Instance, Globals.SignalName.InventorySpawned);
		
		var paddingCount = ItemSize - new Vector2(1, 1);
		var paddingAmount = paddingCount * Globals.GridPadding;

		var actualSize = ItemSize * Globals.TileSize + paddingAmount;

		var rectangleShape = new RectangleShape2D();
		rectangleShape.Size = actualSize;


		SetDeferred(Control.PropertyName.Size, actualSize);
		ninePatchRect.SetDeferred(NinePatchRect.PropertyName.Size, actualSize);
		collisionShape.SetDeferred(CollisionShape2D.PropertyName.Shape, rectangleShape);
		collisionShape.SetDeferred(CollisionShape2D.PropertyName.Position, actualSize / 2);
	}
}
