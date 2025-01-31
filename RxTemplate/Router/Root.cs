using RxTemplate.Components.Error;
using RxTemplate.Components.Home;
using RxTemplate.Components.Layout;
using RxTemplate.Rx;

namespace RxTemplate.Router;

public static class Root {
    public static RouteGroupBuilder MapRoot(this WebApplication app) {

        // The default group without a prefix is for serving HTML pages and HTMX partials.
        // If you also need to serve JSON APIs, MapGroup("api") and add JSON API routes there.
        var routes = app.MapGroup("")
            // The RouteHandler and ErrorHandler filters add the correct behavior for 
            // Minimal APIs that return RazorComponentResults. They are not used for 
            // JSON APIs.
            .WithRouteHandling()
            .WithErrorHandling<App, ErrorPage>();

        // Common routes
        routes.AddRoutePath(RequestType.GET, "/", HomeHandler.Get)
            .AllowAnonymous()
            // The PageRouteFor filter identifies the root (layout) component for the page.
            // Alternatively, the PageRouteForAttribute may be applied to the endpoint
            // handler directly.
            .PageRouteFor<App>();

        return routes;
    }
}