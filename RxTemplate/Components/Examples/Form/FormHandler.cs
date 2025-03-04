using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Form;

public class FormHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {
        router.AddRoutePath(RequestType.GET, "/examples/form", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.AddRoutePath(RequestType.PATCH, "/examples/form/validate", ValidateForm)
            .WithRxValidation<FormValidator>()
            .AllowAnonymous();

        router.AddRoutePath(RequestType.POST, "/examples/form/submit", SubmitForm)
            .WithRxValidation<FormValidator>()
            .AllowAnonymous();
    }

    public static IResult Get(HttpResponse response, ILogger<FormHandler> logger) {
        return response.RenderComponent<FormPage>(logger);
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
