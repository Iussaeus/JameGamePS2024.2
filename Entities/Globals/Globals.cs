using Godot;
using Test.Entities.Player;
using Test.Entities.Console;

namespace Test.Entities.Global;

public partial class Globals : Node {
    public static Globals Instance { get; private set; }

    public static PlayerController Player;
    public static Camera Camera;
    public static Inventory Inventory;
    public static World World;
    public static ConsoleWindow Console;

    public const int GridPadding = 4;

    public override void _EnterTree() => Instance = this;
}
