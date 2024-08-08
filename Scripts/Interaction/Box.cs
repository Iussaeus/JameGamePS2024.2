using Godot;
using static Godot.GD;

namespace Test.Scripts.Interaction;

public partial class Box : Node3D
{
    private Interactable _interactable;

    [Export] private bool _isDebugOn;
    private bool _isOpen;

    public override void _Ready()
    {
        _interactable = GetNode<Interactable>("RigidBody3D/Interactable");

        _interactable.Focused += OnInteractableFocused;
        _interactable.Unfocused += OnInteractableUnfocused;
        _interactable.Interacted += OnInteractableInteracted;
    }

    private void OnInteractableInteracted(Interactor interactor)
    {
        if (_isDebugOn) Print("Box: Interacted");
    }

    private void OnInteractableUnfocused(Interactor interactor)
    {
        if (_isDebugOn) Print("Box: Unfocused");
    }

    private void OnInteractableFocused(Interactor interactor)
    {
        if (_isDebugOn) Print("Box: Focused");
    }
}