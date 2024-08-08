using Godot;
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

	public static void Assert(this CharacterBody3D node, bool truthy, string message)
	{
		if (!truthy)
		{
			PushError("Assert Failed: " + message);
			node.GetTree().Paused = true;
		}
	}
}
