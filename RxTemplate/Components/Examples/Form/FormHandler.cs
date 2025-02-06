﻿using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Form;

public class FormHandler : IRequestHandler {
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
            response.HxRetarget("#input-form", logger);
            return response.RenderComponent<Form, FormModel>(model, logger);
        }
        hxTriggers
            .With(response)
            .Add(new HxToastTrigger("#form-toast", "Form was submitted"))
            .Build();
        return response.RenderComponent<FormPage>(logger);
    }

}
