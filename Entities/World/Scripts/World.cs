using Godot;
using Test.Entities.Components;

public partial class World : Node3D {
    public override void _Ready() {
        Globals.Instance.EmitSignal(Globals.SignalName.WorldSpawned, this);
    }

    public override void _Process(double delta) {
    }
}
