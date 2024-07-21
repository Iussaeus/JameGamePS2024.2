using Godot;

public partial class Box : Node3D
{
    public bool isOpen = false;

    public void Open()
    {
        GD.Print("Opening Now");
    }

    public override void _Ready()
    {
        var interactable = GetNode<Interactable>("Interactable");
        interactable.Connect(nameof(Interactable.Focused), new Callable(this, nameof(_OnInteractableFocused)));
        interactable.Connect(nameof(Interactable.Interacted), new Callable(this, nameof(_OnInteractableInteracted)));
        interactable.Connect(nameof(Interactable.Unfocused), new Callable(this, nameof(_OnInteractableUnfocused)));
    }

    public void _OnInteractableFocused(Interactor interactor)
    {
        if (!isOpen) GD.Print("Focused");
    }

    public void _OnInteractableInteracted(Interactor interactor)
    {
        if (!isOpen)
        {
            GD.Print("Interacted");
            QueueFree(); // $Interactable.queue_free() from gdscript
        }
    }

    public void _OnInteractableUnfocused(Interactor interactor)
    {
        GD.Print("Unfocused");
    }
}