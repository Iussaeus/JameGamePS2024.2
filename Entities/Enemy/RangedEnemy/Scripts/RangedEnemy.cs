using Godot;
using Test.Scripts.Components;

public partial class RangedEnemy : CharacterBody3D
{
	[Export] public float MovementSpeed = 20;
	[Export] public bool IsDebugOn = false;

	private NavigationAgent3D _navigationAgent;
	private Area3D _dangerZone;
	private Area3D _safeZone;
	private Gun _gun;

	private bool _isPlayerVisible = false;
	private bool _isPlayerInDangerZone = false;
	private bool _isPlayerInSafeZone = false;

	public override void _Ready()
	{
		SetPhysicsProcess(false);
		CallDeferred(MethodName.SetMap);

		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
		_dangerZone = GetNode<Area3D>("DangerArea3D");
		_safeZone = GetNode<Area3D>("SafeArea3D");
		_gun = GetNode<Gun>("Gun");

		_navigationAgent.VelocityComputed += OnVelocityComputed;

		_safeZone.BodyEntered += OnSafeZoneEntered;
		_safeZone.BodyExited += OnSafeZoneExited;

		_dangerZone.BodyEntered += OnDangerZoneEntered;
		_dangerZone.BodyExited += OnDangerZoneExited;
	}

	public override void _PhysicsProcess(double delta)
	{
		var nextPathPos = _navigationAgent.GetNextPathPosition();

		var newVelocity = Velocity;

		if (_isPlayerVisible && _isPlayerInSafeZone) {
			_gun.Shoot();
			newVelocity.X = 0;
			newVelocity.Z = 0;
		}
		else if (_isPlayerInDangerZone) {
			newVelocity.X = nextPathPos.DirectionTo(GlobalPosition).X * MovementSpeed;
			newVelocity.Z = nextPathPos.DirectionTo(GlobalPosition).Z * MovementSpeed;
		}
		else newVelocity = GlobalPosition.DirectionTo(nextPathPos) * MovementSpeed; 

		if (_navigationAgent.AvoidanceEnabled) _navigationAgent.Velocity = newVelocity;
		else OnVelocityComputed(newVelocity);
	}

	private void SetMovementTarget(Vector3 target) {
		_navigationAgent.TargetPosition = target;
	}

	private void OnDangerZoneEntered(Node3D body)
	{
		if (body is CharacterBody3D player) {
			_isPlayerInDangerZone = true;
			_isPlayerInSafeZone = false;
			if (IsDebugOn) GD.Print("Player in DangerZone");
		}
	}

	private void OnDangerZoneExited(Node3D body)
	{
		if (body is CharacterBody3D player) {
			_isPlayerInDangerZone = false;
			_isPlayerInSafeZone = true;
			if (IsDebugOn) GD.Print("Player left DangerZone");
		}
	}

	private void OnSafeZoneEntered(Node3D body)
	{
		if (body is CharacterBody3D player) {
			_isPlayerInSafeZone = true;
			_isPlayerVisible = true;
			if (IsDebugOn) GD.Print("Player in SafeZone");
		}

	}

	private void OnSafeZoneExited(Node3D body)
	{
		if (body is CharacterBody3D player) {
			_isPlayerInSafeZone = false;
			_isPlayerVisible = false;
			if (IsDebugOn) GD.Print("Player left SafeZone");
		}
	}

	private void OnVelocityComputed(Vector3 safeVelocity)
	{
		SetMovementTarget(Globals.Player.GlobalPosition);
		LookAt(Globals.Player.GlobalPosition);

		Velocity = safeVelocity;
		MoveAndSlide();
	}

	private async void SetMap() {
		await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
		SetPhysicsProcess(true);
	}

}
