using RxTemplate.Rx;

namespace RxTemplate.Components.Home;

public class HomeHandler : IRequestHandler {
    public static IResult Get(HttpResponse response, ILogger<HomeHandler> logger) {
        return response.RenderComponent<HomePage>(logger);
    }
}

