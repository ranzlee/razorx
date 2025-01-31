using FluentValidation;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Form;

public class FormValidator : Validator<FormModel> {
    public FormValidator(ValidationContext validationContext, ILogger<FormValidator> logger)
    : base(validationContext, logger) {

        RuleFor(x => x.Name)
           .NotEmpty()
           .WithMessage("Name is required");

        RuleFor(x => x.Email)
           .NotEmpty()
           .WithMessage("Email is required")
           .EmailAddress()
           .WithMessage("Not a valid email address");

        RuleFor(x => x.BirthDate)
           .NotNull()
           .WithMessage("Birth Date is required")
           .LessThan(DateOnly.FromDateTime(DateTime.Today.AddDays(1)))
           .WithMessage("Must not be in the future");

        RuleFor(x => x.AppointmentTime)
           .NotNull()
           .WithMessage("Appointment Time is required");

        // need to verify date/time as UTC
        RuleFor(x => x.GetAppointmentTimeAsUtc(logger))
           .GreaterThan(DateTime.UtcNow)
           .When(x => x.AppointmentTime is not null)
           .WithName(nameof(FormModel.AppointmentTime))
           .WithMessage("Must be in the future");

        RuleFor(x => x.NumberOfDays)
           .NotNull()
           .WithMessage("Number of Days is required")
           .InclusiveBetween(1, 7)
           .WithMessage("Must be between 1 and 7");

        RuleFor(x => x.IsPublished)
            .Equal(true)
            .When(x => x.NumberOfDays >= 3)
            .WithMessage("Must be published when Number of Days >= 3");

        RuleFor(x => x.Cost)
           .NotNull()
           .WithMessage("Cost is required")
           .InclusiveBetween(10, 100)
           .WithMessage("Must be between 10 and 100");

        RuleFor(x => x.ReportingStatus)
            .Equal(ReportingStatusType.Completed)
            .When(x => x.IsPublished)
            .WithMessage("Must be completed when published");

        RuleFor(x => x.Subscription)
            .NotEqual(SubscriptionType.NoRenewal)
            .When(x => x.Cost >= 30)
            .WithMessage("Renewal subscription required when cost >= 30");

        RuleFor(x => x.Notes)
            .NotEmpty()
            .When(x => x.ReportingStatus == ReportingStatusType.Completed)
            .WithMessage("Notes required when reporting is complete");
    }
}