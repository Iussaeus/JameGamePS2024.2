using Godot;
using Godot.Collections;
using System.Collections.Generic;
using static Godot.GD;

namespace Test.Scripts.Helpers;

public static class Extensions
{
    public static void Assert(this Node3D node, bool truthy, string message)
    {
        if (truthy)
        {
            PushError("Assert Failed: " + message);
            node.GetTree().Paused = true;
        }
    }
    public static Array<T> ToGDArray<[MustBeVariant] T>(this T[] sysArray)
    {
        var array = new Array<T>();
        for (var i = 0; i < sysArray.Length; i++)
        {
            array.Add(sysArray[i]);
        }
        return array;
    }

    public static List<T> ToList<T>(this T[] array)
    {
		var list = new List<T>();
        for (var i = 0; i < array.Length; i++)
        {
            list.Add(array[i]);
        }
		return list;
    }
}
