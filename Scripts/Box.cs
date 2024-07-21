using Godot;

public partial class Box : Node3D
{
    public Interactable Interactable;
    public bool isOpen = true;

    public void Open()
    {
        GD.Print("Opening Now");
    }

    public override void _Ready()
    {
        Interactable = GetNode<Interactable>("Interactable");
        Interactable.Interacted += _OnInteractableInteracted;
        Interactable.Interact += _OnInteractableInteract;
        Interactable.Focused += _OnInteractableFocused;
        Interactable.Unfocused += _OnInteractableUnfocused;
    }

    private void _OnInteractableInteract()
    {
        GD.Print("WTF DIMA");
    }

    public void _OnInteractableFocused(Interactor interactor)
    {
        GD.Print("Box: Focused");
    }

    public void _OnInteractableInteracted(Interactor interactor)
    {
        GD.Print("Box: Interacted");
    }

    public void _OnInteractableUnfocused(Interactor interactor)
    {
        GD.Print("Box: Unfocused");
    }
}