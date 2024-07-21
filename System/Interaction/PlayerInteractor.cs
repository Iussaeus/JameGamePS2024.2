using Godot;

public partial class PlayerInteractor : Area3D
{
    [Export] // PLayerInteractor needs reference to the player, that will be assigned to the controller
    public CharacterBody3D player;

    private Interactable cachedClosest; // Always knowing the closest
    private Node3D controller;

    public override void _Ready()
    {
        controller = player;
    }

    public override void _Process(double delta)
    {
        var newClosest = GetClosestInteractable();

        if (newClosest != cachedClosest)
        {
            if (IsInstanceValid(cachedClosest)) Unfocus(cachedClosest);
            if (newClosest != null) Focus(newClosest);

            cachedClosest = newClosest;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
            if (cachedClosest != null)
                Interact(cachedClosest);
    }

    public void _OnAreaExited(Interactable area)
    {
        if (cachedClosest == area)
        {
            Unfocus(cachedClosest);
            cachedClosest = null;
        }
    }

    private void Interact(Interactable interactable)
    {
        GD.Print("AYAYAY");
        interactable.EmitSignal(nameof(Interactable.Interacted), this, nameof(Interactable.InteractedEventHandler));
    }

    private void Focus(Interactable interactable)
    {
        GD.Print("Focused1");
        interactable.EmitSignal(nameof(Interactable.Focused), this, nameof(Interactable.FocusedEventHandler));
    }

    private void Unfocus(Interactable interactable)
    {
        GD.Print("Unfocused1");
        interactable.EmitSignal(nameof(Interactable.Unfocused), this, nameof(Interactable.UnfocusedEventHandler));
    }

    private Interactable GetClosestInteractable()
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