using Godot;
using System.Collections.Generic;

[Tool]
public partial class InventoryItemUI : Control
{
	[Export] public Vector2I ItemSize = new(1, 1);

	private CollisionShape2D _collisionShape;
	private NinePatchRect _ninePatchRect;
	private Sprite2D _sprite;

	private bool hasArea;
	private bool hasSprite;
	private bool hasNinePatch;

	public override void _Ready()
	{
		ChildOrderChanged += CheckChildren;
		_collisionShape = GetNode<CollisionShape2D>("Area2D/colision(-1pixel_all_margins)");
		_ninePatchRect = GetNode<NinePatchRect>("NinePatchRect");
		_sprite = GetNode<Sprite2D>("Sprite2D");

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

	public void CheckChildren()
	{
		hasNinePatch = false;
		hasSprite = false;
		hasArea = false;

		foreach (var child in GetChildren())
		{
			switch (child)
			{
				case Area2D:
					hasArea = true;
					break;
				case Sprite2D:
					hasSprite= true;
					break;
				case NinePatchRect:
					hasNinePatch= true;
					break;
			}
		}

		if (!hasArea || !hasSprite || hasNinePatch)
			UpdateConfigurationWarnings();
	}

	public override string[] _GetConfigurationWarnings()
	{
		var warnings = new List<string>();

		if (!hasArea)
		{
			warnings.Add(new string("There is no Area2D"));
		}
		if (!hasSprite)
		{
			warnings.Add(new string("There is no Sprite2D"));
		}
		if (!hasNinePatch)
		{
			warnings.Add(new string("There is no NinePatchRect"));
		}

		return warnings.ToArray();
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
