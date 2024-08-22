using Godot;

namespace Test.Scripts.Components;

public partial class Globals : Node
{
	[Signal]
	public delegate void PlayerSpawnedEventHandler(CharacterBody3D player);
	[Signal]
	public delegate void CameraSpawnedEventHandler(Camera3D camera);
	[Signal]
	public delegate void InventorySpawnedEventHandler(Vector2 tileSize, Vector2 inventorySize);

	public static Globals Instance;

	public static CharacterBody3D Player;
	public static Camera3D Camera;
	public static Vector2 TileSize;
	public static Vector2 InventorySize;

	public const int GridPadding = 4;

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		PlayerSpawned += player => 
		{
			Player = player;
			GD.Print("Player", Player);
		};

		CameraSpawned += camera => 
		{
			Camera = camera;
			GD.Print("Global camera ", Camera);
		};

		InventorySpawned += (tileSize, inventorySize) =>
		{
			TileSize = tileSize;
			InventorySize = inventorySize;
			GD.Print("TileSize ", TileSize);
			GD.Print("InvetorySize ", InventorySize);
		};
	}

	public override string ToString()
	{
		return base.ToString() +
			$"\n{Instance}\nCharacter: {Player}\nCamera: {Camera}\nTileSize: {TileSize}\nInventorySize: {InventorySize}";
	}
}
