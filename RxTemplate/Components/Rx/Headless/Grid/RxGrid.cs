using RxTemplate.Rx;

namespace RxTemplate.Components.Rx.Headless.Grid;

public interface IGrid : IAsyncComponent {
    string SpinnerId { get; set; }
}

public interface IGridModel<T> : IGrid {
    IEnumerable<T> Data { get; set; }
    IGridState State { get; set; }
}

public interface IGridFilterModel : IGrid {
    Type? FilterType { get; set; }
    string FilterProperty { get; set; }
}

public record GridFilter {
    public string FilterId { get; set; } = null!;
    public string FilterProperty { get; set; } = null!;
    public string FilterOperation { get; set; } = null!;
    public string FilterValue { get; set; } = null!;
}

public interface IGridState {
    int Page { get; set; }
    int PageSize { get; set; }
    int TotalRecords { get; set; }
    string SortProperty { get; set; }
    bool SortedDescending { get; set; }
    IList<GridFilter> Filters { get; set; }
}

public static class GridStateExtensions {
    public static T Update<T>(
        this T gridState,
        string? page = null,
        string? sortProperty = null,
        string? filterId = null,
        string? filterProperty = null,
        string? filterOperation = null,
        string? filterValue = null) where T : IGridState {
        if (!string.IsNullOrWhiteSpace(page)) {
            if (int.TryParse(page, out var p)) {
                gridState.Page = p;
                return gridState;
            }
            if (page == "previous") {
                gridState.Page -= 1;
                return gridState;
            }
            if (page == "next") {
                gridState.Page += 1;
                return gridState;
            }
        }
        if (!string.IsNullOrWhiteSpace(sortProperty)) {
            if (gridState.SortProperty == sortProperty) {
                gridState.SortedDescending = !gridState.SortedDescending;
                return gridState;
            }
            gridState.SortProperty = sortProperty;
            gridState.SortedDescending = false;
            return gridState;
        }
        if (!string.IsNullOrWhiteSpace(filterId)) {
            gridState.Filters = [.. gridState.Filters.Where(x => x.FilterId != filterId)];
            return gridState;
        }
        if (!string.IsNullOrWhiteSpace(filterProperty)) {
            gridState.Filters.Add(new GridFilter {
                FilterId = Guid.NewGuid().ToString(),
                FilterProperty = filterProperty,
                FilterOperation = filterOperation ?? "",
                FilterValue = filterValue ?? ""
            });
            return gridState;
        }
        return gridState;
    }

    public static bool HasPreviousPage(this IGridState gridState) {
        return gridState.Page > 1;
    }

    public static bool HasNextPage(this IGridState gridState) {
        return gridState.Page * gridState.PageSize < gridState.TotalRecords;
    }

    public static int GetTotalPages(this IGridState gridState) {
        if (gridState.TotalRecords == 0 || gridState.PageSize == 0) {
            return 1;
        }
        return Convert.ToInt32(Math.Ceiling((decimal)gridState.TotalRecords / gridState.PageSize));
    }
}