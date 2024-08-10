using Godot;
using Test.Scripts.Components;

namespace Test.Scripts.Player;

public partial class Camera : Camera3D
{
	private CharacterBody3D _player;
	[Export] public Vector3 CameraOffset = new(30, 60, 40);

	public override void _Ready()
	{
		Globals.Instance.EmitSignal(Globals.SignalName.CameraSpawned, this);

		_player = Globals.Player;
	}

	public override void _Process(double delta)
	{
		LookAtFromPosition(_player.GlobalPosition + CameraOffset, _player.GlobalPosition);
	}
}
