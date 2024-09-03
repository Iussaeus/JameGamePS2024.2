using Godot;

public partial class NavigationTest : Node3D
{
	private StaticBody3D Obstacle;

	public override void _Ready()
	{
		Obstacle = GetNode<StaticBody3D>("Obstacle");
	}

	public override void _Process(double delta)
	{
	}
}
