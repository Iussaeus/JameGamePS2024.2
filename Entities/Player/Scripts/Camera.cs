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

    public Vector3 GetDirection() {
        var inputDir = Input.GetVector("left", "right", "forward", "backward");
        var direction = new Vector3(0, 0, 0);

        var upMarker = GetNode<Marker3D>("Marker3DUp");
        var rightMarker = GetNode<Marker3D>("Marker3DRight");
        var upDirection = GlobalPosition.DirectionTo(upMarker.GlobalPosition).Normalized();
        var rightDirection = GlobalPosition.DirectionTo(rightMarker.GlobalPosition).Normalized();

        if (inputDir != Vector2.Zero) {
            if (inputDir.Y > 0) {
                direction += -upDirection;
            }
            if (inputDir.Y < 0) {
                direction += upDirection;
            }
            if (inputDir.X > 0) {
                direction += rightDirection;
            }
            if (inputDir.X < 0) {
                direction += -rightDirection;
            }
        }

        return direction.Normalized();
    }
}
