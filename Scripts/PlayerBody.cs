using Godot;
using Godot.Collections;

namespace Test.Scripts;

public partial class PlayerBody : CharacterBody3D
{
    // Ray-casting shenanigans
    private const float RayLeN = 1000f;
    private Camera3D _camera3D;

    private bool _canDash = true;
    [Export] private int _dashCooldown = 2;
    private Timer _dashTimer;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    [Export] public float JumpVelocity = 4.5f;
    [Export] public float Speed = 5.0f;

    public override void _Ready()
    {
        _camera3D = GetNode<Camera3D>("%Camera3D");
        _dashTimer = new Timer();
        AddChild(_dashTimer);
        _dashTimer.OneShot = true;
        _dashTimer.Timeout += () => _canDash = true;
    }

    public override void _Process(double delta)
    {
        var mousePos = GetViewport().GetMousePosition();
        var from = _camera3D.ProjectRayOrigin(mousePos);
        var to = from + _camera3D.ProjectRayNormal(mousePos) * RayLeN;
        var directSpaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.Exclude = new Array<Rid> { GetRid() };
        var intersection = directSpaceState.IntersectRay(query);

        if (Input.GetLastMouseVelocity() != Vector2.Zero && intersection.TryGetValue("position", out var value))
            LookAt((Vector3)value);

        GlobalRotation = new Vector3(0f, GlobalRotation.Y, 0f);
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
            velocity.Y -= _gravity * (float)delta;

        var inputDir = Input.GetVector("left", "right", "forward", "backward");
        var direction = new Vector3(inputDir.X, 0, inputDir.Y).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        if (Input.IsActionJustPressed("space") && _canDash)
        {
            _canDash = false;
            _dashTimer.Start(_dashCooldown);
            velocity = direction * JumpVelocity;
        }

        GD.Print(_dashTimer.TimeLeft);

        Velocity = velocity;
        MoveAndSlide();
    }
}