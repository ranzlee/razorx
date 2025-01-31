using FluentValidation;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Crud;

public class ItemValidator : Validator<ItemModel> {
    public ItemValidator(ValidationContext validationContext, ILogger<ItemValidator> logger)
    : base(validationContext, logger) {

        RuleFor(x => x.Summary)
            .NotEmpty()
            .WithMessage("Summary is required");

        RuleFor(x => x.TemperatureC)
            .NotEmpty()
            .WithMessage("Temp. C is required")
            .InclusiveBetween(-20, 55)
            .WithMessage("Temp. C must be between -20 and 55");

        RuleFor(x => x.Date)
            .NotNull()
            .WithMessage("Date is required")
            .InclusiveBetween(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow.AddYears(5))
            .WithMessage("Date must be between 5 years in the past and 5 years in the future");

        RuleFor(x => x.TemperatureTaken)
            .NotEqual(TimeOfDay.Select)
            .WithMessage("Observed is required");
    }
}