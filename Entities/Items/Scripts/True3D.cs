using Godot;
using Test.Scripts.Components;
using Test.Scripts.Interaction;

public partial class True3D : InventoryItem3D
{
	private Interactable Interactable;

	public override void _Ready()
	{
		Interactable = GetNode<Interactable>("Interactable");

		Interactable.Focused += OnFocused;
		Interactable.Unfocused += OnUnfocused;
		Interactable.Interacted += OnInteracted;
	}

	private void OnInteracted(Interactor interactor)
	{
		GD.Print($"Item: {Name}, Interacted");
	}

	private void OnUnfocused(Interactor interactor)
	{
		GD.Print($"Item: {Name}, Unfocused");
	}

	private void OnFocused(Interactor interactor)
	{
		GD.Print($"Item: {Name}, Focused");
	}
}
