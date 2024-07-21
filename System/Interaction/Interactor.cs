using Godot;

public partial class Interactor : Area3D
{
    private Node3D controller;

    public void Interact(Interactable interactable)
    {
        interactable.EmitSignal(nameof(Interactable.Interacted), this, nameof(Interactable.InteractedEventHandler));
    }

    public void Focus(Interactable interactable)
    {
        interactable.EmitSignal(nameof(Interactable.Focused), this, nameof(Interactable.FocusedEventHandler));
    }

    public void Unfocus(Interactable interactable)
    {
        interactable.EmitSignal(nameof(Interactable.Unfocused), this, nameof(Interactable.UnfocusedEventHandler));
    }

    // Returns the closest overlapping Interactable or null if there isn't one.
    public Interactable GetClosestInteractable()
    {
        var list = GetOverlappingAreas();
        var closestDistance = Mathf.Inf;
        Interactable closest = null;

        foreach (var area in list)
            if (area is Interactable interactable)
            {
                var distance = interactable.GlobalPosition.DistanceTo(GlobalPosition);
                if (distance < closestDistance)
                {
                    closest = interactable;
                    closestDistance = distance;
                }
            }

        return closest;
    }
}