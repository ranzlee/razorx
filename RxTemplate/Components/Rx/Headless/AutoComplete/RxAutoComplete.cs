namespace RxTemplate.Components.Rx.Headless.AutoComplete;

public record RxAutoCompleteModel {
    public bool SortExactMatchesFirst { get; set; }
    public string SearchPattern { get; set; } = null!;
    public IEnumerable<IRxAutoCompleteItem> Items { get; set; } = [];
}

public interface IRxAutoCompleteItem {
    public string Id { get; }
    public string DisplayValue { get; }
}