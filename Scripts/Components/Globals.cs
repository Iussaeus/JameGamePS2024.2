using Godot;

namespace Test.Scripts.Components;

public partial class Globals : Node3D
{
    [Signal]
    public delegate void PlayerSpawnedEventHandler(CharacterBody3D player);
    [Signal]
    public delegate void CameraSpawnedEventHandler(Camera3D camera);

	public static Globals Instance {get; private set;}

    public static CharacterBody3D Player { get; set; }
    public static Camera3D Camera { get; set; }

    public override void _Ready()
    {
		Instance = this;

        PlayerSpawned += (player => Player = player);
        CameraSpawned += (camera => Camera = camera);
    }
}
