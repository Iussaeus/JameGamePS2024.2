using Godot;
using Godot.Collections;
using Test.Scripts;

public partial class PlayerBody : CharacterBody3D
{
    [Export] public const float Speed = 5.0f;

    [Export] public const float JumpVelocity = 4.5f;

    // Raycasting shenanigans
    public const float RayLeN = 1000f;
    public Camera3D Camera3D;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float Gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    public override void _Ready()
    {
        Camera3D = GetParent<SubViewport>().GetNodeOrNull<Camera3D>("Camera3D");
        this.AssertMeDaddy(Camera3D != null, "HAHAHAHAHAHA");
    }

    public override void _Process(double delta)
    {
        var mousePos = GetViewport().GetMousePosition();
        var from = Camera3D.ProjectRayOrigin(mousePos);
        var to = from + Camera3D.ProjectRayNormal(mousePos) * RayLeN;
        var directSpaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.Exclude = new Array<Rid> { GetRid() };
        var intersection = directSpaceState.IntersectRay(query);

        if (Input.IsActionPressed("right_click") && intersection.TryGetValue("position", out var value))
            LookAt((Vector3)value);

        GlobalRotation = new Vector3(0f, GlobalRotation.Y, 0f);
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y -= Gravity * (float)delta;

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            velocity.Y = JumpVelocity;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        var inputDir = Input.GetVector("left", "right", "forward", "backward");
        var direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
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

        Velocity = velocity;
        MoveAndSlide();
    }
}