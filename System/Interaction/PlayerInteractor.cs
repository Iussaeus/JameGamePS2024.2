using Godot;

public partial class PlayerInteractor : Area3D
{
    private Interactable cachedClosest; // Always knowing the closest
    private Node3D controller;

    [Export] // PLayerInteractor needs reference to the player, that will be assigned to the controller
    public CharacterBody3D player;

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
        var interactor = new Interactor();
        AddChild(interactor);
        interactable.EmitSignal(Interactable.SignalName.Interact, interactor);
    }

    private void Focus(Interactable interactable)
    {
        GD.Print("Focused1");
        var interactor = new Interactor();
        AddChild(interactor);
        interactable.EmitSignal(Interactable.SignalName.Focused, interactor);
    }

    private void Unfocus(Interactable interactable)
    {
        GD.Print("Unfocused1");
        var interactor = new Interactor();
        AddChild(interactor);
        interactable.EmitSignal(Interactable.SignalName.Unfocused, interactor);
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