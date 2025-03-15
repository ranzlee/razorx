namespace RxTemplate.Components.Rx.Headless.AutoComplete;

public record RxAutoCompleteModel {
    public HttpMethod OnSelectedHttpMethod { get; set; } = HttpMethod.Get;
    public string OnSelectedEndpoint { get; set; } = null!;
    public string OnSelectedResponseTarget { get; set; } = null!;
    public string OnSelectedResponseSwap { get; set; } = null!;
    public int ListMaxPixelHeight { get; set; } = 250;
    public IEnumerable<IRxAutoCompleteItem> Items { get; set; } = [];
}

public interface IRxAutoCompleteItem {
    public string Id { get; }
    public string DisplayName { get; }
}