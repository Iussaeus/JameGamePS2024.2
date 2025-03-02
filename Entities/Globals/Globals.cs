using Godot;
using Test.Utils.Extensions;
using Test.Entities.Player;
using Test.Entities.Console;
using System.Threading.Tasks;

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

    public async Task<T> AwaitSignalSingle<T>(StringName signal) where T : Node {
        GD.Print($"Waiting for {signal}");

        T result = default;
        var s = await ToSignal(SignalBus.Instance, signal);
        GD.Print($"Done waiting for {signal}");
        s.Print();
        if (s.Length == 1) {
            result = (T)s[0];
        }

        GD.Print($"result of{signal}: ", result);

        return result;
    }

    public async Task<T[]> AwaitSignalArray<T>(StringName signal) where T : Node {
        GD.Print($"Waiting for {signal}");

        var s = await ToSignal(SignalBus.Instance, signal);

        GD.Print($"Done waiting for {signal}");
        s.Print();
        T[] result = new T[s.Length];

        for (int i = 0; i < result.Length; i++) {
            result[i] = (T)s[i];
        }

        GD.Print($"result of{signal}: ", result);
        result.Print();

        return result;
    }

    public async Task<Variant[]> AwaitSignal(StringName signal) {
        GD.Print($"Waiting for {signal}");
        var s = await ToSignal(SignalBus.Instance, signal);
        GD.Print($"Done waiting for {signal}");
        s.Print();
        return s;
    }
}
