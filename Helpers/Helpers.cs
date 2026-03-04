using Godot;
using System.Diagnostics;
using System.Threading;

namespace Test.Utils;

public static class Assert
{
    [Conditional("DEBUG")]
    public static void Condition(bool truthy, string message)
    {
        if (!truthy)
        {
            var pid = OS.GetProcessId();
            var stackTrace = new StackTrace(true);
            var frame = stackTrace.GetFrame(1);
            var file = System.IO.Path.GetFileName(frame.GetFileName());
            var line = frame.GetFileLineNumber();
            var @class = frame.GetMethod().DeclaringType.Name;
            var method = frame.GetMethod().Name;
            GD.PrintRich($"[color=red]{file}:{line} {@class}.{method} - Assertion Failed: {message}[/color]");
            (Engine.GetMainLoop() as SceneTree).Quit(1);
        }
    }
}

public static class Helpers
{
    public static (bool Ok, object Result) PCall(System.Delegate @delegate, params System.Object[] args)
    {
        var method = @delegate.Method;
        var methodParams = method.GetParameters(); ;
        object result = null;

        for (int i = 0; i < methodParams.Length; i++)
        {
            if (args[i].GetType() != methodParams[i].ParameterType)
            {
                args = ParseArgs(@delegate, args);
                break;
            }
        }

        try
        {
            result = method.Invoke(@delegate.Target, args);
        }
        catch (System.Exception e)
        {
            return (false, e);
        }

        return (true, result);
    }

    private static object[] ParseArgs(System.Delegate @delegate, object[] args)
    {
        var delArgs = @delegate.Method.GetParameters();
        var objArgs = new object[delArgs.Length];

        for (int i = 0; i < delArgs.Length; i++)
        {
            var delType = delArgs[i].ParameterType;
            var argsType = args[i].GetType();
            if (delType == argsType)
                objArgs[i] = args[i];

            if (delType.BaseType == typeof(System.Array))
            {
                var varArgs = new object[args.Length - i];
                for (int j = 0; j < varArgs.Length; j++)
                {
                    varArgs[j] = args[j + i];
                }
                objArgs[i] = varArgs;
                break;
            }
        }
        return objArgs;
    }
}
