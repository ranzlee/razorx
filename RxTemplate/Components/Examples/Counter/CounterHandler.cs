using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Counter;

public class CounterHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {
        router.MapGet("/examples/counter", Get)
            .AllowAnonymous()
            .WithRxRootComponent();

        router.MapPost("/examples/counter/update", UpdateCounter)
            .WithRxValidation<CounterValidator>()
            .AllowAnonymous();
    }

    public static IResult Get(HttpResponse response, ILogger<CounterHandler> logger) {
        return response.RenderComponent<CounterPage>(logger);
    }

    public static IResult UpdateCounter(
        HttpResponse response,
        CounterModel model,
        IHxTriggers hxTriggers,
        ILogger<CounterHandler> logger) {
        model.Count = model.IsAdd.HasValue ? model.Count + 1 : model.Count - 1;
        hxTriggers
            .With(response)
            .Add(new HxFocusTrigger(model.IsAdd == true ? "#increment-btn" : "#decrement-btn"))
            .Build();
        return response.RenderComponent<Counter, CounterModel>(model, logger);
    }
}
