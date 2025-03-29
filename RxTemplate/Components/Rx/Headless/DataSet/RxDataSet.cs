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
    string FilterProperty { get; set; }
}

public record DataSetFilter {
    public string FilterId { get; set; } = null!;
    public string FilterProperty { get; set; } = null!;
    public string FilterOperation { get; set; } = null!;
    public string FilterValue { get; set; } = null!;
}

public interface IDataSetState {
    int Page { get; set; }
    int PageSize { get; set; }
    int TotalRecords { get; set; }
    string SortProperty { get; set; }
    bool SortedDescending { get; set; }
    IList<DataSetFilter> Filters { get; set; }
}

public static class DataSetStateExtensions {
    public static T Update<T>(
        this T dataSetState,
        string? page = null,
        string? sortProperty = null,
        string? filterId = null,
        string? filterProperty = null,
        string? filterOperation = null,
        string? filterValue = null) where T : IDataSetState {
        if (!string.IsNullOrWhiteSpace(page)) {
            if (int.TryParse(page, out var p)) {
                dataSetState.Page = p;
                return dataSetState;
            }
            if (page == "previous") {
                dataSetState.Page -= 1;
                return dataSetState;
            }
            if (page == "next") {
                dataSetState.Page += 1;
                return dataSetState;
            }
        }
        if (!string.IsNullOrWhiteSpace(sortProperty)) {
            if (dataSetState.SortProperty == sortProperty) {
                dataSetState.SortedDescending = !dataSetState.SortedDescending;
                return dataSetState;
            }
            dataSetState.SortProperty = sortProperty;
            dataSetState.SortedDescending = false;
            return dataSetState;
        }
        if (!string.IsNullOrWhiteSpace(filterId)) {
            dataSetState.Filters = [.. dataSetState.Filters.Where(x => x.FilterId != filterId)];
            return dataSetState;
        }
        if (!string.IsNullOrWhiteSpace(filterProperty)) {
            dataSetState.Filters.Add(new DataSetFilter {
                FilterId = Guid.NewGuid().ToString(),
                FilterProperty = filterProperty,
                FilterOperation = filterOperation ?? "",
                FilterValue = filterValue ?? ""
            });
            return dataSetState;
        }
        return dataSetState;
    }

    public static bool HasPreviousPage(this IDataSetState dataSetState) {
        return dataSetState.Page > 1;
    }

    public static bool HasNextPage(this IDataSetState dataSetState) {
        return dataSetState.Page * dataSetState.PageSize < dataSetState.TotalRecords;
    }

    public static int GetTotalPages(this IDataSetState dataSetState) {
        if (dataSetState.TotalRecords == 0 || dataSetState.PageSize == 0) {
            return 1;
        }
        return Convert.ToInt32(Math.Ceiling((decimal)dataSetState.TotalRecords / dataSetState.PageSize));
    }
}