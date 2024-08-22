using Godot;
using static Godot.GD;

namespace Test.Scripts.Components;

public partial class Bullet : RigidBody3D
{
	private Timer _lifeTimer = new();

	[Export] public float Damage = 20;
	[Export] public bool IsDebugOn;
	[Export] public float LifeTime = 20;
	[Export] public float Speed = 20;

	public override void _Ready()
	{
		ContactMonitor = true;
		MaxContactsReported = 10;
		BodyEntered += OnBodyEntered;

		_lifeTimer.WaitTime = LifeTime;
		_lifeTimer.Autostart = true;
		_lifeTimer.OneShot = true;
		_lifeTimer.Timeout += QueueFree;
		AddChild(_lifeTimer);
	}

	private void OnBodyEntered(Node body)
	{
		if (IsDebugOn) Print("Hit target", body);
		if (body.HasNode("%HpComponent"))
		{
			var hpComponent = body.GetNode<HpComponent>("%HpComponent");
			hpComponent.TakeDamage(Damage);
		}
		else if (body.GetParent().HasNode("HpComponent"))
		{
			var hpComponent = body.GetParent().GetNode<HpComponent>("HpComponent");
			hpComponent.TakeDamage(Damage);
		}

		QueueFree();
	}
}
