using System.Collections.Generic;
using Godot;

[GlobalClass]
[Tool]
// TODO: figure out how to add the itemUI to the inventory
public partial class Item : Node
{
    public InventoryItemUI ItemUI;
    public InventoryItem3D Item3D;

    public bool InUI;
    public bool In3D;

    private bool _hasUI;
    private bool _has3D;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            ChildOrderChanged += CheckChildren;
            CheckChildren();
        }

        if (!Engine.IsEditorHint())
        {
            ItemUI = GetNode<InventoryItemUI>("InventoryItem");
            Item3D = GetNode<InventoryItem3D>("InventoryItem3d");
        }
    }

    public override void _Process(double delta)
    {
    }

    public void CheckChildren()
    {
        _has3D = false;
        _hasUI = false;

        foreach (var child in GetChildren())
        {
            switch (child)
            {
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

    public override string[] _GetConfigurationWarnings()
    {
        var warnings = new List<string>();

        if (!_hasUI)
        {
            warnings.Add(new string("There is no InvetoryItemUI"));
        }
        if (!_has3D)
        {
            warnings.Add(new string("There is no IventoryItem3D"));
        }

        return warnings.ToArray();
    }
}
