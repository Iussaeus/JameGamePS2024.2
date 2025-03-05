using Godot;
using Test.Utils.Extensions;
using Test.Entities.Components;
using Test.Entities.Interaction;
using Test.Entities.Global;

namespace Test.Entities.Player;

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

    public float Gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public Camera Camera;
    public Marker3D Marker3D;
    public Gun Gun;

    public PlayerInteractor Interactor;

    public readonly Timer DashTimer = new();
    public float DashTime = 5;
    public bool CanDash = true;
    public bool IsDashing = false;
    private readonly Timer _dashCooldown = new();

    private Vector3 HorizontalVelocity = new();
    private Vector3 DashHorizontalVelocity = new();

    private Vector3 _dashStart = new();
    private Vector3 _dashEnd = new();
    private Vector3 DashDirection = new();

    private readonly float _rayLen = 1000;

    public override void _Ready() {

        Interactor = GetNode<PlayerInteractor>("PlayerInteractor");
        Marker3D = GetNode<Marker3D>("Marker3D");
        Gun = GetNode<Gun>("Gun");
        Camera = GetNode<Camera>("Camera3D");

        SignalBus.Instance.EmitSignal(SignalBus.SignalName.PlayerSpawned, this);

        _dashCooldown.OneShot = true;
        _dashCooldown.Timeout += () => CanDash = true;
        AddChild(_dashCooldown);

        DashTimer.OneShot = true;
        DashTimer.Timeout += () => IsDashing = true;
        AddChild(DashTimer);

    }

    public override void _Process(double delta) {
        Gun.GlobalPosition = Marker3D.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta) {
        MoveAndSlide();
        // if (!_dashing.IsStopped()) GD.PrintS("start:", _dashStart, "end:", _dashEnd, "current:", GlobalPosition);
    }
    public void Rotate() {
        var mousePos = GetViewport().GetMousePosition();
        var from = Camera.ProjectRayOrigin(mousePos);
        var to = from + Camera.ProjectRayNormal(mousePos) * _rayLen;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        var directSpaceState = GetWorld3D().DirectSpaceState;

        var intersection = directSpaceState.IntersectRay(query);

        if (Input.GetLastMouseVelocity() != Vector2.Zero && intersection.TryGetValue("position", out var position))
            LookAt((Vector3)position);

        GlobalRotation = GlobalRotation with { X = 0, Y = GlobalRotation.Y, Z = 0 };
    }

    public void Move(float delta, Vector3 direction) {
        var newVelocity = direction * Speed;

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
    public void StartDash(Vector3 direction) {
        DashHorizontalVelocity = HorizontalVelocity + Velocity;
        DashDirection = direction == Vector3.Zero ? Vector3.Right : direction;
        _dashStart = GlobalPosition;
        _dashEnd = GlobalPosition + (DashDirection * DashLength);

        DashHorizontalVelocity = new();
        DashTimer.Start(DashTime);
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
        var weight = DashTimer.TimeLeft.MinMax(0, DashTime);
        var acceleration = RandomFloat + DashAcceleration * delta;

        if (progress <= 0)
            progress = GlobalPosition.Lerp(dashVector, acceleration - RandomFloat).Progress(_dashStart, _dashEnd);

        ease = DashHorizontalVelocity.Ease(dashVector, progress, easingFunc) * acceleration;

        DashHorizontalVelocity = ease;

        if ((GlobalPosition - _dashStart).LengthSquared() >= dashVector.LengthSquared()) {
            Velocity = Vector3.Zero;
            DashHorizontalVelocity = Vector3.Zero;
            HorizontalVelocity = Vector3.Zero;
            DashTimer.Stop();

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

}
