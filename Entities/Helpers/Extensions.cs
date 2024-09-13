using Godot;
using Godot.Collections;
using System.Collections.Generic;
using Test.Scripts.Components;
using static Godot.GD;

namespace Test.Entities.Helpers;

public static class Extensions
{
	public static Vector2I ToGlobalSpaceSnapped(this Vector2I v)
	{
		return (Vector2I)(((v - Vector2.One) * Globals.GridPadding) + (v * Globals.TileSize)).Snapped(Globals.TileSize + new Vector2I(4, 4));
	}

	public static Vector2 ToGlobalSpaceSnapped(this Vector2 v)
	{
		return (((v - Vector2.One) * Globals.GridPadding) + (v * Globals.TileSize)).Snapped(Globals.TileSize + new Vector2I(4, 4));
	}

	public static Vector2 ToGlobalSpace(this Vector2 v)
	{
		return ((v - Vector2.One) * Globals.GridPadding) + (v * Globals.TileSize);
	}

	public static Vector2 ToGlobalSpace(this Vector2I v)
	{
		return ((v - Vector2.One) * Globals.GridPadding) + (v * Globals.TileSize);
	}

	public static Vector2I ToTileSpace(this Vector2 v)
	{
		return (Vector2I)(v / (Globals.TileSize + new Vector2I(4, 4))).Round();
	}

	public static Vector2I ToTileSpace(this Vector2I v)
	{

		return v / (Globals.TileSize + new Vector2I(4, 4));
	}

	public static void ClearMatrix(this int[,] m)
	{
		for (int i = 0; i < m.GetLength(0); i++)
		{
			for (int j = 0; j < m.GetLength(1); j++)
			{
				m[i, j] = 0;
			}
		}
	}

	public static void Assert(this Node node, bool truthy, string message)
	{
		if (!truthy)
		{
			PushError("Assert Failed: " + message);
			node.GetTree().Paused = true;
		}
	}

	public static Array<T> ToGDArray<[MustBeVariant] T>(this T[] sysArray)
	{
		var GDarray = new Array<T>();
		for (var i = 0; i < sysArray.Length; i++)
		{
			GDarray.Add(sysArray[i]);
		}
		return GDarray;
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
