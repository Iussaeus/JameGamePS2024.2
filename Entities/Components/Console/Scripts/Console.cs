using Godot;
using Test.Helpers.Extensions;
using System.Collections.Generic;
using Test.Helpers;

public partial class Console : Control {
    private List<string> _history = new();
    private Dictionary<StringName, System.Delegate> _commands = new();
    private CodeEdit _textBox;
    private int _currentIdx = -1;
    private bool _requesting;

    public override void _Ready() {
        _textBox = GetNode<CodeEdit>("CenterContainer/CodeEdit");
        _textBox.Editable = true;
        _textBox.CaretBlink = true;
        this.Assert(_textBox != null, "Console doesn't have a CodeEdit");

        AddCommand(MinePrint);

        Visible = false;
        _textBox.ReleaseFocus();
        _textBox.GetCodeCompletionOptions();
    }

    public void MinePrint(params object[] args) {
        GD.Print("print called");
        foreach (var a in args) {
            GD.Print(a);
        }
    }

    public override void _Input(InputEvent @event) {
        // NOTE: the  `  is printed when exiting console
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

        if (@event.IsActionReleased("enter") && !_requesting) {
            var text = _textBox.Text.StripEscapes();
            _textBox.Clear();
            if (text.Length > 0) {
                AddHistoryItem(text);
                var command = text.Split(" ")[0];
                GD.PrintS(command, _commands.ContainsKey(command));
                if (_commands.ContainsKey(command)) CallCommand(command, text.Split(" ")[1..]);

            }
        }
        else {
            _requesting = false;
            _textBox.ConfirmCodeCompletion();
        }

        // TODO: Make the CodeCompletionOptions persist
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

    public void AddCompletionItem(string text) {
        _history.Add(text);
        _textBox.CodeCompletionPrefixes.Add(text);
        _textBox.AddCodeCompletionOption(CodeEdit.CodeCompletionKind.Function,
                                displayText: text + " [command]",
                                insertText: text,
                                textColor: Colors.Red);
        _currentIdx = _history.Count - 1;
        // GD.PrintS("accept: ", _currentIdx);
    }

    // TODO: match the function to a dict or something 
    // TODO: grab args from the in-game console , parse them correctly and pass them to their respective functions
    public void AddCommand(System.Delegate @delegate) {
        var name = @delegate.Method.Name;

        GD.PrintS("func to add", @delegate, @delegate.Target, @delegate.Method.Name);

        AddCompletionItem(name);
        _commands.Add(name, @delegate);
    }

    public void CallCommand(StringName name, params System.Object[] args) {
        // GD.Print("call command");
        var @delegate = _commands[name];
        GD.PrintS("func to call", @delegate, @delegate.Target, @delegate.Method.Name);
        // GD.PrintS(@delegate, @delegate.Method.Name, @delegate.Target.GetType().GetMethod(name), args.Length);

        if (args.Length == 1 && args[0] is string s && s.Equals("")) {
            GD.Print("No args:");
            return;
        }

        var (ok, result) = Helpers.Pcall(@delegate, args);

        GD.PrintS(ok, result);
        // GD.Print("end call command");
    }
}
