namespace RxTemplate.Components.Rx.Headless.AutoComplete;

public record RxAutoCompleteModel {
    public HttpMethod? OnSelectedHttpMethod { get; set; }
    public string OnSelectedRoute { get; set; } = null!;
    public string OnSelectedResponseTarget { get; set; } = null!;
    public string OnSelectedResponseSwap { get; set; } = null!;
    public string SearchPattern { get; set; } = null!;
    public IEnumerable<IRxAutoCompleteItem> Items { get; set; } = [];
}

public interface IRxAutoCompleteItem {
    public string Id { get; }
    public string DisplayName { get; }
}