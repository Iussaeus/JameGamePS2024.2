namespace Test.Helpers;

public static class Helpers {
    public static (bool Ok, object Result) Pcall(System.Delegate function, params object[] args) {
        object result = null;
        try {
            result = function.Method.Invoke(function.Target, args);
        }
        catch (System.Exception e) {
            result = e;
            return (false, e);
        };
        return (true, result);
    }
}

