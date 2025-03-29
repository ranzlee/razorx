namespace RxTemplate.Components.Rx.Headless.AutoComplete;

public record RxAutoCompleteModel(bool SortExactMatchesFirst, string SearchPattern, IEnumerable<IRxAutoCompleteItem> Items);

public interface IRxAutoCompleteItem {
    public string Id { get; }
    public string DisplayValue { get; }
}