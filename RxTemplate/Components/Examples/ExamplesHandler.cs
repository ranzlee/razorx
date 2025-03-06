using RxTemplate.Rx;

namespace RxTemplate.Components.Examples;

public class ExamplesHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {
        router.MapGet("/examples", Get)
            .AllowAnonymous()
            .WithRxRootComponent();
    }

    public static IResult Get(HttpResponse response, ILogger<ExamplesHandler> logger) {
        return response.RenderComponent<ExamplesPage>(logger);
    }
}
