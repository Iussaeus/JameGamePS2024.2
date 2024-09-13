using Godot;
using Test.Scripts.Components;
using Test.Scripts.Interaction;

[GlobalClass]
public partial class InventoryItem3D : RigidBody3D {
	private Interactable Interactable;
	private InventoryItemUI ItemUI;
	private Node3D Parent;

	private bool _inInventory = false;

	public override void _Ready() {
		ItemUI = GetParent<InventoryItemUI>();
		Interactable = GetNode<Interactable>("Interactable");

		Interactable.Focused += OnFocused;
		Interactable.Unfocused += OnUnfocused;
		Interactable.Interacted += OnInteracted;

		ItemUI.Visible = false;
	}

	public override void _Process(double delta) {
		if (ItemUI.GetParent() is Node3D) {
			_inInventory = false;
			ItemUI.Visible = false;
		}
		else if (ItemUI.GetParent() is Inventory) {
			_inInventory = true;
		}
	}

	public void Disable() {
		Visible = false;
		ProcessMode = ProcessModeEnum.Disabled;
	}

	public void Enable() {
		Visible = true;
		ProcessMode = ProcessModeEnum.Inherit;
	}

	private void OnInteracted(Interactor interactor) {
		Disable();

		ItemUI.Visible = true;
		ItemUI.Reparent(Globals.Inventory);
		Globals.Inventory.CallDeferred(Inventory.MethodName.PlaceItem, ItemUI);
		GD.Print($"Item: {Name}, Interacted");

	}

	private void OnUnfocused(Interactor interactor) {
		GD.Print($"Item: {Name}, Unfocused");
	}

	private void OnFocused(Interactor interactor) {
		GD.Print($"Item: {Name}, Focused");
	}
}
