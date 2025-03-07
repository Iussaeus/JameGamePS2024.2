using Godot;

namespace Test.Entities.Interaction;

public partial class Interactor : Area3D {
    private CharacterBody3D _controller;

    public void Interact(Interactable interactable) {
        interactable.EmitSignal(Interactable.SignalName.Interacted, this);
    }

    public void Focus(Interactable interactable) {
        interactable.EmitSignal(Interactable.SignalName.Focused, this);
    }

    public void Unfocus(Interactable interactable) {
        interactable.EmitSignal(Interactable.SignalName.Unfocused, this);
    }

    public Interactable GetClosestInteractable() {
        var list = GetOverlappingAreas();
        var closestDistance = Mathf.Inf;
        Interactable closestInteractable = null;

        foreach (var area3D in list)
            if (area3D is Interactable interactable) {
                var distance = interactable.GlobalPosition.DistanceSquaredTo(GlobalPosition);

                if (distance < closestDistance) {
                    closestInteractable = interactable;
                    closestDistance = distance;
                }
            }

        return closestInteractable;
    }
}
