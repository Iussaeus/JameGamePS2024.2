using System.Collections.Generic;
using Godot;
using Test.Scripts.Components;

[GlobalClass]
[Tool]
public partial class Item : Node {
	public InventoryItemUI ItemUI;
	public InventoryItem3D Item3D;

	public bool InUI = false;
	public bool In3D = true;

	private bool _hasUI;
	private bool _has3D;

	public override void _Ready() {
		if (Engine.IsEditorHint()) {
			ChildOrderChanged += CheckChildren;
			CheckChildren();
		}

		if (!Engine.IsEditorHint()) {
			ItemUI = GetNode<InventoryItemUI>("InventoryItem");
			Item3D = GetNode<InventoryItem3D>("InventoryItem3d");
		}
	}

	public override void _Process(double delta) {
		if (!Engine.IsEditorHint()) {
			if (InUI) {
				Item3D.Visible = false;
				Item3D.ProcessMode = ProcessModeEnum.Disabled;
			}
			else {
				Item3D.Visible = true;
				Item3D.ProcessMode = ProcessModeEnum.Inherit;
			}

			if (In3D) {
				ItemUI.Visible = false;
			}
			else {
				ItemUI.Visible = true;
			}
		}
	}

	public void CheckChildren() {
		_has3D = false;
		_hasUI = false;

		foreach (var child in GetChildren()) {
			switch (child) {
				case InventoryItemUI:
					_hasUI = true;
					break;
				case InventoryItem3D:
					_has3D = true;
					break;
			}
		}

		if (!_has3D || !_hasUI)
			UpdateConfigurationWarnings();
	}

	public override string[] _GetConfigurationWarnings() {
		var warnings = new List<string>();

		if (!_hasUI) {
			warnings.Add(new string("There is no InvetoryItemUI"));
		}
		if (!_has3D) {
			warnings.Add(new string("There is no IventoryItem3D"));
		}

		return warnings.ToArray();
	}
}
