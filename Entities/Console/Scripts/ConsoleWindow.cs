using Godot;
using System.Collections.Generic;
using Test.Entities.Global;
using Test.Utils.Extensions;
using Test.Utils;

namespace Test.Entities.Console;

public partial class ConsoleWindow : Control {
    public bool IsOpened;

    private List<string> _history = new();
    private Dictionary<string, System.Delegate> _commands = new();
    private List<Dictionary<object, object>> _savedCommands = new();
    private CodeEdit _textBox;
    private int _currentIdx = 0;
    private bool _requesting;

    public override void _Ready() {
        SignalBus.Instance.EmitSignal(SignalBus.SignalName.ConsoleSpawned, this);

        _textBox = GetNode<CodeEdit>("CenterContainer/CodeEdit");
        _textBox.Editable = true;
        _textBox.CaretBlink = true;
        this.Assert(_textBox != null, "Console doesn't have a CodeEdit");

        Visible = false;
        _textBox.ReleaseFocus();
        _textBox.GetCodeCompletionOptions();
    }

    public override void _Input(InputEvent @event) {
        // NOTE: the  `  is printed when exiting console
        if (Input.IsActionJustReleased("console")) {
            Visible = Visible ? false : true;
            if (Visible) {
                IsOpened = true;
                _textBox.GrabFocus();
                BlockInput();
            }
            else {
                IsOpened = false;
                _textBox.ReleaseFocus();
                UnblockInput();
            }
            _textBox.Clear();
        }

        if (Input.IsActionJustReleased("cancel") && IsOpened) {
            Visible = false;
            IsOpened = false;
            _textBox.ReleaseFocus();
            UnblockInput();

            _textBox.Clear();
        }

        if (@event.IsActionReleased("enter") && !_requesting) {
            var text = _textBox.Text.StripEscapes();
            AddHistoryItem(text);

            var (command, args) = ParseCommandAndArgs(text);
            CallCommand(command, args);

            _textBox.Clear();
        }

        if (@event.IsActionPressed("enter") && _requesting) {
            _requesting = false;
            _textBox.ConfirmCodeCompletion();
        }

        if (@event.IsActionReleased("tab") && !_requesting) {
            _requesting = true;
            RequestCompletion();
        }
        else {
            _requesting = false;
            _textBox.ConfirmCodeCompletion();
        }
    }

    public override void _Process(double delta) {
        if (Visible && !_textBox.HasFocus()) _textBox.GrabFocus();

        if (Input.IsActionJustPressed("ui_up")) {
            _currentIdx = _currentIdx - 1 >= 0 ? _currentIdx - 1 : _currentIdx;

            // GD.PrintS(_history.Count, _currentIdx, _history[_currentIdx], _requesting);
            this.Assert(_currentIdx < _history.Count, $"Current index too small:{_currentIdx}, should be >= than 0");

            _textBox.Clear();
            _textBox.Text = _history[_currentIdx];
        }
        if (Input.IsActionJustPressed("ui_down")) {
            _currentIdx = _currentIdx + 1 <= _history.Count ? _currentIdx + 1 : _currentIdx;

            // GD.PrintS(_history.Count, _currentIdx, _currentIdx == _history.Count ? " " : _history[_currentIdx], _requesting);
            this.Assert(_currentIdx <= _history.Count, $"Current index too big:{_currentIdx}, should be smaller than {_history.Count + 1}");

            _textBox.Clear();
            if (_currentIdx == _history.Count) _textBox.Text = "";
            else _textBox.Text = _history[_currentIdx];
        }
    }

    public void AddHistoryItem(string text) {
        if (_history.Contains(text) || text == "" || text == " ")
            return;

        AddCompletionItem(text, "history");

        _history.Add(text);
        _currentIdx = _history.Count;
    }

    public void RequestCompletion() {
        foreach (var com in _savedCommands) {
            var disp = (string)(com["display_text"]);
            var kind = disp.Contains("[history]") ? CodeEdit.CodeCompletionKind.Function : CodeEdit.CodeCompletionKind.PlainText;

            _textBox.CodeCompletionPrefixes.Add((string)com["insert_text"]);
            _textBox.AddCodeCompletionOption(
                                kind,
                                (string)com["display_text"],
                                (string)com["insert_text"],
                                (Color)com["text_color"]);
        }

        _textBox.UpdateCodeCompletionOptions(true);
        _textBox.RequestCodeCompletion();
    }

    public void AddCompletionItem(string text, string type) {
        var item = new Dictionary<object, object>();
        item["display_text"] = text + " " + $"[{type}]";
        item["insert_text"] = text;
        item["text_color"] = Colors.White;

        _savedCommands.Add(item);
    }

    public void AddCommand(System.Delegate @delegate) {
        var name = @delegate.Method.Name;

        AddCompletionItem(name.ToSnakeCase(), "command");
        _commands.Add(name.ToSnakeCase(), @delegate);
    }

    public void CallCommand(string command, params System.Object[] args) {
        System.Delegate @delegate;

        if (command == null || args == null) {
            GD.PushError("Command not found.");
            return;
        }

        if (!_commands.TryGetValue(command, out @delegate)) {
            GD.PushError("Command not found.");
            return;
        }

        if (args.Length != @delegate.Method.GetParameters().Length) {
            GD.PushError($"Arg count mismatch: {args.Length} passed, expected {@delegate.Method.GetParameters().Length}");
            return;
        }

        var (ok, result) = Helpers.PCall(@delegate, args);

        if (result is System.Exception e)
            GD.PushError(e.Message);
    }

    public (string command, object[] args) ParseCommandAndArgs(string text) {
        if (text == "" || text == " ")
            return (null, null);
        var strippedText = text.StripEscapes();
        var splitText = text.Split(" ", System.StringSplitOptions.RemoveEmptyEntries);

        // GD.Print("split");
        // splitText.Print();

        var command = splitText[0];
        var strArgs = splitText[1..];

        System.Delegate @delegate;

        if (!_commands.TryGetValue(command, out @delegate)) {
            GD.PushError("Command not found");
            return (null, null);
        }

        var delArgs = @delegate.Method.GetParameters();
        var objArgs = new object[delArgs.Length];
        if (strArgs.Length == 0 && delArgs.Length == 0)
            return (command, new string[0]);

        if (strArgs.Length > 0) {
            // GD.PrintS(command, _commands.ContainsKey(command), splitText.Length);

            for (int i = 0; i < delArgs.Length; i++) {
                var type = delArgs[i].ParameterType;

                if (type == typeof(int)) objArgs[i] = int.Parse(strArgs[i]);
                if (type == typeof(float)) objArgs[i] = float.Parse(strArgs[i]);
                if (type == typeof(double)) objArgs[i] = float.Parse(strArgs[i]);
                if (type == typeof(bool)) objArgs[i] = bool.Parse(strArgs[i]);
                if (type == typeof(string)) objArgs[i] = strArgs[i];

                if (type.BaseType == typeof(System.Array)) {
                    var varArgs = new object[strArgs.Length - i];
                    if (strArgs.Length < delArgs.Length) {
                        for (int j = 0; j < varArgs.Length; j++) {
                            varArgs[j] = strArgs[j + i];
                        }
                    }
                    objArgs[i] = varArgs;
                    varArgs.Print();
                    break;
                }
            }

            if (delArgs.Length != objArgs.Length) GD.PushError($"Delegate args:({delArgs.Length}) length and parsed string args:({objArgs.Length})don match.");

            // GD.PrintS("delArgs", delArgs.Length, "strArgs", strArgs.Length, "objArgs", objArgs.Length);
            // delArgs.Print();
            // strArgs.Print();
            // objArgs.Print();

            return (command, objArgs);
        }

        return (command, null);
    }

    public void BlockInput() {
        var root = GetTree().Root;
        foreach (var n in root.GetChildren()) {
            if (n != this) {
                n.SetProcessInput(false);
                n.SetProcessUnhandledKeyInput(false);
            }
        }
    }

    public void UnblockInput() {
        var root = GetTree().Root;
        foreach (var n in root.GetChildren()) {
            if (n != this) {
                n.SetProcessInput(true);
                n.SetProcessUnhandledKeyInput(true);
            }
        }
    }
}
