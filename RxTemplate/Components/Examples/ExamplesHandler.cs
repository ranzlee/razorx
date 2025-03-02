using RxTemplate.Components.Layout;
using RxTemplate.Rx;

namespace RxTemplate.Components.Examples;

public class ExamplesHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {
        router.AddRoutePath(RequestType.GET, "/examples", Get)
            .AllowAnonymous()
            .WithRxRootComponent<App>();
    }

    public static IResult Get(HttpResponse response, ILogger<ExamplesHandler> logger) {
        return response.RenderComponent<ExamplesPage>(logger);
    }
}
