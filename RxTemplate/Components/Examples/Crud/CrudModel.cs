using System.ComponentModel;
using RxTemplate.Components.Rx;
using RxTemplate.Components.Rx.Headless.DataSet;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Crud;

/// <summary>
/// Constants for the IGridModel and IGridFilterModel. These
/// define the RxGrid ID, state key, and state scope.
/// </summary>
public static class GridProperties {
    public const string Id = "demo-grid";
    public const string StateKey = "DemoGridState";
    public const HxMetadataScope StateScope = HxMetadataScope.Session;
    public const string RenderFromRoute = "/examples/crud/grid";
    public const string SpinnerId = "grid-action-spinner";
}

/// <summary>
/// The IGridFilterModel for the example grid.
/// </summary>
public record GridFilterModel : IDataSetFilterModel {
    public string Id { get; set; } = GridProperties.Id;
    public string StateKey { get; set; } = GridProperties.StateKey;
    public HxMetadataScope StateScope { get; set; } = GridProperties.StateScope;
    public string RenderFromRoute { get; set; } = GridProperties.RenderFromRoute;
    public string SpinnerId { get; set; } = GridProperties.SpinnerId;
    public Type? FilterType { get; set; }
    public string FilterProperty { get; set; } = null!;
    public bool IsAsync { get; set; }
    public string InitialState { get; set; } = string.Empty;
}

/// <summary>
/// Defines the types of filter operations the example grid supports.
/// </summary>
public enum FilterOperationType {
    Equals = 0,
    NotEquals = 1,
    GreaterThan = 2,
    LessThan = 3,
    StartsWith = 4,
    Contains = 5,
}

public record GridState : IDataSetState {
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalRecords { get; set; } = 0;
    public string SortProperty { get; set; } = null!;
    public bool SortedDescending { get; set; } = false;
    public IList<DataSetFilter> Filters { get; set; } = [];
}

/// <summary>
/// The IGridModel that binds to the RxGrid.
/// </summary>
public record GridModel : IDataSetModel<ItemModel> {
    public string Id { get; set; } = GridProperties.Id;
    public string StateKey { get; set; } = GridProperties.StateKey;
    public HxMetadataScope StateScope { get; set; } = GridProperties.StateScope;
    public string RenderFromRoute { get; set; } = GridProperties.RenderFromRoute;
    public string SpinnerId { get; set; } = GridProperties.SpinnerId;
    public IEnumerable<ItemModel> Data { get; set; } = [];
    public IDataSetState State { get; set; } = new GridState();
    public bool IsAsync { get; set; }
    public string InitialState { get; set; } = string.Empty;
}

/// <summary>
/// The model that defines a single row in the grid.
/// 
/// Model Rules
/// 1. Create models that are specific to views/components. Do not reuse entities.
/// 2. If it is possible for the user to send in a blank value (RxField and RxMemoField), the property must be nullable even if null is not valid (validate it).
/// 3. Booleans must not be nullable since they bind to a checkbox/toggle.
/// 4. Enums must not be nullable since they are not deserializable by System.Text.Json. Add a null-representative value to the enum if you need it. 
/// </summary>
public record ItemModel {
    public ItemModel() {
        TemperatureTaken = TimeOfDay.Select;
    }

    public int Id { get; set; }
    public DateTime? Date { get; set; }
    public string? DateTimeZone { get; set; }
    public int? TemperatureC { get; set; }
    public string? Summary { get; set; }
    public int? TemperatureF => TemperatureC.HasValue ? 32 + (int)(TemperatureC / 0.5556) : null;
    public bool IsVerified { get; set; }
    public TimeOfDay TemperatureTaken { get; set; }

    public DateTime? GetDateAsUtc(ILogger? logger = default) {
        return Utilities.GetUtcDateFromTimeZone(Date, DateTimeZone, logger);
    }

    public static string DecodePropertyName(string propertyName) {
        return propertyName switch {
            nameof(Date) => "Date",
            nameof(TemperatureC) => "Temp. C",
            nameof(TemperatureF) => "Temp. F",
            nameof(Summary) => "Summary",
            nameof(IsVerified) => "Verified",
            nameof(TemperatureTaken) => "Observed",
            _ => ""
        };
    }
}

/// <summary>
/// The Enum for the TemperatureTaken property.
/// </summary>
public enum TimeOfDay {
    [Description("Select...")]
    Select,
    Evening,
    Morning,
    Night,
    Noon,
    NotReported,
}