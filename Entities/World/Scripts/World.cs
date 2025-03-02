using Godot;
using Test.Entities.Global;

public partial class World : Node3D {
    public override void _Ready() {
        SignalBus.Instance.EmitSignal(SignalBus.SignalName.WorldSpawned, this);
    }

    public override void _Process(double delta) {
    }
}
