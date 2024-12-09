using RxTemplate.Rx;

namespace RxTemplate.Components.Error;

public class ErrorHandler : IRequestHandler {
    public static IResult Get(HttpResponse response, ErrorModel model) {
        return response.RenderComponent<ErrorPage, ErrorModel>(model);
    }
}
