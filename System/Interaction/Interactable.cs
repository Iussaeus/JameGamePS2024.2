using Godot;

public partial class Interactable : Area3D
{
    [Signal]
    public delegate void FocusedEventHandler(Interactor interactor);

    [Signal]
    public delegate void UnfocusedEventHandler(Interactor interactor);

    [Signal]
    public delegate void InteractedEventHandler(Interactor interactor);
}