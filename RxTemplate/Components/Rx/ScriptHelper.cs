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
    private readonly string cacheBuster;
    
    public ScriptHelper() {
        cacheBuster = $"v={GetType().Assembly.GetName().Version!.Revision}";
    }

    public string GetCacheBuster() {
        return cacheBuster;
    }
}