using System.Text.Json;
using System.Web;
using RxTemplate.Components.Rx.Headless.DataSet;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Crud;

public class CrudHandler : IRequestHandler {

    /// <summary>
    /// A mock service/repository that would be replaced with a real implementation and injected in.
    /// </summary>
    private readonly static MockCrudService Service = new();

    public void MapRoutes(IEndpointRouteBuilder router) {

        router.MapGet("/examples/crud", GetNonBlocking)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapGet("/examples/crud/filter", GetFilter)
            .AllowAnonymous();

        router.MapGet("/examples/crud/delete-modal/{id:int}", GetDeleteModal)
            .AllowAnonymous();

        router.MapGet("/examples/crud/save-modal/{id:int}", GetSaveModal)
            .AllowAnonymous();

        router.MapGet("/examples/crud/grid", GetGrid)
            .AllowAnonymous();

        router.MapDelete("/examples/crud/{id:int}", DeleteItem)
            .RequireAuthorization();

        router.MapPut("/examples/crud/{id:int}", SaveItem)
            .WithRxValidation<ItemValidator>()
            .RequireAuthorization();

        router.MapPost("/examples/crud", SaveItem)
            .WithRxValidation<ItemValidator>()
            .RequireAuthorization();
    }

    public async static Task<IResult> GetBlocking(
        HttpResponse response,
        string? demoGridState,
        ILogger<CrudHandler> logger) {
        GridState? state = string.IsNullOrWhiteSpace(demoGridState)
            ? new()
            : JsonSerializer.Deserialize<GridState>(HttpUtility.UrlDecode(demoGridState))!;
        // Simulate a DB latency
        await Task.Delay(200);
        // Get the model
        var model = Service.GetModel(state);
        if (!model.Data.Any() && state.Page > 1) {
            state.Page = 1;
            model = Service.GetModel(state);
        }
        // Render the page
        return response.RenderComponent<CrudPage, GridModel>(model, logger);
    }

    public static IResult GetNonBlocking(
        HttpResponse response,
        string? demoGridState,
        ILogger<CrudHandler> logger) {
        GridModel model = new() {
            IsAsync = true,
            InitialState = demoGridState ?? string.Empty
        };
        // Render the page
        return response.RenderComponent<CrudPage, GridModel>(model, logger);
    }

    public static IResult GetFilter(
        HttpResponse response,
        string filterProperty,
        ILogger<CrudHandler> logger) {
        // Render the filter form for the requested item property
        var model = new GridFilterModel {
            FilterProperty = filterProperty,
            FilterType = filterProperty switch {
                nameof(ItemModel.Id) => typeof(int),
                nameof(ItemModel.Date) => typeof(DateOnly),
                nameof(ItemModel.IsVerified) => typeof(bool),
                nameof(ItemModel.Summary) => typeof(string),
                nameof(ItemModel.TemperatureC) => typeof(int),
                nameof(ItemModel.TemperatureF) => typeof(int),
                nameof(ItemModel.TemperatureTaken) => typeof(TimeOfDay),
                _ => null
            }
        };
        return response.RenderComponent<GridFilterDefinition, GridFilterModel>(model, logger);
    }

    public async static Task<IResult> GetDeleteModal(
        HttpResponse response,
        int id,
        IHxTriggers hxTriggers,
        ILogger<CrudHandler> logger) {
        // Simulate a DB latency
        await Task.Delay(200);
        // Get the requested item
        var model = Service.Get(id);
        hxTriggers
            .With(response)
            // Set the focus to the modal dismiss
            .Add(new HxFocusTrigger("#delete-modal-dismiss-action"))
            .Build();
        // Render the modal content
        return response.RenderComponent<GridDeleteModal, ItemModel?>(model, logger);
    }

    public async static Task<IResult> GetSaveModal(
        HttpResponse response,
        int id,
        IHxTriggers hxTriggers,
        ILogger<CrudHandler> logger) {
        if (id > 0) {
            // Simulate a DB latency
            await Task.Delay(200);
        }
        // Get the requested item
        var model = id == 0 ? new() : Service.Get(id);
        hxTriggers
            .With(response)
            // Set the focus to the modal first input
            .Add(new HxFocusTrigger("#save-modal input"))
            .Build();
        // Render the modal content
        return response.RenderComponent<GridSaveModal, ItemModel?>(model, logger);
    }

    public async static Task<IResult> SaveItem(
        HttpRequest request,
        HttpResponse response,
        int? id,
        ItemModel model,
        ValidationContext validationContext,
        IHxTriggers hxTriggers,
        ILogger<CrudHandler> logger) {
        // Simulate a DB latency
        await Task.Delay(200);
        if (model.Id != (id ?? 0)) {
            return TypedResults.BadRequest($"Invalid ID {id ?? 0}");
        }
        if (validationContext.Errors.Count > 0) {
            response.HxRetarget("#save-modal-form", logger);
            // The server must always send back a UTC date for datetime-local form fields
            model.Date = model.GetDateAsUtc(logger);
            hxTriggers
                .With(response)
                .Add(new HxFocusTrigger("#save-item"))
                .Build();
            return response.RenderComponent<GridSaveModal, ItemModel?>(model, logger);
        }
        // Validation passed, so save the item
        Service.Save(model);
        // Focus the new or edit button
        var triggerBuilder = hxTriggers
            .With(response)
            .Add((id ?? 0) > 0
                ? new HxFocusTrigger($"#edit-btn-{id}", true)
                : new HxFocusTrigger($"#new-btn"));
        // POST, PUT, and PATCH send metadata state as a request header instead of a parameter like GET and DELETE
        // htmx uses proper REST and HTTP semantics
        var demoGridState = request.Headers["DemoGridState"].ToString();
        var state = JsonSerializer.Deserialize<GridState>(demoGridState)!;
        var gridModel = Service.GetModel(state);
        if (!gridModel.Data.Any() && state.Page > 1) {
            //reset page back 1 if the save removed the last item on the current page due to filters used
            state.Page -= 1;
            gridModel = Service.GetModel(state);
        }
        triggerBuilder
            .Add(new HxSetMetadataTrigger(gridModel.StateScope, gridModel.StateKey, JsonSerializer.Serialize(state)))
            .Add(new HxCloseModalTrigger("#save-modal"))
            .Add(new HxToastTrigger("#crud-toast", $"Item ID: {model.Id} was {(id.HasValue ? "updated" : "created")}"))
            .Build();
        return response.RenderComponent<Grid, GridModel>(gridModel, logger);
    }

    public async static Task<IResult> DeleteItem(
        HttpResponse response,
        int id,
        string demoGridState,
        IHxTriggers hxTriggers,
        ILogger<CrudHandler> logger) {
        // Simulate a DB latency
        await Task.Delay(200);
        // Delete the item
        Service.Delete(id);
        // Load the last state object that was returned from the server
        var state = JsonSerializer.Deserialize<GridState>(HttpUtility.UrlDecode(demoGridState))!;
        var model = Service.GetModel(state);
        if (!model.Data.Any() && state.Page > 1) {
            //reset page back 1 if the delete was for the last item on the current page
            state.Page -= 1;
            model = Service.GetModel(state);
        }
        // Add triggers
        hxTriggers
            .With(response)
            // Set focus to the first delete button in the table
            .Add(new HxFocusTrigger("table tbody tr td button"))
            // Close the modal
            .Add(new HxCloseModalTrigger("#delete-modal"))
            // Pop a toast
            .Add(new HxToastTrigger("#crud-toast", $"Item ID: {id} was deleted"))
            // Set the state metadata
            .Add(new HxSetMetadataTrigger(model.StateScope, model.StateKey, JsonSerializer.Serialize(state)))
            .Build();
        // Render
        return response.RenderComponent<Grid, GridModel>(model, logger);
    }

    public async static Task<IResult> GetGrid(
        HttpResponse response,
        string? demoGridState,
        string? page,
        string? sortProperty,
        string? filterId,
        string? filterProperty,
        string? filterOperation,
        string? filterValue,
        IHxTriggers hxTriggers,
        ILogger<CrudHandler> logger) {
        // Simulate a DB latency
        await Task.Delay(200);
        // Load the state, which may be part of the request or new
        GridState state = string.IsNullOrWhiteSpace(demoGridState)
            ? new()
            : JsonSerializer.Deserialize<GridState>(HttpUtility.UrlDecode(demoGridState))!;
        // Update the state based on the grid action (sort, page, filter) sent in the request.
        state.Update(page, sortProperty, filterId, filterProperty, filterOperation, filterValue);
        // Get the page data and total count based on the new state
        var model = Service.GetModel(state);
        if (!model.Data.Any() && state.Page > 1) {
            //reset page to 1 if no data was found for current filters/page
            state.Page = 1;
            model = Service.GetModel(state);
        }
        // Trigger the persistence of the state on the client   
        var serializedState = JsonSerializer.Serialize(state);
        var triggerBuilder = hxTriggers
            .With(response)
            .Add(new HxSetMetadataTrigger(model.StateScope, model.StateKey, serializedState));
        // Optionally add the state to the URL - useful for allowing the state to be transferred to another client by copying the link
        // You may remove this and everything will continue function as expected, only with a "clean" URL
        response.HxReplaceUrl($"/examples/crud?{nameof(demoGridState)}={HttpUtility.UrlEncode(serializedState)}");
        // Pop toast for filter change
        if (!string.IsNullOrWhiteSpace(filterProperty)) {
            triggerBuilder
                .Add(new HxToastTrigger("#crud-toast", "Filter added"))
                .Add(new HxFocusTrigger("#filter-selector"));
        }
        if (!string.IsNullOrWhiteSpace(filterId)) {
            triggerBuilder
                .Add(new HxToastTrigger("#crud-toast", "Filter removed"));
        }
        // Grid focus management - this is optional, but provides a good experience for keyboard users
        if (!string.IsNullOrWhiteSpace(page)) {
            // Trigger focus on the pager button
            page = page switch {
                "next" => state.HasNextPage() ? "next" : "previous",
                "previous" => state.HasPreviousPage() ? "previous" : "next",
                _ => "range"
            };
            if (page == "range") {
                triggerBuilder
                    .Add(new HxFocusTrigger($"[name=\"{nameof(GridState.Page)}\"][type=\"range\"]"));
            } else {
                triggerBuilder
                    .Add(new HxFocusTrigger($"[name=\"{nameof(GridState.Page)}\"][value=\"{page}\"]"));
            }
        }
        if (!string.IsNullOrWhiteSpace(sortProperty)) {
            // Trigger focus on the sort button
            triggerBuilder
                .Add(new HxFocusTrigger($"[name=\"{nameof(GridState.SortProperty)}\"][value=\"{sortProperty}\"]"));
        }
        if (!string.IsNullOrWhiteSpace(filterId)) {
            // Trigger focus on filter removal
            triggerBuilder
                .Add(state.Filters.Count == 0
                    ? new HxFocusTrigger("#filter-selector")
                    : new HxFocusTrigger($"[name=\"{nameof(DataSetFilter.FilterId)}\"][value=\"{state.Filters[0].FilterId}\"]"));
        }
        // Build triggers
        triggerBuilder.Build();
        // Render
        return response.RenderComponent<Grid, GridModel>(model, logger);
    }

}
