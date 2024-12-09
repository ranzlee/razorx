namespace RxTemplate.Rx;
public record RxConfig {
    public bool DebugRxClient { get; set; }
    public string LightTheme { get; set; } = "light";
    public string DarkTheme { get; set; } = "dark";
}