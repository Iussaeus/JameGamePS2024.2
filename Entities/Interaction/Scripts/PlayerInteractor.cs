using Godot;

namespace Test.Entities.Interaction;

public partial class PlayerInteractor : Area3D {
    public Interactable ClosestInteractable;
    private CharacterBody3D _controller;

    [Export] public bool IsDebugOn;

    public override void _Ready() {
        _controller = GetParent<CharacterBody3D>();
        AreaExited += OnAreaExited;
    }

    public override void _Process(double delta) {
        var newClosest = GetClosestInteractable();

        if (newClosest != ClosestInteractable) {
            if (IsInstanceValid(ClosestInteractable)) Unfocus(newClosest);
            if (newClosest != null) Focus(newClosest);
            ClosestInteractable = newClosest;
        }
    }

    private void OnAreaExited(Area3D area) {
        if (ClosestInteractable == area) {
            Unfocus(ClosestInteractable);
            ClosestInteractable = null;
        }
    }

    private void Focus(Interactable interactable) {
        if (IsDebugOn) GD.Print("Player: Focused");

        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Focused, interactor);
    }

    private void Unfocus(Interactable interactable) {
        if (IsDebugOn) GD.Print("Player: Unfocused");

        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Unfocused, interactor);
    }

    public void Interact(Interactable interactable) {
        if (IsDebugOn) GD.Print("Player: Interacted");

        var interactor = new Interactor();
        interactable.EmitSignal(Interactable.SignalName.Interacted, interactor);
    }

    public Interactable GetClosestInteractable() {
        var list = GetOverlappingAreas();
        var closestDistance = Mathf.Inf;
        Interactable closestInteractable = null;

        foreach (var area3D in list)
            if (area3D is Interactable interactable) {
                var distance = interactable.GlobalPosition.DistanceTo(GlobalPosition);

                if (distance < closestDistance) {
                    closestInteractable = interactable;
                    closestDistance = distance;
                }
            }

        return closestInteractable;
    }
}
