using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Extensions;

namespace RxTemplate.Rx;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class WithRxSkipAntiforgeryValidationAttribute : Attribute { }

public class WithRxSkipAntiforgeryValidation() : IEndpointFilter {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        return await next(context);
    }
}

public static class AntiforgeryExtensions {

    public static RouteHandlerBuilder WithRxSkipAntiforgeryValidation(this RouteHandlerBuilder routeBuilder) {
        return routeBuilder
            .AddEndpointFilter<WithRxSkipAntiforgeryValidation>()
            .WithMetadata(new WithRxSkipAntiforgeryValidationAttribute());
    }

    /// <summary>
    /// Use the AntiforgeryCookieMiddleware to add the RequestVerificationToken to the GET response cookies 
    /// and validate the cookie on non-GET requests. This eliminates the need to use a hidden field or the <AntiforgeryToken/>
    /// component as protection for CSRF attacks. On the client, the razorx.pipeline.configureRequest() method
    /// listens for the htmx:config-request event to add the token to the request headers.
    /// </summary>
    /// <param name="app">WebApplication</param>
    public static void UseAntiforgeryCookie(this WebApplication app) {
        app.UseMiddleware<AntiforgeryCookieMiddleware>();
    }
}

/// <summary>
/// Middleware that adds the RequestVerificationToken to the GET response cookies and validates the 
/// cookie on non-GET requests. This eliminates the need to use a hidden field or the <AntiforgeryToken/>
/// component as protection for CSRF attacks. On the client, the razorx.pipeline.configureRequest() method
/// listens for the htmx:config-request event to add the token to the request headers.
/// </summary>
/// <param name="next">The next request delegate in the pipeline.</param>
public sealed class AntiforgeryCookieMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery, ILogger<AntiforgeryCookieMiddleware> logger) {
        if (context.Request.Method.Trim().Equals("GET", StringComparison.CurrentCultureIgnoreCase)) {
            // Return an antiforgery token in the response for GET requests
            var tokenSet = antiforgery.GetAndStoreTokens(context);
            logger.LogTrace("Adding Antiforgery token cookie for {method}:{request}.",
                context.Request.Method,
                context.Request.GetDisplayUrl());
            context.Response.Cookies.Append("RequestVerificationToken", tokenSet.RequestToken!,
                new CookieOptions {
                    HttpOnly = false,
                    Secure = true,
                    SameSite = SameSiteMode.Strict
                });
            await next(context);
            return;
        }
        // Check for explicit antiforgery skip validation
        var endpoint = context.GetEndpoint();
        if (endpoint is not null
        && endpoint.Metadata.GetMetadata<WithRxSkipAntiforgeryValidationAttribute>() is not null) {
            await next(context);
            return;
        }
        // Validate antiforgery token for non-GET requests
        logger.LogTrace("Validating Antiforgery token for {method}:{request}.",
            context.Request.Method,
            context.Request.GetDisplayUrl());
        await antiforgery.ValidateRequestAsync(context);
        await next(context);
    }
}