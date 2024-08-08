using Godot;
using Test.Scripts.Helpers;

namespace Test.Scripts.Components;

public partial class Globals : Node3D
{
	public static Camera3D Camera;
	public static CharacterBody3D Player;

	public override void _Ready()
	{
		Camera = GetNode<Camera3D>("/root/World/SubViewportContainer/SubViewport/Camera3D");
		Player = GetNode<CharacterBody3D>("/root/World/SubViewportContainer/SubViewport/PlayerBody/CharacterBody3D");
	}
}
