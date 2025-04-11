using RxTemplate.Api.HealthCheck;

namespace RxTemplate.Api;

public static class RoutingExtensions {    
    public static void UseApiRouter(this WebApplication app, string routePrefix = "api") {
        var router = app.MapGroup(routePrefix);
        // health-check
        router.MapGet("health-check", HealthCheckHandler.Get);
        // add other JSON APIs here
    }
}
