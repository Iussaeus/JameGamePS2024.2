using Godot;
using Test.Entities.Helpers;

namespace Test.Scripts.Components;

public partial class Globals : Node
{
    [Signal]
    public delegate void PlayerSpawnedEventHandler(CharacterBody3D player);
    [Signal]
    public delegate void CameraSpawnedEventHandler(Camera3D camera);
    [Signal]
    public delegate void InventorySpawnedEventHandler(Vector2I tileSize, Vector2I inventorySize, Inventory inventory);
	[Signal]
	public delegate void WorldSpawnedEventHandler(Node3D world);

    public static Globals Instance;

    public static CharacterBody3D Player;
    public static Camera3D Camera;
    public static Vector2I TileSize;
    public static Vector2I InventorySize;
    public static Inventory Inventory;
	public static Node3D World;

    public const int GridPadding = 4;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
		WorldSpawned += world => { World = world; };
        PlayerSpawned += player => { Player = player; };
        CameraSpawned += camera => { Camera = camera; };
        InventorySpawned += (tileSize, inventorySize, inventory) =>
        {
			var projectTileSize = (Vector2)ProjectSettings.GetSetting("application/config/tile_size");
			this.Assert(tileSize == projectTileSize, $"Script tileSize({tileSize}) dont match the project setting tileSize{projectTileSize}");
            TileSize = tileSize;
            InventorySize = inventorySize;
            Inventory = inventory;
        };
    }


    public override string ToString()
    {
        return base.ToString() +
            $"\n{Instance}\nCharacter: {Player}\nCamera: {Camera}\nTileSize: {TileSize}\nInventorySize: {InventorySize}";
    }
}
