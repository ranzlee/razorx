using RxTemplate.Rx;

namespace RxTemplate.Components.Error;

public class ErrorHandler {
    public static IResult Get(HttpResponse response, ErrorModel model) {
        return response.RenderComponent<ErrorPage, ErrorModel>(model);
    }
}
