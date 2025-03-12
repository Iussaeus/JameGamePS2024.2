using Godot;
using Test.Entities.Player;
using Test.Entities.Console;
using Test.Entities.Interaction;
using Test.Entities.Enemy;
using Test.Utils.Extensions;

namespace Test.Entities.Global;

public partial class SignalBus : Node {
    [Signal]
    public delegate void PlayerSpawnedEventHandler(PlayerController player);

    [Signal]
    public delegate void InventorySpawnedEventHandler(Inventory inventory);

    [Signal]
    public delegate void WorldSpawnedEventHandler(World world);

    [Signal]
    public delegate void ConsoleSpawnedEventHandler(ConsoleWindow console);

    public static SignalBus Instance { get; private set; }

    public override void _EnterTree() {
        Instance = this;
    }

    public override void _Ready() {
        WorldSpawned += OnWorldSpawned;
        PlayerSpawned += OnPlayerSpawned;
        InventorySpawned += OnInventorySpawned;
        ConsoleSpawned += OnConsoleSpawned;
    }

    public override void _ExitTree() {
        WorldSpawned -= OnWorldSpawned;
        PlayerSpawned -= OnPlayerSpawned;
        InventorySpawned -= OnInventorySpawned;
        ConsoleSpawned -= OnConsoleSpawned;
    }

    public void OnWorldSpawned(World world) => Globals.World = world;
    public void OnPlayerSpawned(PlayerController player) => Globals.Player = player;
    public void OnInventorySpawned(Inventory inventory) => Globals.Inventory = inventory;

    public void OnConsoleSpawned(ConsoleWindow console) {
        console.AddCommand(SpawnBox);
        console.AddCommand(SpawnMelleeEnemy);
        console.AddCommand(SpawnNewItem);

        Globals.Console = console;
    }

    public void SpawnNewItem(int x = 4, int y = 4) {
        var item = GD.Load<PackedScene>("res://Entities/Items/Scenes/NewItem.tscn").Instantiate<InventoryItemUI>();
        item.ItemSize = new(x, y);

        Globals.World.AddChild(item);

        var item3D = item.GetNode<InventoryItem3D>("InventoryItem3D");
        item3D.GlobalPosition = Globals.Player.GlobalPosition + Globals.Player.ForwardVector() * 5 + new Vector3(0, 10, 0);
    }

    public void SpawnBox() {
        var x = GD.Load<PackedScene>("res://Entities/Interaction/Scenes/Box.tscn").Instantiate<Box>();
        Globals.World.AddChild(x);

        x.GlobalPosition = Globals.Player.GlobalPosition + Globals.Player.ForwardVector() * 5 + new Vector3(0, 10, 0);
    }

    public void SpawnMelleeEnemy() {
        var x = GD.Load<PackedScene>("res://Entities/Enemy/MeleeEnemy/Scenes/MeleeEnemy.tscn").Instantiate<MeleeEnemy>();
        Globals.World.AddChild(x);

        x.GlobalPosition = Globals.Player.GlobalPosition + Globals.Player.ForwardVector() * 5 + new Vector3(0, 10, 0);
    }
}
