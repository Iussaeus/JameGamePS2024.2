using Godot;
namespace Test.Helpers;

public static class Helpers {
    public static (bool Ok, object Result) Pcall(System.Delegate @delegate, params System.Object[] args) {
        var method = @delegate.Target.GetType().GetMethod(@delegate.Method.Name);
        // GD.Print("pcall");
        object result = null;

        GD.Print(args.GetType());

        try {
            result = method.Invoke(@delegate.Target, new object[] { args });
        }
        catch (System.Exception e) {
            // GD.Print("end pcall");
            return (false, e);
        };
        // GD.Print("end pcall");
        return (true, result);
    }
}

