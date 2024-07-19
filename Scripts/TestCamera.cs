using Godot;

public partial class TestCamera : Camera3D
{
    [Export] public Vector3 CameraOffset;
    public CharacterBody3D CharacterBody3D;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        CharacterBody3D = GetParent<SubViewport>().GetNode<CharacterBody3D>("PlayerBody");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        LookAtFromPosition(target: CharacterBody3D.GlobalPosition,
            position: CharacterBody3D.GlobalPosition - CameraOffset);
    }
}