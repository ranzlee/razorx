using FluentValidation;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Counter;

public class CounterValidator : Validator<CounterModel> {
    public CounterValidator(ValidationContext validationContext, ILogger<CounterValidator> logger)
    : base(validationContext, logger) {
        //validation occurs after model binding and before endpoint handler, so...
        RuleFor(x => x.Count + 1)
            .InclusiveBetween(-5, 5)
            .When(x => x.IsAdd.HasValue)
            .WithName(nameof(CounterModel.Count))
            .WithMessage("Value must be between -5 and 5.");
        RuleFor(x => x.Count - 1)
            .InclusiveBetween(-5, 5)
            .When(x => !x.IsAdd.HasValue)
            .WithName(nameof(CounterModel.Count))
            .WithMessage("Value must be between -5 and 5.");

        //if you want to validate manually in the handler...
        //REMOVE from the route
        //************************************************************
        // .WithRxValidation<CounterValidator>()
        //************************************************************
        //ADD to the handler after modifying the model
        //************************************************************
        // var validationResult = await ValidateAsync(model);
        // validationContext.ValidationResult = validationResult;
        //************************************************************

    }
}