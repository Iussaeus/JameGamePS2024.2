using Godot;

namespace Test.Scripts;

public static class Assert
{
    public static void AssertMeDaddy(this Node node, bool cond, string msg)
    {
        if (cond) return;
        GD.PrintErr(msg);
        node.GetTree().Paused = true;
    }
}