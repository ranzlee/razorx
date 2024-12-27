using RxTemplate.Rx;

namespace RxTemplate.Components.Rx.Headless.Grid;

public interface IGrid : ISuspendedComponent {
    string SpinnerId { get; set; }
}

public interface IGridModel<T> : IGrid {
    IEnumerable<T> Data { get; set; }
    GridState State { get; set; }
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

public record GridState {
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalRecords { get; set; } = 0;
    public string SortProperty { get; set; } = null!;
    public bool SortedDescending { get; set; } = false;
    public List<GridFilter> Filters { get; set; } = [];

    public void Update(
        string? page,
        string? sortProperty,
        string? filterId = null,
        string? filterProperty = null,
        string? filterOperation = null,
        string? filterValue = null) {
        if (page == "previous") {
            Page -= 1;
            return;
        }
        if (page == "next") {
            Page += 1;
            return;
        }
        if (!string.IsNullOrWhiteSpace(sortProperty)) {
            if (SortProperty == sortProperty) {
                SortedDescending = !SortedDescending;
                return;
            }
            SortProperty = sortProperty;
            SortedDescending = false;
            return;
        }
        if (!string.IsNullOrWhiteSpace(filterId)) {
            Filters = [.. Filters.Where(x => x.FilterId != filterId)];
            return;
        }
        if (!string.IsNullOrWhiteSpace(filterProperty)) {
            Filters.Add(new GridFilter {
                FilterId = Guid.NewGuid().ToString(),
                FilterProperty = filterProperty,
                FilterOperation = filterOperation ?? "",
                FilterValue = filterValue ?? ""
            });
            return;
        }
    }

    public bool HasPreviousPage() {
        return Page > 1;
    }

    public bool HasNextPage() {
        return Page * PageSize < TotalRecords;
    }

    public int GetTotalPages() {
        if (TotalRecords == 0 || PageSize == 0) {
            return 1;
        }
        return Convert.ToInt32(Math.Ceiling((decimal)TotalRecords / PageSize));
    }
}

