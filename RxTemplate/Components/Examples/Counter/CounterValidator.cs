using FluentValidation;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Counter;

public class CounterValidator : Validator<CounterValidator, CounterModel> {
    public CounterValidator(ValidationContext validationContext, ILogger<CounterValidator> logger)
    : base(validationContext, logger) {
        //validation occurs after model binding and before endpoint handler, so...
        //shift -1 on add
        RuleFor(x => x.Count)
            .InclusiveBetween(-6, 4)
            .When(x => x.IsAdd.HasValue)
            .WithMessage("Value must be between -5 and 5.");
        //shift +1 on subtract
        RuleFor(x => x.Count)
            .InclusiveBetween(-4, 6)
            .When(x => !x.IsAdd.HasValue)
            .WithMessage("Value must be between -5 and 5.");

        //if you want to validate manually in the handler...
        //REMOVE from the route
        //************************************************************
        // .WithValidation<CounterValidator>()
        //************************************************************
        //ADD to the handler after modifying the model
        //************************************************************
        // var validationResult = await ValidateAsync(model);
        // validationContext.ValidationResults = validationResult;
        //************************************************************

    }
}