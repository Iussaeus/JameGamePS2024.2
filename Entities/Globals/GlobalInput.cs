using Godot;
using Test.Entities.Player;
using Test.Entities.Console;
using Test.Utils.Extensions;

namespace Test.Entities.Global;

public partial class GlobalInput : Node {
    public static GlobalInput Instance;

    private static PlayerController _player;
    private static Inventory _inventory;
    private static ConsoleWindow _console;
    private static Camera3D _camera;

    public override void _EnterTree() {
        Instance = this;
    }

    public override async void _Ready() {
        _console = (ConsoleWindow)(await Globals.Instance.AwaitSignal(SignalBus.SignalName.ConsoleSpawned))[0];
        _inventory = (Inventory)(await Globals.Instance.AwaitSignal(SignalBus.SignalName.InventorySpawned))[0];
        _player = (PlayerController)(await Globals.Instance.AwaitSignal(SignalBus.SignalName.PlayerSpawned))[0];
        _camera = _player.GetNode<Camera>("Camera3D");

        this.Assert(_console != null, "Console is null");
        this.Assert(_inventory != null, "Inventory is null");
        this.Assert(_player != null, "Player is null");
        this.Assert(_camera != null, "Camera is null");
    }

    public override void _Process(double delta) {
        if (!_console.IsOpened) {
            ProcessContinuousInvetoryInput();
            ProcessContinuousGunInput();
        }
    }

    public override void _PhysicsProcess(double delta) {
        if (!_console.IsOpened) {
            ProcessContinuousPlayerInput((float)delta);
        }
    }

    public override void _Input(InputEvent @event) {
        if (!_console.IsOpened) {
            ProcessSingularPlayerInput();

            ProcessSingularGunInput();

            ProcessPlayerInteractorInput(@event);

            if (_inventory.IsOpen) {
                ProcessSingularInventoryInput();

            }
            if (Input.IsActionJustPressed("inventory")) {
                if (_inventory.IsOpen) _inventory.Close();
                else _inventory.Open();
            }
        }

    }

    public void ProcessContinuousGunInput() {
        var gun = _player.Gun;

        if (Input.IsActionPressed("left_click")) gun.Shoot();
        if (Input.IsActionJustReleased("left_click")) gun.IsMouseHeld = false;
    }

    public void ProcessSingularGunInput() {
        var gun = _player.Gun;

        if (Input.IsActionJustPressed("reload")) gun.Reload();
    }

    public void ProcessContinuousInvetoryInput() {
        if (_inventory.IsItemSelected || _inventory.IsDraggingItem) {
            _inventory.MoveSelectedItem();
        }
    }

    public void ProcessSingularInventoryInput() {
        // // TODO: make the selected item rotate around it's center (Optional)
        // if (Input.IsActionJustPressed("interact") && _isItemSelected) {
        // 	var oldPivot = _selectedItem.PivotOffset;
        // 	_selectedItem.Rotation += Mathf.DegToRad(90);
        // 	GD.Print($"old:{oldPivot} new: {_selectedItem.PivotOffset}, rotation: {_selectedItem.RotationDegrees}");
        // }

#if DEBUG
        if (Input.IsActionJustPressed("cancel")) {
            foreach (var node in _inventory.GetChildren()) {
                if (node is InventoryItemUI i) {
                    _inventory.PlaceItem(i);
                }
            }
        }

        if (Input.IsActionJustPressed("test")) {
            _inventory.PrintMatrix(false);
            // GD.Print(ItemsPositions);
        }

        if (Input.IsActionJustPressed("reload")) {
            _inventory.PlaceDummyItem();
        }
#endif

    }

    public void ProcessContinuousPlayerInput(double delta) {
        var direction = GetDirection();

        if (!_player.DashTimer.IsStopped()) _player.Dash((float)delta);
        else _player.Move((float)delta, direction);

        if (!_inventory.IsOpen)
            _player.Rotate();
    }

    public void ProcessSingularPlayerInput() {
        var direction = GetDirection();

        if (Input.IsActionJustPressed("space") && _player.CanDash /* && GetDirection() != Vector3.Zero */) {
            _player.StartDash(direction);
        }
    }

    public void ProcessPlayerInteractorInput(InputEvent @event) {
        if (@event.IsActionPressed("interact") && _player.Interactor.ClosestInteractable != null)
            _player.Interactor.Interact(_player.Interactor.ClosestInteractable);
    }


    public Vector3 GetDirection() {
        var inputDir = Input.GetVector("left", "right", "forward", "backward");
        var direction = new Vector3(0, 0, 0);

        var upMarker = _camera.GetNode<Marker3D>("Marker3DUp");
        var rightMarker = _camera.GetNode<Marker3D>("Marker3DRight");
        var upDirection = _camera.GlobalPosition.DirectionTo(upMarker.GlobalPosition).Normalized();
        var rightDirection = _camera.GlobalPosition.DirectionTo(rightMarker.GlobalPosition).Normalized();

        if (inputDir != Vector2.Zero) {
            if (inputDir.Y > 0) {
                direction += -upDirection;
            }
            if (inputDir.Y < 0) {
                direction += upDirection;
            }
            if (inputDir.X > 0) {
                direction += rightDirection;
            }
            if (inputDir.X < 0) {
                direction += -rightDirection;
            }
        }

        return direction.Normalized();
    }
}
