using RxTemplate.Rx;

namespace RxTemplate.Components.Examples;

public class ExamplesHandler : IRequestHandler {
    public static IResult Get(HttpResponse response, ILogger<ExamplesHandler> logger) {
        return response.RenderComponent<ExamplesPage>(logger);
    }
}
