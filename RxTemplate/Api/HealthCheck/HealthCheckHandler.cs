namespace RxTemplate.Api.HealthCheck;

public static class HealthCheckHandler {
    public static IResult Get () {
        return Results.Json(new { Status = "ok" }); 
    }
}