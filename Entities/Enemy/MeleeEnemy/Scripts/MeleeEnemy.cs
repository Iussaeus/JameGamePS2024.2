using Godot;
using Test.Entities.Global;
using Test.Entities.Player;

namespace Test.Entities.Enemy;

public partial class MeleeEnemy : CharacterBody3D {
    private NavigationAgent3D _navigationAgent;

    [Export] public bool IsDebugOn;
    [Export] public float MovementSpeed = 20;
    private PlayerController _player;

    public override async void _Ready() {
        _navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

        SetPhysicsProcess(false);
        CallDeferred(MethodName.SetMap);
        _player = await Globals.Instance.AwaitSignalSingle<PlayerController>(SignalBus.SignalName.PlayerSpawned);

        _navigationAgent.VelocityComputed += OnVelocityComputed;
    }

    public override void _PhysicsProcess(double delta) {
        var nextPathPos = _navigationAgent.GetNextPathPosition();
        var newVelocity = GlobalPosition.DirectionTo(nextPathPos) * MovementSpeed * (float)delta;

        if (_navigationAgent.AvoidanceEnabled) _navigationAgent.TargetPosition = newVelocity;
        else OnVelocityComputed(newVelocity);

    }

    private void SetMovementTarget(Vector3 target) {
        _navigationAgent.TargetPosition = target;
    }

    private void OnVelocityComputed(Vector3 safeVelocity) {
        SetMovementTarget(_player.GlobalPosition);
        LookAt(_player.GlobalPosition);
        GlobalRotation = GlobalRotation with { X = 0, Y = GlobalRotation.Y, Z = 0 };

        Velocity = safeVelocity;

        MoveAndSlide();
    }

    private async void SetMap() {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        SetPhysicsProcess(true);
    }
}
