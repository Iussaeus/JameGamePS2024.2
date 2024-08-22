using Godot;

public partial class InventoryGrid : CenterContainer 
{
	[Export] public PackedScene Tile;

	public GridContainer Container;

	public override void _Ready()
	{
		Container = GetNode<GridContainer>("GridContainer");
	}

	public void PlaceTiles(Vector2 inventorySize, Vector2 tileSize)
	{
		Container.Columns = (int)inventorySize.X;
		Container.SetDeferred(GridContainer.PropertyName.Size, inventorySize * tileSize);
		for (var i = 0; i < inventorySize.X * inventorySize.Y; i++)
		{
			var newTile = Tile.Instantiate<ColorRect>();
			newTile.CustomMinimumSize = tileSize;
			Container.AddChild(newTile);
		}
	}
}
