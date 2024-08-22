using Godot;
using Test.Scripts.Interaction;
using static Godot.GD;

public partial class PlayerInteractor : Area3D
{
    private Interactable _closestInteractable;
    private CharacterBody3D _controller;
    [Export] public bool IsDebugOn;

    public override void _Ready()
    {
        _controller = GetParent<CharacterBody3D>();
        AreaExited += OnAreaExited;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact") && _closestInteractable != null)
            Interact(_closestInteractable);
    }

    public override void _Process(double delta)
    {
        var newClosest = GetClosestInteractable();

        if (newClosest != _closestInteractable)
        {
            if (IsInstanceValid(_closestInteractable)) Unfocus(newClosest);
            if (newClosest != null) Focus(newClosest);
            _closestInteractable = newClosest;
        }
    }

    private void OnAreaExited(Area3D area)
    {
        if (_closestInteractable == area)
        {
            Unfocus(_closestInteractable);
            _closestInteractable = null;
        }
    }

    private void Focus(Interactable interactable)
    {
        if (IsDebugOn) Print("Player: Focused");

        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Focused, interactor);
    }

    private void Unfocus(Interactable interactable)
    {
        if (IsDebugOn) Print("Player: Unfocused");

        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Unfocused, interactor);
    }

    private void Interact(Interactable interactable)
    {
        if (IsDebugOn) Print("Player: Interacted");

        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Interacted, interactor);
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
