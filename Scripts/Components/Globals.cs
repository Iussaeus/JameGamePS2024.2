using Godot;

namespace Test.Scripts.Components;

public partial class Globals : Node3D
{
    [Signal]
    public delegate void PlayerSpawnedEventHandler(CharacterBody3D player);
    [Signal]
    public delegate void CameraSpawnedEventHandler(Camera3D camera);
	[Signal]
	public delegate void InventorySpawnedEventHandler(Vector2 tileSize, Vector2 inventorySize);

	public static Globals Instance {get; private set;}

    public static CharacterBody3D Player { get; set; }
    public static Camera3D Camera { get; set; }
	public static Vector2 TileSize { get; set; }
	public static Vector2 InventorySize { get; set; }
	
	public const int GridPadding = 4;

    public override void _Ready()
    {
		Instance = this;

        PlayerSpawned += (player => Player = player);
        CameraSpawned += (camera => Camera = camera);
		InventorySpawned += (tileSize, inventorySize) => 
		{
			TileSize = tileSize;
			InventorySize = inventorySize;
		};
    }

    public override string ToString()
    {
        return base.ToString() +
			$"\n{Instance}\nCharacter: {Player}\nCamera: {Camera}\nTileSize: {TileSize}\nInventorySize: {InventorySize}";
    }
}
