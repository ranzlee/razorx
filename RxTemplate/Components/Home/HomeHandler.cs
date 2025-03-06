using RxTemplate.Rx;

namespace RxTemplate.Components.Home;

public class HomeHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {
        router.MapGet("/", Get)
            .AllowAnonymous()
            .WithRxRootComponent();
    }

    public static IResult Get(HttpResponse response, ILogger<HomeHandler> logger) {
        return response.RenderComponent<HomePage>(logger);
    }
}

