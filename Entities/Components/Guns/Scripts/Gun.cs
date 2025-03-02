using Godot;

namespace Test.Entities.Components;

// TODO: fix the gun
[GlobalClass]
public partial class Gun : RigidBody3D {

    private Marker3D _marker;
    private Node _grandParent;

    [Export] public PackedScene Projectile;
    [Export] public float ProjectileSpeed = 20;
    [Export] public float ShootingInterval = 0.1f;

    [Export] private bool _isFullAuto;
    [Export] private int _magazineCapacity = 10;
    [Export] private float _reloadInterval = 3f;

    public bool IsMouseHeld;

    private Timer _shootTimer = new();
    private Timer _reloadTimer = new();

    private int _currentAmmo;
    private bool _canShoot = true;


    public override void _Ready() {
        _grandParent = GetParent<Node>().GetParent<Node>();
        _marker = GetNode<Marker3D>("Marker3D");

        _reloadTimer = new();
        _currentAmmo = _magazineCapacity;
        _reloadTimer.WaitTime = _reloadInterval;
        _reloadTimer.Timeout += () => {
            _currentAmmo = _magazineCapacity;
            _canShoot = true;
        };

        _reloadTimer.OneShot = true;
        AddChild(_reloadTimer);

        _shootTimer.WaitTime = ShootingInterval;
        _shootTimer.OneShot = true;
        AddChild(_shootTimer);
    }

    public void Shoot() {
        if (_currentAmmo == 0) _canShoot = false;
        if (_shootTimer.IsStopped() && _canShoot && _currentAmmo != 0 && !IsMouseHeld) {
            _shootTimer.Start();
            var bullet = Projectile.Instantiate<RigidBody3D>();
            _grandParent.AddChild(bullet);
            _currentAmmo--;

            // if (GetParent().Equals(Globals.Player))
            //     GD.Print($"{this.Name}, current ammo: {_currentAmmo}");


            bullet.GlobalPosition = _marker.GlobalPosition;
            bullet.ApplyCentralImpulse(_marker.GlobalBasis.Y.Normalized() * ProjectileSpeed);
            if (!_isFullAuto) IsMouseHeld = true;
        }

    }

    public void Reload() {
        if (_reloadTimer.IsStopped()) {
            _canShoot = false;
            // if (GetParent().Equals(Globals.Player))
            //     GD.Print($"{this.Name} reloading");
            _reloadTimer.Start();
        }
    }
}
