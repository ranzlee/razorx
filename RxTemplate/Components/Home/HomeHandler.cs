using RxTemplate.Components.Layout;
using RxTemplate.Rx;

namespace RxTemplate.Components.Home;

public class HomeHandler : IRequestHandler {

    public void MapRoutes(IEndpointRouteBuilder router) {
        router.AddRoutePath(RequestType.GET, "/", Get)
            .AllowAnonymous()
            .WithRxRootComponent<App>();
    }

    public static IResult Get(HttpResponse response, ILogger<HomeHandler> logger) {
        return response.RenderComponent<HomePage>(logger);
    }
}

