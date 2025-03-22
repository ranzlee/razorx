using RxTemplate.Components.Rx.Headless.AutoComplete;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Form;

public class FormHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {
        router.MapGet("/examples/form", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapGet("/examples/form/widgets", GetWidgets)
            .AllowAnonymous();

        router.MapPost("/examples/form/widget/{id}", PostWidget)
            .AllowAnonymous();

        router.MapPatch("/examples/form/validate", ValidateForm)
            .WithRxValidation<FormValidator>()
            .AllowAnonymous();

        router.MapPost("/examples/form/submit", SubmitForm)
            .WithRxValidation<FormValidator>()
            .AllowAnonymous();
    }

    public static IResult Get(HttpResponse response, ILogger<FormHandler> logger) {
        return response.RenderComponent<FormPage>(logger);
    }

    public static IResult GetWidgets(
        HttpResponse response,
        string widget,
        ILogger<FormHandler> logger) {
        var widgets = MockWidgetService.Find(widget);
        var m = new RxAutoCompleteModel {
            SortExactMatchesFirst = true,
            SearchPattern = widget,
            Items = widgets
        };
        return response.RenderComponent<RxAutoCompleteList, RxAutoCompleteModel>(m, logger);
    }

    public static IResult PostWidget(
        HttpResponse response,
        string id,
        ILogger<FormHandler> logger
    ) {
        return TypedResults.NoContent();
    }

    public static IResult ValidateForm(
        HttpResponse response,
        FormModel model,
        ILogger<FormHandler> logger) {
        return response.RenderComponent<Form, FormModel>(model, logger);
    }

    public static IResult SubmitForm(
        HttpResponse response,
        FormModel model,
        ValidationContext validationContext,
        IHxTriggers hxTriggers,
        ILogger<FormHandler> logger) {
        if (validationContext.Errors.Count > 0) {
            // The server must always send back a UTC date for datetime-local form fields
            if (model.AppointmentTime.HasValue) {
                model.AppointmentTime = model.GetAppointmentTimeAsUtc(logger);
            }
            hxTriggers
                .With(response)
                .Add(new HxFocusTrigger("#form-submit"))
                .Build();
            // re-render the form with validation errors
            response.HxRetarget("#form-change-validator", logger);
            return response.RenderComponent<Form, FormModel>(model, logger);
        }
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#form-toast", "Form was submitted"))
            .Build();
        return response.RenderComponent<FormPage>(logger);
    }

}
