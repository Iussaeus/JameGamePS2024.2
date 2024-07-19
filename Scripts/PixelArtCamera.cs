using Godot;

public partial class PixelArtCamera : Camera3D
{
	// Called when the node enters the scene tree for the first time.
	public Camera3D Camera3D;
	
	// TODO: Proper camera rotation towards player
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		LookAt(GetParent<CharacterBody3D>().Position);
	}
}
