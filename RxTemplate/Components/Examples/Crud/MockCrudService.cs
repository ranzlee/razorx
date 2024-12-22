using RxTemplate.Components.Rx;
using RxTemplate.Components.Rx.Headless.Grid;

namespace RxTemplate.Components.Examples.Crud;

/// <summary>
/// This is a fake service/repository. In a real application, this service would encapsulate 
/// an EF context and be injected into the handler.
/// </summary>
public class MockCrudService {
    private static IEnumerable<ItemModel> Data;

    static MockCrudService() {
        var summaries = new[]
        {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching"
        };

        var data = new List<ItemModel>();
        // Generate some random data
        for (var i = 0; i < 1024; i++) {
            var d = new ItemModel {
                Id = i + 1,
                Date = DateTime.UtcNow.AddDays(i),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = summaries[Random.Shared.Next(summaries.Length)],
                IsVerified = Random.Shared.Next(1, 100) % 2 == 0,
                TemperatureTaken = Random.Shared.Next(1, 100) switch {
                    < 21 => TimeOfDay.Morning,
                    < 41 => TimeOfDay.Noon,
                    < 61 => TimeOfDay.Evening,
                    < 81 => TimeOfDay.Night,
                    _ => TimeOfDay.NotReported
                }

            };
            data.Add(d);
        }
        Data = data;
    }

    /// <summary>
    /// A fake EF select by ID.
    /// </summary>
    /// <param name="id">The ID of the item to select.</param>
    /// <returns>The item</returns>
    public ItemModel? Get(int id) {
        return Data.SingleOrDefault(x => x.Id == id);
    }

    /// <summary>
    /// A fake EF delete.
    /// </summary>
    /// <param name="id">The ID of the item to delete.</param>
    public void Delete(int id) {
        Data = [.. Data.Where(x => x.Id != id)];
    }

    public void Save(ItemModel model) {
        // Insert
        if (model.Id == 0) {
            model.Id = Data.Max(x => x.Id) + 1;
            model.Date = model.GetDateAsUtc();
            Data = [.. Data, model];
            return;
        }
        // Update
        var item = Data.SingleOrDefault(x => x.Id == model.Id);
        if (item is not null) {
            item.Date = model.GetDateAsUtc();
            item.IsVerified = model.IsVerified;
            item.Summary = model.Summary;
            item.TemperatureC = model.TemperatureC;
            item.TemperatureTaken = model.TemperatureTaken;
        }
    }

    /// <summary>
    /// This will usually be an EF IQueryable operation.
    /// The general process is:
    /// 1. Create IQueryable object
    /// 2. Apply filter predicates
    /// 3. Execute a count and set state.TotalRecords
    /// 4. Apply sort predicate
    /// 5. Execute a WithNoTracking data query with skip/take predicates and set state.Data
    /// IMPORTANT: Execute exactly 2 queries per request; 1 count and 1 data.
    /// </summary>
    /// <param name="state">The GridState object</param>
    /// <returns>IGridModel</returns>
    public IGridModel<ItemModel> GetModel(GridState state) {
        // Create IQueryable to data source
        var data = Data;
        // Apply filters
        data = ApplyDateFilters(data, state);
        data = ApplyIntFilters(data, state);
        data = ApplyStringFilters(data, state);
        data = ApplyBooleanFilters(data, state);
        data = ApplyEnumFilters(data, state);
        // Select count
        state.TotalRecords = data.Count();
        // Sort data
        if (!string.IsNullOrWhiteSpace(state.SortProperty)) {
            data = state.SortedDescending
                ? state.SortProperty switch {
                    nameof(ItemModel.Id) => [.. data.OrderByDescending(x => x.Id)],
                    nameof(ItemModel.Date) => [.. data.OrderByDescending(x => x.Date)],
                    nameof(ItemModel.Summary) => [.. data.OrderByDescending(x => x.Summary)],
                    nameof(ItemModel.TemperatureC) => [.. data.OrderByDescending(x => x.TemperatureC)],
                    nameof(ItemModel.TemperatureF) => [.. data.OrderByDescending(x => x.TemperatureF)],
                    nameof(ItemModel.IsVerified) => [.. data.OrderByDescending(x => x.IsVerified)],
                    nameof(ItemModel.TemperatureTaken) => [.. data.OrderByDescending(x => x.TemperatureTaken.ToString())],
                    _ => data
                }
                : state.SortProperty switch {
                    nameof(ItemModel.Id) => [.. data.OrderBy(x => x.Id)],
                    nameof(ItemModel.Date) => [.. data.OrderBy(x => x.Date)],
                    nameof(ItemModel.Summary) => [.. data.OrderBy(x => x.Summary)],
                    nameof(ItemModel.TemperatureC) => [.. data.OrderBy(x => x.TemperatureC)],
                    nameof(ItemModel.TemperatureF) => [.. data.OrderBy(x => x.TemperatureF)],
                    nameof(ItemModel.IsVerified) => [.. data.OrderBy(x => x.IsVerified)],
                    nameof(ItemModel.TemperatureTaken) => [.. data.OrderBy(x => x.TemperatureTaken.ToString())],
                    _ => data
                };
        }
        // Select page
        data = [.. data.Skip((state.Page - 1) * state.PageSize).Take(state.PageSize)];
        // Return the model
        return new GridModel {
            Data = data,
            State = state
        };
    }

    private static IEnumerable<ItemModel> ApplyDateFilters(IEnumerable<ItemModel> data, GridState state) {
        if (!data.Any()) {
            return [];
        }
        foreach (var f in state.Filters) {
            if (f.FilterProperty != nameof(ItemModel.Date)) {
                continue;
            }
            if (string.IsNullOrWhiteSpace(f.FilterValue)) {
                return [];
            }
            var compareDate = Utilities.Converter<DateTime>(f.FilterValue);
            if (f.FilterOperation == FilterOperationType.Equals.ToString()) {
                data = data.Where(x => x.Date!.Value.Date == compareDate.Date);
                continue;
            }
            if (f.FilterOperation == FilterOperationType.LessThan.ToString()) {
                data = data.Where(x => x.Date!.Value.Date < compareDate.Date);
                continue;
            }
            if (f.FilterOperation == FilterOperationType.GreaterThan.ToString()) {
                data = data.Where(x => x.Date!.Value.Date > compareDate.Date);
                continue;
            }
        }
        return data;
    }

    private static IEnumerable<ItemModel> ApplyIntFilters(IEnumerable<ItemModel> data, GridState state) {
        if (!data.Any()) {
            return [];
        }
        foreach (var f in state.Filters) {
            if (f.FilterProperty != nameof(ItemModel.TemperatureC)
            && f.FilterProperty != nameof(ItemModel.TemperatureF)
            && f.FilterProperty != nameof(ItemModel.Id)) {
                continue;
            }
            if (string.IsNullOrWhiteSpace(f.FilterValue)) {
                return [];
            }
            var compareInt = Utilities.Converter<int>(f.FilterValue);
            if (f.FilterProperty == nameof(ItemModel.TemperatureC)) {
                if (f.FilterOperation == FilterOperationType.Equals.ToString()) {
                    data = data.Where(x => x.TemperatureC == compareInt);
                    continue;
                }
                if (f.FilterOperation == FilterOperationType.LessThan.ToString()) {
                    data = data.Where(x => x.TemperatureC < compareInt);
                    continue;
                }
                if (f.FilterOperation == FilterOperationType.GreaterThan.ToString()) {
                    data = data.Where(x => x.TemperatureC > compareInt);
                    continue;
                }
            }
            if (f.FilterProperty == nameof(ItemModel.TemperatureF)) {
                if (f.FilterOperation == FilterOperationType.Equals.ToString()) {
                    data = data.Where(x => x.TemperatureF == compareInt);
                    continue;
                }
                if (f.FilterOperation == FilterOperationType.LessThan.ToString()) {
                    data = data.Where(x => x.TemperatureF < compareInt);
                    continue;
                }
                if (f.FilterOperation == FilterOperationType.GreaterThan.ToString()) {
                    data = data.Where(x => x.TemperatureF > compareInt);
                    continue;
                }
            }
            if (f.FilterProperty == nameof(ItemModel.Id)) {
                if (f.FilterOperation == FilterOperationType.Equals.ToString()) {
                    data = data.Where(x => x.Id == compareInt);
                    continue;
                }
                if (f.FilterOperation == FilterOperationType.LessThan.ToString()) {
                    data = data.Where(x => x.Id < compareInt);
                    continue;
                }
                if (f.FilterOperation == FilterOperationType.GreaterThan.ToString()) {
                    data = data.Where(x => x.Id > compareInt);
                    continue;
                }
            }
        }
        return data;
    }

    private static IEnumerable<ItemModel> ApplyStringFilters(IEnumerable<ItemModel> data, GridState state) {
        if (!data.Any()) {
            return [];
        }
        foreach (var f in state.Filters) {
            if (f.FilterProperty != nameof(ItemModel.Summary)) {
                continue;
            }
            if (f.FilterOperation == FilterOperationType.Equals.ToString() && string.IsNullOrWhiteSpace(f.FilterValue)) {
                data = data.Where(x => string.IsNullOrWhiteSpace(x.Summary));
                continue;
            }
            if (f.FilterOperation == FilterOperationType.Equals.ToString()) {
                data = data.Where(x => x.Summary is not null && x.Summary.Equals(f.FilterValue, StringComparison.InvariantCultureIgnoreCase));
                continue;
            }
            if (f.FilterOperation == FilterOperationType.StartsWith.ToString()) {
                data = data.Where(x => x.Summary is not null && x.Summary.StartsWith(f.FilterValue, StringComparison.InvariantCultureIgnoreCase));
                continue;
            }
            if (f.FilterOperation == FilterOperationType.Contains.ToString()) {
                data = data.Where(x => x.Summary is not null && x.Summary.Contains(f.FilterValue, StringComparison.InvariantCultureIgnoreCase));
                continue;
            }
        }
        return data;
    }

    private static IEnumerable<ItemModel> ApplyBooleanFilters(IEnumerable<ItemModel> data, GridState state) {
        if (!data.Any()) {
            return [];
        }
        foreach (var f in state.Filters) {
            if (f.FilterProperty != nameof(ItemModel.IsVerified)) {
                continue;
            }
            var compareIsVerified = f.FilterValue.Equals("Y", StringComparison.InvariantCultureIgnoreCase);
            if (f.FilterOperation == FilterOperationType.Equals.ToString()) {
                data = data = data.Where(x => x.IsVerified == compareIsVerified);
                continue;
            }
            if (f.FilterOperation == FilterOperationType.NotEquals.ToString()) {
                data = data = data.Where(x => x.IsVerified != compareIsVerified);
                continue;
            }
        }
        return data;
    }

    private static IEnumerable<ItemModel> ApplyEnumFilters(IEnumerable<ItemModel> data, GridState state) {
        if (!data.Any()) {
            return [];
        }
        foreach (var f in state.Filters) {
            if (f.FilterProperty != nameof(ItemModel.TemperatureTaken)) {
                continue;
            }
            var compareTemperatureTaken = Utilities.Converter<TimeOfDay>(f.FilterValue);
            if (f.FilterOperation == FilterOperationType.Equals.ToString()) {
                data = data = data.Where(x => x.TemperatureTaken == compareTemperatureTaken);
                continue;
            }
            if (f.FilterOperation == FilterOperationType.NotEquals.ToString()) {
                data = data = data.Where(x => x.TemperatureTaken != compareTemperatureTaken);
                continue;
            }
        }
        return data;
    }
}