namespace RxTemplate.Components.Rx;

public interface IScriptHelper {
    string GetCacheBuster();
}

public static class ScriptHelperConfig {
    public static void AddScriptHelper(this IServiceCollection services) {
        services.AddSingleton<IScriptHelper, ScriptHelper>();
    }
}

file sealed class ScriptHelper : IScriptHelper {
    private static string cacheBuster = string.Empty;
    private static readonly object locker = new();

    public string GetCacheBuster() {
        if (!string.IsNullOrWhiteSpace(cacheBuster)) {
            return cacheBuster;
        }
        lock (locker) {
            if (!string.IsNullOrWhiteSpace(cacheBuster)) {
                return cacheBuster;
            }
            cacheBuster = $"v={GetType().Assembly.GetName().Version!.Revision}";
        }
        return cacheBuster;
    }
}