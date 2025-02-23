using FluentValidation;
using FluentValidation.Results;

namespace RxTemplate.Rx;

/// <summary>
/// The validation context for a model.
/// </summary>
public sealed class ValidationContext {
    public ValidationResult ValidationResult { get; set; } = new();
    public List<ValidationFailure> Errors => ValidationResult.Errors;
}

/// <summary>
/// Runs ValidateAsync on a model
/// </summary>
/// <typeparam name="TModel">Model type</typeparam>
/// <param name="validationContext">ValidationContext</param>
/// <param name="logger">ILogger</param>
public class Validator<TModel>(ValidationContext validationContext, ILogger? logger = null)
: AbstractValidator<TModel>, IEndpointFilter {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        var model = context.Arguments.OfType<TModel>().SingleOrDefault();
        if (model is null) {
            logger?.LogError("Request model {model} cannot be resolved for validation", typeof(TModel));
            return Results.BadRequest($"Model of type {typeof(TModel)} is not included in the request.");
        }
        var validationResult = await ValidateAsync(model);
        logger?.LogInformation("Validation for {model}: {result} {results}",
            typeof(TModel),
            validationResult.IsValid ? "Passed" : $"Failed - Details:\n",
            validationResult);
        validationContext.ValidationResult = validationResult;
        return await next(context);
    }
}

public static class ValidationExtensions {

    /// <summary>
    /// Adds a validator endpoint filter to the route.
    /// </summary>
    /// <typeparam name="TValidator">IEndpointFilter</typeparam>
    /// <param name="routeBuilder">RouteHandlerBuilder</param>
    /// <returns>RouteHandlerBuilder</returns>
    public static RouteHandlerBuilder WithRxValidation<TValidator>(this RouteHandlerBuilder routeBuilder)
    where TValidator : IEndpointFilter {
        return routeBuilder.AddEndpointFilter<TValidator>();
    }

    /// <summary>
    /// Gets the validation results for a model property from the ValidationContext
    /// </summary>
    /// <param name="validationContext">ValidationContext</param>
    /// <param name="PropertyName">String model property name/param>
    /// <param name="error">ValidationFailure</param>
    /// <returns></returns>
    public static bool TryGetError(this ValidationContext validationContext, string PropertyName, out ValidationFailure error) {
        error = validationContext.Errors.FirstOrDefault(x => x.PropertyName == PropertyName)!;
        return error is not null;
    }
}