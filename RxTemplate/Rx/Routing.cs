using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Extensions;

namespace RxTemplate.Rx;

/// <summary>
/// Enumerated HTTP methods.
/// </summary>
public enum RequestType {
    GET,
    POST,
    PUT,
    PATCH,
    DELETE,
}

public static class RoutingExtensions {

    public static void UseRouter<TRootComponent, TErrorPage>(this WebApplication app, string routePrefix = "")
        where TRootComponent : IRootComponent
        where TErrorPage : IComponent, IComponentModel<ErrorModel> {

        var router = app.MapGroup(routePrefix)
            .WithRxRouteHandling()
            .WithRxErrorHandling<TRootComponent, TErrorPage>();

        // Inspect for IRouteGroups 
        var routeGroups = Assembly.GetExecutingAssembly().DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false }
                && type.IsAssignableTo(typeof(IRequestHandler)))
            .Select(type => Activator.CreateInstance(type) as IRequestHandler)
            .ToArray();

        // Map routes for IRouteGroups found
        foreach (var routeGroup in routeGroups) {
            routeGroup?.MapRoutes(router);
        }
    }

    /// <summary>
    /// Helper to define a route handler endpoint delegate.
    /// </summary>
    /// <param name="routeBuilder">IEndpointRouteBuilder</param>
    /// <param name="requestType">Http enum</param>
    /// <param name="routePath">Route path</param>
    /// <param name="endpointHandler">Delegate handler</param>
    /// <returns>RouteHandlerBuilder</returns>
    /// <exception cref="NotSupportedException">Unsupported HTTP method</exception>
    public static RouteHandlerBuilder AddRoutePath(
        this IEndpointRouteBuilder routeBuilder,
        RequestType requestType,
        string routePath,
        Delegate endpointHandler) {
        return requestType switch {
            RequestType.GET => routeBuilder.MapGet(routePath, endpointHandler),
            RequestType.POST => routeBuilder.MapPost(routePath, endpointHandler),
            RequestType.PUT => routeBuilder.MapPut(routePath, endpointHandler),
            RequestType.PATCH => routeBuilder.MapPatch(routePath, endpointHandler),
            RequestType.DELETE => routeBuilder.MapDelete(routePath, endpointHandler),
            _ => throw new NotSupportedException($"HTTP method '{requestType}' is not supported")
        };
    }

    public static RouteGroupBuilder WithRxRouteHandling(this RouteGroupBuilder routeBuilder) {
        return routeBuilder.AddEndpointFilter<RouteHandler>();
    }

    public static RouteGroupBuilder WithRxErrorHandling<TFallbackRootComponent, TComponent>(this RouteGroupBuilder routeBuilder)
    where TFallbackRootComponent : IRootComponent
    where TComponent : IComponent, IComponentModel<ErrorModel> {
        routeBuilder.MapFallback(static (context) => {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return Task.CompletedTask;
        });
        return routeBuilder.AddEndpointFilter<ErrorHandler<TFallbackRootComponent, TComponent>>();
    }


    /// <summary>
    /// Adds the WithAttribute to the endpoint via the RouteHandlerBuilder if the preference is not to 
    /// declare the attribute on the endpoint explicitly.
    /// </summary>
    /// <typeparam name="TRootComponent"></typeparam>
    /// <param name="routeBuilder"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder WithRxRootComponent<TRootComponent>(this RouteHandlerBuilder routeBuilder)
    where TRootComponent : IRootComponent {
        return routeBuilder
            .AddEndpointFilter<WithRxRootComponent>()
            .WithMetadata(new WithRxRootComponentAttribute<TRootComponent>());
    }

    public static RouteHandlerBuilder WithRxSkipRouteHandling(this RouteHandlerBuilder routeBuilder) {
        return routeBuilder
            .AddEndpointFilter<WithRxSkipRouteHandling>()
            .WithMetadata(new WithRxSkipRouteHandlingAttribute());
    }
}

public class RouteHandler(ILogger<RouteHandler> logger) : IEndpointFilter {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        // Verify is GET request.
        if (context.HttpContext.Request.Method != RequestType.GET.ToString()) {
            logger.LogTrace("Skip pre-route processing for non-get request {method}:{request}.",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
            return await next(context);
        }
        // Verify endpoint.
        var endpoint = context.HttpContext.GetEndpoint();
        if (endpoint is null) {
            logger.LogTrace("Skip pre-route processing for non-determined endpoint {method}:{request}.",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
            // If no endpoint return 404.
            context.HttpContext.Items.Add(nameof(ErrorModel), new ErrorModel(HttpStatusCode.NotFound));
            return await next(context);
        }
        // Check for skip Rx custom route processing metadata.    
        var skipRouteHandling = endpoint.Metadata.GetMetadata<WithRxSkipRouteHandlingAttribute>();
        if (skipRouteHandling is not null) {
            return await next(context);
        }
        // Check if the request is a not-boosted htmx request.
        if (context.HttpContext.Request.IsHxRequest() && !context.HttpContext.Request.IsHxBoosted()) {
            // htmx request, so call next middleware and bailout 
            logger.LogTrace("Skip pre-route processing for htmx partial request {method}:{request}.",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
            return await next(context);
        }
        // Check for razor component response metadata.    
        var rootComponentAttr = endpoint.Metadata.GetMetadata<IWithRxRootComponentAttribute>();
        // Full page request to a partial component
        if (rootComponentAttr is null) {
            logger.LogTrace("No WithRxRootComponentAttribute for request {method}:{request}. Responding with 404 NOT FOUND.",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
            // The status code may have been sent by the client after a handler response error
            if (context.HttpContext.Request.Query.Any(x => x.Key == "code")) {
                var code = context.HttpContext.Request.Query.FirstOrDefault(x => x.Key == "code");
                if (int.TryParse(code.Value, out var val)
                && Enum.TryParse<HttpStatusCode>(val.ToString(), out var status)) {
                    context.HttpContext.Items.Add(nameof(ErrorModel), new ErrorModel(status));
                    return await next(context);
                }
            }
            // Default to 404 since the route may only be valid as a partial via hx-[verb]
            context.HttpContext.Items.Add(nameof(ErrorModel), new ErrorModel(HttpStatusCode.NotFound));
            return await next(context);
        }
        // Add the root component type to the context.
        var rootComponent = rootComponentAttr.GetRootComponentType();
        logger.LogTrace("Adding WithRxRootComponent context item for root component type {rootComponent} for request {method}:{request}.",
                rootComponent,
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl());
        context.HttpContext.Items.Add(nameof(IWithRxRootComponentAttribute), rootComponent);
        return await next(context);
    }
}

public interface IWithRxRootComponentAttribute {
    public Type GetRootComponentType();
}

/// <summary>
/// Identifies an endpoint as a route that should return a complete page.
/// </summary>
/// <typeparam name="TRootComponent">The root component that is the layout for the page.</typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class WithRxRootComponentAttribute<TRootComponent> : Attribute, IWithRxRootComponentAttribute
where TRootComponent : IRootComponent {
    public Type GetRootComponentType() {
        return typeof(TRootComponent);
    }
}

/// <summary>
/// Identifies an endpoint as a route that should return a complete page.
/// </summary>
public class WithRxRootComponent() : IEndpointFilter {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        return await next(context);
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class WithRxSkipRouteHandlingAttribute : Attribute { }

public class WithRxSkipRouteHandling() : IEndpointFilter {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        return await next(context);
    }
}

public class ErrorHandler<TFallbackRootComponent, TComponent>(ILogger<ErrorHandler<TFallbackRootComponent, TComponent>> logger) : IEndpointFilter
where TFallbackRootComponent : IRootComponent
where TComponent : IComponent, IComponentModel<ErrorModel> {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        if (context.HttpContext.Items.ContainsKey(nameof(ErrorModel))) {
            var model = context.HttpContext.Items[nameof(ErrorModel)] as ErrorModel;
            logger.LogInformation("Error for request {method}:{request} with model {model}.",
               context.HttpContext.Request.Method,
               context.HttpContext.Request.GetDisplayUrl(),
               model is null ? "null" : model.ToString());
            // Add the layout component
            context.HttpContext.Items.Add(nameof(IWithRxRootComponentAttribute), typeof(TFallbackRootComponent));
            //short circuit and return error
            if (model is null) {
                return context.HttpContext.Response.RenderComponent<TComponent>();
            }
            logger.LogInformation("Error for request {method}:{request} - responding with status code {statusCode}.",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.GetDisplayUrl(),
                model.StatusCode);
            context.HttpContext.Response.StatusCode = (int)model.StatusCode;
            return context.HttpContext.Response.RenderComponent<TComponent, ErrorModel>(model);
        }
        return await next(context);
    }
}


