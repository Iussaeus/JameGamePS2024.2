using Godot;
using Test.Scripts.Components;
using Test.Scripts.Helpers;

namespace Test.Scripts.Player;

public partial class PlayerController : CharacterBody3D
{
    private readonly Timer _dashTimer = new();

    private readonly float _rayLen = 1000;
    private Camera3D _camera3D;
    private Gun _gun;
    private Marker3D _marker3D;
    public bool CanDash = true;
    [Export] public float DashCooldown = 2;
    [Export] public float DashVelocity = 200;
    public float Gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
    [Export] public float Speed = 5;

    public override void _Ready()
    {
        Globals.Instance.EmitSignal(Globals.SignalName.PlayerSpawned, this);

        _marker3D = GetNode<Marker3D>("Marker3D");
        _gun = GetNode<Gun>("Gun");
		AwaitCamera();
        this.Assert(_camera3D != null, "Player Don't got no camera");

        _dashTimer.OneShot = true;
        _dashTimer.Timeout += () => CanDash = false;
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
        var inputDir = Input.GetVector("left", "right", "forward", "backward");

        Move(inputDir, (float)delta);
    }

    public void Move(Vector2 inputDir, float delta)
    {
        var newVelocity = Velocity;

        if (!IsOnFloor()) newVelocity.Y -= Gravity * delta;

        var direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

        if (direction != Vector3.Zero)
        {
            newVelocity.X = direction.X * Speed;
            newVelocity.Z = direction.Z * Speed;
        }
        else
        {
            newVelocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            newVelocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        if (Input.IsActionJustPressed("space") && CanDash)
        {
            CanDash = false;
            _dashTimer.Start();
            newVelocity = direction * DashVelocity;
        }

        Velocity = newVelocity;

        MoveAndSlide();
    }
}
