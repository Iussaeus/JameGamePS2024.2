using Godot;

// TODO: remove this shit
public partial class NavigationTest : Node3D
{
	private StaticBody3D Obstacle;

	public override void _Ready()
	{
		Obstacle = GetNode<StaticBody3D>("Obstacle");
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("cancel"))
		{
			Obstacle.ProcessMode = Obstacle.ProcessMode == ProcessModeEnum.Disabled ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
			Obstacle.Visible = Obstacle.Visible == true ? false : true;
		}
	}
}
