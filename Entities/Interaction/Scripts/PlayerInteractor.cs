using Godot;

namespace Test.Entities.Interaction;

public partial class PlayerInteractor : Interactor {
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
}
