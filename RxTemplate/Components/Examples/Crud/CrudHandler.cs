using RxTemplate.Components.Rx.Headless.Grid;
using RxTemplate.Rx;
using System.Text.Json;
using System.Web;

namespace RxTemplate.Components.Examples.Crud;

public class CrudHandler : IRequestHandler {

    /// <summary>
    /// A mock service/repository that would be replaced with a real implementation and injected in.
    /// </summary>
    private readonly static MockCrudService Service = new();

    public static IResult Get(HttpResponse response, ILogger<CrudHandler> logger) {
        // Render the page
        return response.RenderComponent<CrudPage>(logger);
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
        return response.RenderComponent<GridFilter, GridFilterModel>(model, logger);
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
            .AddTrigger(new HxFocusTrigger("#delete-modal-dismiss-action"))
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
            .AddTrigger(new HxFocusTrigger("#save-modal input"))
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
        if (validationContext.Errors.Count > 0) {
            response.HxRetarget("#save-modal-form", logger);
            // The server must always send back a UTC date for datetime-local form fields
            model.Date = model.GetDateAsUtc(logger);
            hxTriggers
                .With(response)
                .AddTrigger(new HxFocusTrigger("#save-item"))
                .Build();
            return response.RenderComponent<GridSaveModal, ItemModel?>(model, logger);
        }
        var triggerBuilder = hxTriggers.With(response);
        // Validation passed, so save the item
        if (id.HasValue) {
            // PUT sends ID as parameter
            model.Id = id.Value;
            triggerBuilder.AddTrigger(new HxFocusTrigger($"#edit-btn-{id}", true));
        }
        else {
            triggerBuilder.AddTrigger(new HxFocusTrigger($"#new-btn"));
        }
        Service.Save(model);
        // POST, PUT, and PATCH send metadata state as a request header instead of a parameter like GET and DELETE
        // htmx uses proper REST and HTTP semantics
        var demoGridState = request.Headers["DemoGridState"].ToString();
        var state = JsonSerializer.Deserialize<GridState>(demoGridState)!;
        var gridModel = Service.GetModel(state);
        triggerBuilder.AddTrigger(new HxCloseModalTrigger("#save-modal"));
        triggerBuilder.AddTrigger(new HxToastTrigger("#crud-toast", $"Item ID: {model.Id} was {(id.HasValue ? "updated" : "created")}"));
        triggerBuilder.Build();
        return response.RenderComponent<Grid, IGridModel<ItemModel>>(gridModel, logger);
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
            .AddTrigger(new HxFocusTrigger("table tbody tr td button"))
            // Close the modal
            .AddTrigger(new HxCloseModalTrigger("#delete-modal"))
            // Pop a toast
            .AddTrigger(new HxToastTrigger("#crud-toast", $"Item ID: {id} was deleted"))
            // Set the state metadata
            .AddTrigger(new HxSetMetadataTrigger(model.StateScope, model.StateKey, JsonSerializer.Serialize(state)))
            .Build();
        // Render
        return response.RenderComponent<Grid, IGridModel<ItemModel>>(model, logger);
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
        GridState state = new();
        if (!string.IsNullOrWhiteSpace(demoGridState)) {
            // This state is the last state object that was returned from the server
            state = JsonSerializer.Deserialize<GridState>(HttpUtility.UrlDecode(demoGridState))!;
        }
        // Update the state based on the grid action (sort, page, filter) sent in the request.
        state.Update(page, sortProperty, filterId, filterProperty, filterOperation, filterValue);
        // Get the page data and total count based on the new state
        var model = Service.GetModel(state);
        if (!model.Data.Any() && state.Page > 1) {
            //reset page to 1 if no data was found for current filters/page
            state.Page = 1;
            model = Service.GetModel(state);
        }
        var triggerBuilder = hxTriggers.With(response);
        // Trigger the persistence of the state on the client    
        triggerBuilder.AddTrigger(new HxSetMetadataTrigger(model.StateScope, model.StateKey, JsonSerializer.Serialize(state)));
        // Pop toast for filter change
        if (!string.IsNullOrWhiteSpace(filterProperty)) {
            triggerBuilder.AddTrigger(new HxToastTrigger("#crud-toast", "Filter added"));
        }
        if (!string.IsNullOrWhiteSpace(filterId)) {
            triggerBuilder.AddTrigger(new HxToastTrigger("#crud-toast", "Filter removed"));
        }
        // Grid focus management - this is optional, but provides a good experience for keyboard users
        if (!string.IsNullOrWhiteSpace(page)) {
            // Trigger focus on the pager button
            page = page switch {
                "next" => state.HasNextPage() ? "next" : "previous",
                "previous" => state.HasPreviousPage() ? "previous" : "next",
                _ => page
            };
            triggerBuilder.AddTrigger(new HxFocusTrigger($"[name=\"Page\"][value=\"{page}\"]"));
        }
        if (!string.IsNullOrWhiteSpace(sortProperty)) {
            // Trigger focus on the sort button
            triggerBuilder.AddTrigger(new HxFocusTrigger($"[name=\"SortProperty\"][value=\"{sortProperty}\"]"));
        }
        if (!string.IsNullOrWhiteSpace(filterId)) {
            // Trigger focus on filter removal
            if (state.Filters.Count != 0) {
                triggerBuilder.AddTrigger(new HxFocusTrigger($"[name=\"FilterId\"][value=\"{state.Filters[0].FilterId}\"]"));
            }
            else {
                triggerBuilder.AddTrigger(new HxFocusTrigger($"[name=\"FilterProperty\"]"));
            }
        }
        //build triggers
        triggerBuilder.Build();
        // Render
        return response.RenderComponent<Grid, IGridModel<ItemModel>>(model, logger);
    }

}
