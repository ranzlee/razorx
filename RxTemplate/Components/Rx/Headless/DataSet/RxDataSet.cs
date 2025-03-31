using RxTemplate.Rx;

namespace RxTemplate.Components.Rx.Headless.DataSet;

public interface IDataSet : IAsyncComponent {
    string SpinnerId { get; set; }
}

public interface IDataSetModel<T> : IDataSet {
    IEnumerable<T> Data { get; set; }
    IDataSetState State { get; set; }
}

public interface IDataSetFilterModel : IDataSet {
    Type? FilterType { get; set; }
    string FilterPropertyName { get; set; }
}

public record DataSetFilter(string FilterId, string FilterPropertyName, string FilterOperation, string FilterValue);

public interface IDataSetState {
    int Page { get; set; }
    int PageSize { get; set; }
    int TotalRecords { get; set; }
    string SortPropertyName { get; set; }
    bool SortedDescending { get; set; }
    IList<DataSetFilter> Filters { get; set; }
}

public static class DataSetStateExtensions {
    public static void Update<T>(
        this T dataSetState,
        string? page = null,
        string? sortPropertyName = null,
        string? filterId = null,
        string? filterPropertyName = null,
        string? filterOperation = null,
        string? filterValue = null) where T : IDataSetState {
        if (!string.IsNullOrWhiteSpace(page)) {
            if (int.TryParse(page, out var p)) {
                dataSetState.Page = p;
                dataSetState.AutoCorrect();
                return;
            }
            if (page == "previous") {
                dataSetState.Page -= 1;
                dataSetState.AutoCorrect();
                return;
            }
            if (page == "next") {
                dataSetState.Page += 1;
                dataSetState.AutoCorrect();
                return;
            }
        }
        if (!string.IsNullOrWhiteSpace(sortPropertyName)) {
            if (dataSetState.SortPropertyName == sortPropertyName) {
                dataSetState.SortedDescending = !dataSetState.SortedDescending;
                dataSetState.AutoCorrect();
                return;
            }
            dataSetState.SortPropertyName = sortPropertyName;
            dataSetState.SortedDescending = false;
            dataSetState.AutoCorrect();
            return;
        }
        if (!string.IsNullOrWhiteSpace(filterId)) {
            dataSetState.Filters = [.. dataSetState.Filters.Where(x => x.FilterId != filterId)];
            dataSetState.AutoCorrect();
            return;
        }
        if (!string.IsNullOrWhiteSpace(filterPropertyName)) {
            dataSetState.Filters.Add(new DataSetFilter(Guid.NewGuid().ToString(), filterPropertyName, filterOperation ?? "", filterValue ?? ""));
            dataSetState.AutoCorrect();
            return;
        }
    }

    private static void AutoCorrect(this IDataSetState dataSetState) {
        if (dataSetState.Page < 1) {
            dataSetState.Page = 1;
        }
        if (dataSetState.PageSize < 0) {
            dataSetState.PageSize = 0;
        }
        if (dataSetState.TotalRecords < 0) {
            dataSetState.TotalRecords = 0;
        }
        dataSetState.SortPropertyName ??= string.Empty;
        dataSetState.Filters ??= [];
    }

    public static bool HasPreviousPage(this IDataSetState dataSetState) {
        return dataSetState.Page > 1;
    }

    public static bool HasNextPage(this IDataSetState dataSetState) {
        if (dataSetState.PageSize == 0) {
            return false;
        }
        return dataSetState.Page * dataSetState.PageSize < dataSetState.TotalRecords;
    }

    public static int GetTotalPages(this IDataSetState dataSetState) {
        if (dataSetState.TotalRecords == 0 || dataSetState.PageSize == 0) {
            return 1;
        }
        return Convert.ToInt32(Math.Ceiling((decimal)dataSetState.TotalRecords / dataSetState.PageSize));
    }
}