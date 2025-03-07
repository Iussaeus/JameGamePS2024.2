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
        WorldSpawned += world => Globals.World = world;
        PlayerSpawned += player => Globals.Player = player;
        InventorySpawned += inventory => Globals.Inventory = inventory;
        ConsoleSpawned += console => {
            console.AddCommand(SpawnBox);
            console.AddCommand(SpawnMelleeEnemy);
            Globals.Console = console;
        };
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

    public override void _ExitTree() {
        WorldSpawned -= world => Globals.World = world;
        PlayerSpawned -= player => Globals.Player = player;
        InventorySpawned -= inventory => Globals.Inventory = inventory;
        ConsoleSpawned -= console => {
            console.AddCommand(SpawnBox);
            console.AddCommand(SpawnMelleeEnemy);
            Globals.Console = console;
        };
    }
}
