using Godot;

namespace Test.Scripts.Interaction;

public partial class Interactor : Area3D
{
    private CharacterBody3D _controller;

    public void Interact(Interactable interactable)
    {
        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Interacted, interactor);
    }

    public void Focus(Interactable interactable)
    {
        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Focused, interactor);
    }

    public void Unfocus(Interactable interactable)
    {
        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Unfocused, interactor);
    }

    public Interactable GetClosestInteractable()
    {
        var list = GetOverlappingAreas();
        var closestDistance = Mathf.Inf;
        Interactable closestInteractable = null;

        foreach (var area3D in list)
            if (area3D is Interactable interactable)
            {
                var distance = interactable.GlobalPosition.DistanceTo(GlobalPosition);

                if (distance < closestDistance)
                {
                    closestInteractable = interactable;
                    closestDistance = distance;
                }
            }

        return closestInteractable;
    }
}