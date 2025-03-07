using Godot;

namespace Test.Entities.Interaction;

public partial class Box : Node3D {
    private Interactable _interactable;

    public override void _Ready() {
        _interactable = GetNode<Interactable>("RigidBody3D/Interactable");

        _interactable.Focused += OnInteractableFocused;
        _interactable.Unfocused += OnInteractableUnfocused;
        _interactable.Interacted += OnInteractableInteracted;
    }

    private void OnInteractableInteracted(Interactor interactor) { }

    private void OnInteractableUnfocused(Interactor interactor) { }

    private void OnInteractableFocused(Interactor interactor) { }
}
