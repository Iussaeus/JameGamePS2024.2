using Godot;

namespace Test.Scripts.Components;

public partial class Gun : RigidBody3D
{
    private Marker3D _marker;
    private Node3D _parent;

    private Timer _shootTimer = new();

    [Export] public PackedScene Projectile;
    [Export] public float ProjectileSpeed = 20;
    [Export] public float ShootingInterval = 0.1f;

    public override void _Ready()
    {
        _parent = GetParent<Node3D>().GetParent<Node3D>();
        _marker = GetNode<Marker3D>("Marker3D");

        _shootTimer.WaitTime = ShootingInterval;
        _shootTimer.OneShot = true;
        AddChild(_shootTimer);
    }

    public override void _Process(double delta)
    {
        var inputMap = InputMap.GetActions();
        foreach (var input in inputMap)
            if (input.Equals("left_click") && Input.IsActionPressed("left_click"))
                Shoot();
    }

    public void Shoot()
    {
        if (_shootTimer.IsStopped())
        {
            _shootTimer.Start();
            var bullet = Projectile.Instantiate<RigidBody3D>();
            _parent.AddChild(bullet);

            bullet.GlobalPosition = _marker.GlobalPosition;
            bullet.ApplyCentralImpulse(_marker.GlobalBasis.Y.Normalized() * ProjectileSpeed);
        }
    }
}
