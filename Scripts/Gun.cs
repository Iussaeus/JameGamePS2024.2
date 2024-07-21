using Godot;

namespace Test.Scripts;

public partial class Gun : RigidBody3D
{
    private Marker3D _marker;
    private Viewport _viewport;

    // Called when the node enters the scene tree for the first time.
    [Export] public PackedScene Projectile;
    [Export] public float ProjectileSpeed;
    [Export] public float ShootingInterval;

    public override void _Ready()
    {
        _marker = GetNode<Marker3D>("Marker3D");
        _viewport = GetParent().GetParent<Viewport>();
    }

    public void Shoot()
    {
        var bullet = Projectile.Instantiate<RigidBody3D>();
        _viewport.AddChild(bullet);

        bullet.GlobalPosition = _marker.GlobalPosition;
        bullet.ApplyImpulse(_marker.GlobalBasis.Y * ProjectileSpeed);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("left_click")) Shoot();
    }
}