using Godot;
using Test.Scripts;

public partial class TestCamera : Camera3D
{
    private CharacterBody3D _characterBody3D;
    [Export] public Vector3 CameraOffset;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _characterBody3D = GetNode<CharacterBody3D>("%PlayerBody");
        this.AssertMeDaddy(_characterBody3D != null, "Where's the body?");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        LookAtFromPosition(target: _characterBody3D.GlobalPosition,
            position: _characterBody3D.GlobalPosition - CameraOffset);
    }
}