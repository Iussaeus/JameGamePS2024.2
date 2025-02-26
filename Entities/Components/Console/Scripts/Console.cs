using Godot;
using Test.Helpers.Extensions;
using System.Collections.Generic;
using Test.Helpers;

public partial class Console : Control {
    private List<string> _history = new();
    private Dictionary<StringName, System.Action> _commands = new();
    private CodeEdit _textBox;
    private int _currentIdx = -1;
    private bool _requesting;

    public override void _Ready() {
        _textBox = GetNode<CodeEdit>("CenterContainer/CodeEdit");
        _textBox.Editable = true;
        _textBox.CaretBlink = true;
        this.Assert(_textBox != null, "Console doesn't have a CodeEdit");

        AddFunction(AddHistoryItem, "Some Bullshit");

        Visible = false;
        _textBox.ReleaseFocus();
        _textBox.GetCodeCompletionOptions();
    }

    public override void _Input(InputEvent @event) {
        if (Input.IsActionJustReleased("console")) {
            Visible = Visible ? false : true;
            if (Visible) {
                _textBox.GrabFocus();
            }
            else {
                _textBox.ReleaseFocus();
            }
            _textBox.Clear();
        }

        if (@event.IsActionReleased("ui_accept") && !_requesting) {
            var noSelection = _textBox.Text;
            _textBox.Clear();
            if (noSelection.Length > 0) {
                var text = noSelection.StripEscapes();
                AddHistoryItem(text);
            }
        }
        else {
            _requesting = false;
            _textBox.ConfirmCodeCompletion();
        }

        if (@event.IsActionReleased("tab") && !_requesting) {
            _requesting = true;
            _textBox.UpdateCodeCompletionOptions(true);
            _textBox.RequestCodeCompletion();
        }
        else {
            _requesting = false;
            _textBox.ConfirmCodeCompletion();
        }
    }

    public override void _Process(double delta) {
        if (Visible && !_textBox.HasFocus()) _textBox.GrabFocus();

        if (_currentIdx >= 0) {
            if (Input.IsActionJustPressed("ui_up")) {
                _currentIdx = _currentIdx - 1 >= 0 ? _currentIdx - 1 : _currentIdx;
                this.Assert(_currentIdx < _history.Count, $"Current index too small:{_currentIdx}, should be >= than 0");
                _textBox.Clear();
                // GD.PrintS("up");
                // GD.PrintS("idx: ", _currentIdx);
                // GD.Print("elem: ", _history[_currentIdx]);
                _textBox.InsertText(_history[_currentIdx], 0, 0, true);
            }
            if (Input.IsActionJustPressed("ui_down")) {
                // _textBox.Clear();
                _currentIdx = _currentIdx + 1 < _history.Count ? _currentIdx + 1 : _currentIdx;
                this.Assert(_currentIdx < _history.Count, $"Current index too big:{_currentIdx}, should be smaller than {_history.Count}");
                _textBox.Clear();
                // GD.PrintS("down");
                // GD.PrintS("idx: ", _currentIdx);
                // GD.Print("elem: ", _history[_currentIdx]);
                _textBox.InsertText(_history[_currentIdx], 0, 0);
            }
            if (Input.IsActionJustPressed("ui_down") && _currentIdx + 1 == _history.Count) {
                _textBox.Text = "";
            }
        }
    }

    public void AddHistoryItem(string text) {
        _history.Add(text);
        _textBox.CodeCompletionPrefixes.Add(text);
        _textBox.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function,
                                displayText: text + " [history]",
                                insertText: text,
                                textColor: Colors.Blue);
        _currentIdx = _history.Count - 1;
        GD.PrintS("accept: ", _currentIdx);
    }

    public void AddFunction(System.Delegate function, params object[] args) {
        var name = function.Method.Name;
        var (ok, err) = Helpers.Pcall(function, args);

        if (ok) {
            GD.PrintS(name, args);
        }
        else GD.Print(err);
    }
}
