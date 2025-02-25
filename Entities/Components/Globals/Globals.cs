using Godot;
using Test.Helpers.Extensions;
using Test.Entities.Player;

namespace Test.Entities.Components;

public partial class Globals : Node {
    [Signal]
    public delegate void PlayerSpawnedEventHandler(PlayerController player);

    [Signal]
    public delegate void CameraSpawnedEventHandler(Camera camera);

    [Signal]
    public delegate void InventorySpawnedEventHandler(Vector2I tileSize, Vector2I inventorySize, Inventory inventory);

    [Signal]
    public delegate void WorldSpawnedEventHandler(World world);

    public static Globals Instance;

    public static PlayerController Player;
    public static Camera Camera;
    public static Vector2I TileSize;
    public static Vector2I InventorySize;
    public static Inventory Inventory;
    public static World World;

    public const int GridPadding = 4;

    public override void _EnterTree() {
        Instance = this;
        WorldSpawned += world => World = world;
        PlayerSpawned += player => Player = player;
        CameraSpawned += camera => Camera = camera;
        InventorySpawned += (tileSize, inventorySize, inventory) => {
            var projectTileSize = (Vector2)ProjectSettings.GetSetting("application/config/tile_size");
            this.Assert(tileSize == projectTileSize, $"Script tileSize({tileSize}) dont match the project setting tileSize{projectTileSize}");
            TileSize = tileSize;
            InventorySize = inventorySize;
            Inventory = inventory;
        };
    }

    public override void _Ready() { }
}
