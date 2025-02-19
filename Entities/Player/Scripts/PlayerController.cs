using Godot;
using Test.Entities.Helpers;
using Test.Scripts.Components;

namespace Test.Scripts.Player;

public partial class PlayerController : CharacterBody3D {

    [ExportCategory("Dash")]
    [Export] public float DashCooldown = 2;
    [Export] public float DashTime = 1;
    [Export] public float DashVelocity = 200;
    [Export] public float DashHorizontalAcceleration = 10;
    [Export] public EasingFunctions Easing { get; set; }
    [ExportCategory("Movement")]
    [Export] public float Speed = 5;
    [Export] public float HorizontalAcceleration = 10;
    [Export(PropertyHint.Enum, "Linear:0, OutExp:1, InExp:2, InOutExp:3")] public int MoveEasing;

    private Vector3 HorizontalVelocity = new();
    private Vector3 DashHorizontalVelocity = new();

    public bool CanDash = true;
    private readonly Timer _dashCooldown = new();

    public bool IsDashing = false;
    private readonly Timer _dashing = new();

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
            DashHorizontalVelocity = new();
            _dashing.Start(DashTime);
        }

        if (!_dashing.IsStopped()) Dash((float)delta);
        else Move((float)delta);

        MoveAndSlide();
    }

    public void Move(float delta) {
        var newVelocity = Velocity;
        var newPos = GetDirection() * Speed;

        // if (!IsOnFloor()) newVelocity.Y -= Gravity * delta;

        var ease = new Vector3();
        switch (MoveEasing) {
            case 0:
                ease = HorizontalVelocity.Ease(newPos, HorizontalAcceleration * delta, Ease.Linear);
                break;
            case 1:
                ease = HorizontalVelocity.Ease(newPos, HorizontalAcceleration * delta, Ease.OutExponential);
                break;
            case 2:
                ease = HorizontalVelocity.Ease(newPos, HorizontalAcceleration * delta, Ease.InExponential);
                break;
            case 3:
                ease = HorizontalVelocity.Ease(newPos, HorizontalAcceleration * delta, Ease.InOutExponential);
                break;
        }

        HorizontalVelocity = ease;

        newVelocity.Z = HorizontalVelocity.Z;
        newVelocity.X = HorizontalVelocity.X;

        Velocity = newVelocity;
    }

    public void Dash(float delta) {
        var newVelocity = Velocity;

        var direction = GetDirection() == Vector3.Zero ? Vector3.Right : GetDirection();
        var newPos = direction * DashVelocity;

        CanDash = false;
        _dashCooldown.Start(DashCooldown);

        var weight = _dashing.TimeLeft.MinMax(0, DashTime);

        var ease = new Vector3();

        System.Func<float, float> easingFunc = (Easing) switch {
            EasingFunctions.Linear => Ease.Linear,
            EasingFunctions.OutExponential => Ease.OutExponential,
            EasingFunctions.InExponential => Ease.InExponential,
            EasingFunctions.InOutExponential => Ease.InOutExponential,
            EasingFunctions.InBack => Ease.InBack,
            EasingFunctions.OutBounce => Ease.OutBounce,
        };

        ease = DashHorizontalVelocity.Ease(newPos, weight, easingFunc);

        GD.PrintS(_dashing.TimeLeft, _dashing.TimeLeft.MinMax(0, DashTime).RoundToOne(), "t:", easingFunc(weight), "v:", DashHorizontalVelocity, "ease:", ease, "newP:", newPos);

        if (easingFunc == Ease.InBack)
            DashHorizontalVelocity = ease * direction;
        else DashHorizontalVelocity = ease;

        newVelocity.Z = DashHorizontalVelocity.Z;
        newVelocity.X = DashHorizontalVelocity.X;


        Velocity = newVelocity;
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
