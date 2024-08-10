using Godot;
using Test.Scripts.Components;
using Test.Scripts.Helpers;

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
        this.Assert(_camera3D != null, "Player Don't got no camera");

        _dashTimer.OneShot = true;
        _dashTimer.Timeout += () => CanDash = true;
        AddChild(_dashTimer);
    }

    public async void AwaitCamera()
    {
        await ToSignal(Globals.Instance, Globals.SignalName.CameraSpawned);
        _camera3D = Globals.Camera;
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

        if (Input.GetLastMouseVelocity() != Vector2.Zero &&
                intersection.TryGetValue("position", out var position))
            LookAt((Vector3)position);

        GlobalRotation = GlobalRotation with { X = 0, Y = GlobalRotation.Y, Z = 0 };
    }

    public override void _PhysicsProcess(double delta)
    {
        Move();
    }

    public void Move()
    {
        var newVelocity = Velocity;
		var delta = (float)GetPhysicsProcessDeltaTime();

        if (!IsOnFloor()) newVelocity.Y -= Gravity * delta;

        HorizontalVelocity = HorizontalVelocity.Lerp(GetDirection() * Speed,
                HorizontalAcceleration * delta);
        newVelocity.Z = HorizontalVelocity.Z;
        newVelocity.X = HorizontalVelocity.X;

        if (Input.IsActionJustPressed("space") && CanDash) Dash();

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

        if (inputDir.X > 0)
        { 
			direction.X += -_camera3D.GlobalBasis.Y.X; 
			direction.Z += -_camera3D.GlobalBasis.Y.Y; 
		}
        if (inputDir.X < 0) 
		{
			direction.X += _camera3D.GlobalBasis.Y.X;
			direction.Z += _camera3D.GlobalBasis.Y.Y;
		}
        if (inputDir.Y > 0) 
		{
			direction.X += _camera3D.GlobalBasis.Z.X;
			direction.Z += _camera3D.GlobalBasis.Z.Y;
		}
        if (inputDir.Y < 0) 
		{
			direction.X += -_camera3D.GlobalBasis.Z.X;
			direction.Z += -_camera3D.GlobalBasis.Z.Y;
		}

		return direction.Normalized();
	}
}
