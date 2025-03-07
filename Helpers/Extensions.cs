using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Test.Entities.Global;

namespace Test.Utils.Extensions;

public enum EasingFunctions {
    Linear,
    OutExponential,
    InExponential,
    InBack,
    OutBounce,
}

public static class Ease {
    public static float Linear(float weight) {
        return weight;
    }
    public static float OutExponential(float weight) {
        return weight == 1 ? 1 : 1 - Mathf.Pow(2, -10 * weight);
    }
    public static float InExponential(float weight) {
        return weight == 0 ? 0 : Mathf.Pow(2, 10 * (weight - 1));
    }
    public static float InBack(float weight) {
        var c1 = 1.70158f;
        var c3 = c1 + 1;

        return c3 * weight * weight * weight - c1 * weight * weight;
    }

    public static float OutBounce(float weight) {
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (weight < 1 / d1) {
            return n1 * weight * weight;
        }
        else if (weight < 2 / d1) {
            return n1 * (weight -= 1.5f / d1) * weight + 0.75f;
        }
        else if (weight < 2.5 / d1) {
            return n1 * (weight -= 2.25f / d1) * weight + 0.9375f;
        }
        else {
            return n1 * (weight -= 2.625f / d1) * weight + 0.984375f;
        }
    }

}
public static class Vec3Extension {
    public static Vector3 Ease(this Vector3 from, Vector3 to, float weight, System.Func<float, float> easeFunc) {
        weight = Mathf.Clamp(weight, 0, 1);
        var t = easeFunc(weight);
        return from + (to - from) * t;
    }

    public static float Progress(this Vector3 current, Vector3 from, Vector3 to) {
        return Mathf.InverseLerp(0, from.DistanceTo(to), from.DistanceTo(current));
    }
    public static Vector3 Map(this Vector3 v, System.Func<float, float> fun) {
        return new Vector3(fun(v.X), fun(v.Y), fun(v.Z));
    }
}

public static class DoubleExtension {
    public static double MinMax(this double n, float min, float max) {
        return (1 - ((n - min) / (max - min)));
    }
    public static double RoundToOne(this double n) {
        return (Mathf.Round(n * 10) / 10);
    }
}
public static class FloatExtension {
    public static float MinMax(this float n, float min, float max) {
        return (float)(1 - ((n - min) / (max - min)));
    }
    public static float RoundToOne(this float n) {
        return Mathf.Round(n * 10) / 10;
    }
}

public static class Vec2Extension {
    public static Vector2I ToGlobalSpaceSnapped(this Vector2I v) {
        return (Vector2I)(((v - Vector2.One) * Globals.GridPadding) + (v * Globals.Inventory.TileSize)).Snapped(Globals.Inventory.TileSize + new Vector2I(4, 4));
    }

    public static Vector2 ToGlobalSpaceSnapped(this Vector2 v) {
        return (((v - Vector2.One) * Globals.GridPadding) + (v * Globals.Inventory.TileSize)).Snapped(Globals.Inventory.TileSize + new Vector2I(4, 4));
    }

    public static Vector2 ToGlobalSpace(this Vector2 v) {
        return ((v - Vector2.One) * Globals.GridPadding) + (v * Globals.Inventory.TileSize);
    }

    public static Vector2 ToGlobalSpace(this Vector2I v) {
        return ((v - Vector2.One) * Globals.GridPadding) + (v * Globals.Inventory.TileSize);
    }

    public static Vector2I ToTileSpace(this Vector2 v) {
        return (Vector2I)(v / (Globals.Inventory.TileSize + new Vector2I(4, 4))).Round();
    }

    public static Vector2I ToTileSpace(this Vector2I v) {
        return v / (Globals.Inventory.TileSize + new Vector2I(4, 4));
    }
}

public static class SysArrayExtensions {
    public static Array<T> ToGDArray<[MustBeVariant] T>(this T[] sysArray) {
        var GDarray = new Array<T>();
        for (var i = 0; i < sysArray.Length; i++) {
            GDarray.Add(sysArray[i]);
        }
        return GDarray;
    }

    public static List<T> ToList<T>(this T[] array) {
        var list = new List<T>();
        for (var i = 0; i < array.Length; i++) {
            list.Add(array[i]);
        }
        return list;
    }

    public static void Print<T>(this T[] a) {
        var str = new System.Text.StringBuilder();

        foreach (var e in a) {
            str.Append(e + " ");
        }

        GD.Print(str.ToString());
    }

    public static T[] Initialize<T>(this T[] a, T thing) {
        for (int i = 0; i < a.Length; i++) {
            a[i] = thing;
        }
        return a;
    }

    public static void ForEach<T>(this T[] a, System.Action<T> action) {
        for (int i = 0; i < a.Length; i++)
            action(a[i]);
    }

    public static void ForEach<T>(this T[] a, System.Func<T, T> action) {
        for (int i = 0; i < a.Length; i++)
            a[i] = action(a[i]);
    }

    public static void ClearMatrix(this int[,] m) {
        for (int i = 0; i < m.GetLength(0); i++) {
            for (int j = 0; j < m.GetLength(1); j++) {
                m[i, j] = 0;
            }
        }
    }

}

public static class ListExtensions {
    public static void ForEach<T>(this List<T> a, System.Action<T> action) {
        for (int i = 0; i < a.Count; i++)
            action(a[i]);
    }

    public static void ForEach<T>(this List<T> a, System.Func<T, T> action) {
        for (int i = 0; i < a.Count; i++)
            a[i] = action(a[i]);
    }

    public static void Print<T>(this List<T> a) {
        var str = new System.Text.StringBuilder();

        foreach (var e in a) {
            str.Append(e + " ");
        }

        GD.Print(str.ToString());
    }
}

public static class NodeExtensions {
    public static void Assert(this Node node, bool truthy, string message) {
        if (!truthy) {
            GD.PushError($"{node.Name}: Assert Failed: {message}");
            node.GetTree().Paused = true;
        }
    }

    public static async Task<T> AwaitSignalSingle<T>(this Node node, StringName signal) where T : Node {
        // GD.Print($"Waiting for {signal}");

        T result = default;
        var s = await node.ToSignal(SignalBus.Instance, signal);
        // GD.Print($"Done waiting for {signal}");
        // s.Print();
        if (s.Length == 1) {
            result = (T)s[0];
        }

        // GD.Print($"result of {signal}: ", result);

        return result;
    }

    public static async Task<T[]> AwaitSignalArray<T>(this Node node, StringName signal) where T : Node {
        // GD.Print($"Waiting for {signal}");

        var s = await node.ToSignal(SignalBus.Instance, signal);

        // GD.Print($"Done waiting for {signal}");
        // s.Print();
        T[] result = new T[s.Length];

        for (int i = 0; i < result.Length; i++) {
            result[i] = (T)s[i];
        }

        // GD.Print($"result of{signal}: ", result);
        // result.Print();

        return result;
    }

    public static async Task<Variant[]> AwaitSignal(this Node node, StringName signal) {
        // GD.Print($"Waiting for {signal}");
        var s = await node.ToSignal(SignalBus.Instance, signal);

        // GD.Print($"Done waiting for {signal}");
        // s.Print();
        return s;
    }
}

public static class Node3DExtentions {
    public static Vector3 ForwardVector(this Node3D n) {
        return -n.Transform.Basis.Z;
    }
}
