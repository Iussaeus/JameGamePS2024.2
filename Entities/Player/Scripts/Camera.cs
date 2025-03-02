using Godot;

namespace Test.Entities.Player;

public partial class Camera : Camera3D {

    private CharacterBody3D _player;
    [Export] public Vector3 CameraOffset = new(30, 60, 40);

    public override void _Ready() {
        _player = GetParent<PlayerController>();
    }

    public override void _Process(double delta) {
        LookAtFromPosition(_player.GlobalPosition + CameraOffset, _player.GlobalPosition);
    }
}
