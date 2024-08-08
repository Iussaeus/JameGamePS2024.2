using Godot;

public partial class Navigation: NavigationRegion3D {

    public override void _Ready()
    {
		BakeNavigationMesh();
    }

}
