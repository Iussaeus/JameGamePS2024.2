using Godot;
using Test.Entities.Helpers;
using Test.Scripts.Components;

namespace Test.Scripts.Player;

public partial class PlayerController : CharacterBody3D {

    [ExportCategory("Dash")]
    [Export] public float DashCooldown = 2;
    [Export] public float RandomFloat = 0.5f;
    [Export] public float DashAcceleration = 5;
    [Export] public float DashLength = 100;
    [Export] public EasingFunctions DashEasing;
    [ExportCategory("Movement")]
    [Export] public float Speed = 5;
    [Export] public float HorizontalAcceleration = 10;
    [Export] public EasingFunctions MoveEasing;

    private Vector3 HorizontalVelocity = new();
    private Vector3 DashHorizontalVelocity = new();

    public float DashTime = 5;
    public bool CanDash = true;
    private readonly Timer _dashCooldown = new();
    public bool IsDashing = false;
    private readonly Timer _dashing = new();
    private Vector3 _dashStart = new();
    private Vector3 _dashEnd = new();
    private Vector3 DashDirection = new();

    public float Gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public Marker3D _marker3D;
    public Gun _gun;

    private Camera3D _camera3D;
    private readonly float _rayLen = 1000;


    public override void _Ready() {
        Globals.Instance.EmitSignal(Globals.SignalName.PlayerSpawned, this);

        _marker3D = GetNode<Marker3D>("Marker3D");
        _gun = GetNode<Gun>("Gun");
        AwaitCamera();

        _dashCooldown.OneShot = true;
        _dashCooldown.Timeout += () => CanDash = true;
        AddChild(_dashCooldown);

        _dashing.OneShot = true;
        _dashing.Timeout += () => IsDashing = true;
        AddChild(_dashing);
    }

    public async void AwaitCamera() {
        await ToSignal(Globals.Instance, Globals.SignalName.CameraSpawned);
        _camera3D = Globals.Camera;
        this.Assert(_camera3D != null, "Player has no camera");
    }

    public override void _Process(double delta) {
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

    public override void _PhysicsProcess(double delta) {
        if (Input.IsActionJustPressed("space") && CanDash /* && GetDirection() != Vector3.Zero */) {
            DashHorizontalVelocity = HorizontalVelocity + Velocity;

            DashDirection = GetDirection() == Vector3.Zero ? Vector3.Right : GetDirection();
            _dashStart = GlobalPosition;
            _dashEnd = GlobalPosition + (DashDirection * DashLength);

            DashHorizontalVelocity = new();
            _dashing.Start(DashTime);
        }

        if (!_dashing.IsStopped()) Dash((float)delta);
        else Move((float)delta);

        MoveAndSlide();
        // if (!_dashing.IsStopped()) GD.PrintS("start:", _dashStart, "end:", _dashEnd, "current:", GlobalPosition);
    }

    public void Move(float delta) {
        var newVelocity = GetDirection() * Speed;

        System.Func<float, float> easingFunc = (MoveEasing) switch {
            EasingFunctions.Linear => Ease.Linear,
            EasingFunctions.OutExponential => Ease.OutExponential,
            EasingFunctions.InExponential => Ease.InExponential,
            EasingFunctions.InBack => Ease.InBack,
            EasingFunctions.OutBounce => Ease.OutBounce,
            _ => Ease.Linear,
        };

        HorizontalVelocity = HorizontalVelocity.Ease(newVelocity, HorizontalAcceleration * delta, easingFunc);

        if (!IsOnFloor()) Velocity = Velocity with { Y = Velocity.Y - Gravity * (float)delta };

        Velocity = Velocity with {
            Z = HorizontalVelocity.Z,
            X = HorizontalVelocity.X,

        };
    }

    public void Dash(float delta) {
        CanDash = false;
        _dashCooldown.Start(DashCooldown);

        var progress = GlobalPosition.Progress(_dashStart, _dashEnd);
        var dashVector = _dashEnd - _dashStart;

        System.Func<float, float> easingFunc = (DashEasing) switch {
            EasingFunctions.Linear => Ease.Linear,
            EasingFunctions.OutExponential => Ease.OutExponential,
            EasingFunctions.InExponential => Ease.InExponential,
            EasingFunctions.InBack => Ease.InBack,
            EasingFunctions.OutBounce => Ease.OutBounce,
            _ => Ease.Linear,
        };
        var ease = new Vector3();
        var weight = _dashing.TimeLeft.MinMax(0, DashTime);
        var acceleration = RandomFloat + DashAcceleration * delta;

        if (progress <= 0)
            progress = GlobalPosition.Lerp(dashVector, acceleration - RandomFloat).Progress(_dashStart, _dashEnd);

        ease = DashHorizontalVelocity.Ease(dashVector, progress, easingFunc) * acceleration;

        DashHorizontalVelocity = ease;

        if ((GlobalPosition - _dashStart).LengthSquared() >= dashVector.LengthSquared()) {
            Velocity = Vector3.Zero;
            DashHorizontalVelocity = Vector3.Zero;
            HorizontalVelocity = Vector3.Zero;
            _dashing.Stop();

            // GD.PrintS("p: ", GlobalPosition.Progress(_dashStart, _dashEnd));
            // GD.PrintS("start:", _dashStart, "end:", _dashEnd, "current:", GlobalPosition);
        }
        // GD.PrintS("p: ", GlobalPosition.Progress(_dashStart, _dashEnd));
        // GD.PrintS("t:", easingFunc((float)weight), "v:", DashHorizontalVelocity, "ease:", ease, "newP:", newVelocity);
        Velocity = Velocity with {
            Z = DashHorizontalVelocity.Z,
            X = DashHorizontalVelocity.X,
        };
    }

    public Vector3 GetDirection() {
        var inputDir = Input.GetVector("left", "right", "forward", "backward");
        var direction = new Vector3(0, 0, 0);

        var upMarker = _camera3D.GetNode<Marker3D>("Marker3DUp");
        var rightMarker = _camera3D.GetNode<Marker3D>("Marker3DRight");
        var upDirection = _camera3D.GlobalPosition.DirectionTo(upMarker.GlobalPosition).Normalized();
        var rightDirection = _camera3D.GlobalPosition.DirectionTo(rightMarker.GlobalPosition).Normalized();

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
