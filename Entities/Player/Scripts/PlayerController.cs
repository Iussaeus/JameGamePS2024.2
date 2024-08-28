using Godot;
using Test.Entities.Helpers;
using Test.Scripts.Components;

namespace Test.Scripts.Player;

public partial class PlayerController : CharacterBody3D
{

    [Export] public float DashCooldown = 2;
    [Export] public float DashVelocity = 200;
    [Export] public float DashAcceleration = 4;
    [Export] public float Speed = 5;
    [Export] public float HorizontalAcceleration = 10;

    public Vector3 HorizontalVelocity = new();
    public bool CanDash = true;
    private readonly Timer _dashTimer = new();

    public float Gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    private Camera3D _camera3D;
    private Gun _gun;
    private Marker3D _marker3D;
    private readonly float _rayLen = 1000;

    public override void _Ready()
    {
        Globals.Instance.EmitSignal(Globals.SignalName.PlayerSpawned, this);

        _marker3D = GetNode<Marker3D>("Marker3D");
        _gun = GetNode<Gun>("Gun");
        AwaitCamera();

        _dashTimer.OneShot = true;
        _dashTimer.Timeout += () => CanDash = true;
        AddChild(_dashTimer);
    }

    public async void AwaitCamera()
    {
        await ToSignal(Globals.Instance, Globals.SignalName.CameraSpawned);
        _camera3D = Globals.Camera;
        this.Assert(_camera3D != null, "Player has no camera");
    }

    public override void _Process(double delta)
    {
        _gun.GlobalPosition = _marker3D.GlobalPosition;

        var mousePos = GetViewport().GetMousePosition();
        var from = _camera3D.ProjectRayOrigin(mousePos);
        var to = from + _camera3D.ProjectRayNormal(mousePos) * _rayLen;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var directSpaceState = GetWorld3D().DirectSpaceState;

        var intersection = directSpaceState.IntersectRay(query);

        if (!Globals.Inventory.IsOpen)
            if (Input.GetLastMouseVelocity() != Vector2.Zero && intersection.TryGetValue("position", out var position))
                LookAt((Vector3)position);

        GlobalRotation = GlobalRotation with { X = 0, Y = GlobalRotation.Y, Z = 0 };
    }

    public override void _PhysicsProcess(double delta)
    {
        Fall((float)delta);

        if (!Globals.Inventory.IsOpen)
        {
            if (Input.IsActionJustPressed("space") && CanDash) Dash();
            Move((float)delta);
        }
    }

    public void Fall(float delta)
    {
        var newVelocity = Velocity;

        if (!IsOnFloor()) newVelocity.Y -= Gravity * delta;

        Velocity = newVelocity;

        MoveAndSlide();
    }

    public void Move(float delta)
    {
        var newVelocity = Velocity;

        HorizontalVelocity = HorizontalVelocity.Lerp(GetDirection() * Speed, HorizontalAcceleration * delta);
        newVelocity.Z = HorizontalVelocity.Z;
        newVelocity.X = HorizontalVelocity.X;

        Velocity = newVelocity;

        MoveAndSlide();
    }

    public void Dash()
    {
        var newVelocity = Velocity;

        CanDash = false;
        _dashTimer.Start();
        HorizontalVelocity = HorizontalVelocity.Lerp(GetDirection() * DashVelocity,
                DashAcceleration * (float)GetPhysicsProcessDeltaTime());
        newVelocity.Z = HorizontalVelocity.Z;
        newVelocity.X = HorizontalVelocity.X;

        Velocity = newVelocity;

        MoveAndSlide();
    }

    public Vector3 GetDirection()
    {
        var inputDir = Input.GetVector("left", "right", "forward", "backward");
        var direction = new Vector3(0, 0, 0);

		var upMarker = _camera3D.GetNode<Marker3D>("Marker3DUp");
		var rightMarker = _camera3D.GetNode<Marker3D>("Marker3DRight");
        var upDirection = _camera3D.GlobalPosition.DirectionTo(upMarker.GlobalPosition).Normalized();
		var rightDirection = _camera3D.GlobalPosition.DirectionTo(rightMarker.GlobalPosition).Normalized();

        if (inputDir != Vector2.Zero)
		{
			if (inputDir.Y > 0)
			{
				direction += -upDirection;
			}
			if (inputDir.Y < 0)
			{
				direction += upDirection;
			}
			if (inputDir.X > 0)
			{
				direction += rightDirection;
			}
			if (inputDir.X < 0)
			{
				direction += -rightDirection;
			}
		}

        return direction.Normalized();
    }
}
