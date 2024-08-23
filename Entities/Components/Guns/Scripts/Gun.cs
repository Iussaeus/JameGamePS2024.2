using Godot;

namespace Test.Scripts.Components;

[GlobalClass]
public partial class Gun : RigidBody3D
{
	private Marker3D _marker;
	private Node _grandParent;

	[Export] private bool _isFullAuto;
	[Export] private int _magazineCapacity = 10;
	[Export] private float _reloadInterval = 3f;
	private Timer _reloadTimer = new();

	private int _currentAmmo;
	private bool _canShoot = true;
	private bool _isMouseHeld;

	[Export] public PackedScene Projectile;
	[Export] public float ProjectileSpeed = 20;
	[Export] public float ShootingInterval = 0.1f;
	private Timer _shootTimer = new();

	public override void _Ready()
	{
		_grandParent = GetParent<Node>().GetParent<Node>();
		_marker = GetNode<Marker3D>("Marker3D");

		_reloadTimer = new();
		_currentAmmo = _magazineCapacity;
		_reloadTimer.WaitTime = _reloadInterval;
		_reloadTimer.Timeout += () =>
		{
			_currentAmmo = _magazineCapacity;
			_canShoot = true;
		};
		_reloadTimer.OneShot = true;
		AddChild(_reloadTimer);

		_shootTimer.WaitTime = ShootingInterval;
		_shootTimer.OneShot = true;
		AddChild(_shootTimer);
	}

	public override void _Process(double delta)
	{
		var inputMap = InputMap.GetActions();
		foreach (var input in inputMap)
		{
			if (input.Equals("left_click") && Input.IsActionPressed("left_click"))
				Shoot();
			if (input.Equals("reload") && Input.IsActionJustPressed("reload"))
				Reload();
		}
		if (Input.IsActionJustReleased("left_click")) _isMouseHeld = false;
	}

	public void Shoot()
	{
		if (_currentAmmo == 0) _canShoot = false;
		if (_shootTimer.IsStopped() && _canShoot && _currentAmmo != 0 && !_isMouseHeld)
		{
			_shootTimer.Start();
			var bullet = Projectile.Instantiate<RigidBody3D>();
			_grandParent.AddChild(bullet);
			GD.Print($"{this.Name}, current ammo: {_currentAmmo}");
			_currentAmmo--;

			bullet.GlobalPosition = _marker.GlobalPosition;
			bullet.ApplyCentralImpulse(_marker.GlobalBasis.Y.Normalized() * ProjectileSpeed);
			if (!_isFullAuto) _isMouseHeld = true;
		}

	}

	public void Reload()
	{
		if (_reloadTimer.IsStopped() && _currentAmmo == 0 && !_canShoot)
		{
			GD.Print($"{this.Name} reloading");
			_reloadTimer.Start();
		}
	}
}
