using RxTemplate.Rx;

namespace RxTemplate.Components.Examples.Counter;

public class CounterHandler : IRequestHandler {
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
            .AddTrigger(new HxFocusTrigger(model.IsAdd == true ? "#increment-btn" : "#decrement-btn"))
            .Build();
        return response.RenderComponent<Counter, CounterModel>(model, logger);
    }
}
