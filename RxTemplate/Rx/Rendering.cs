using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.HttpResults;

namespace RxTemplate.Rx;

public static class RazorComponentRenderer {

    /// <summary>
    /// Renders a RazorComponent response.
    /// </summary>
    /// <typeparam name="TComponent">RazorComponent type to render</typeparam>
    /// <typeparam name="TModel">Model type to bind</typeparam>
    /// <param name="response">HttpResponse</param>
    /// <param name="model">Model instance</param>
    /// <param name="logger">ILogger</param>
    /// <returns>RazorComponentResult</returns>
    public static RazorComponentResult RenderComponent<TComponent, TModel>(this HttpResponse response, TModel model, ILogger<IRequestHandler>? logger = null)
    where TComponent : IComponent, IComponentModel<TModel> {
        var root = response.HttpContext.GetRootComponent();
        if (root is not null) {
            var parameters = new Dictionary<string, object?> {
                { nameof(IRootComponent.MainContent), typeof(TComponent) },
                { nameof(IRootComponent.MainContentParameters), new Dictionary<string, object?> {{ nameof(IComponentModel<TModel>.Model), model }}},
            };
            logger?.LogInformation("Rendering page - Root component: {root} - Content component: {content} - Model: {model}",
                root,
                typeof(TComponent),
                typeof(TModel));
            return new RazorComponentResult(root, parameters);
        }
        logger?.LogInformation("Rendering partial - Content component: {content} - Model: {model}",
                typeof(TComponent),
                typeof(TModel));
        return new RazorComponentResult<TComponent>(new { Model = model });
    }

    /// <summary>
    /// Renders a RazorComponent response.
    /// </summary>
    /// <typeparam name="TComponent">RazorComponent type to render</typeparam>
    /// <param name="response">HttpResponse</param>
    /// <param name="logger">ILogger</param>
    /// <returns>RazorComponentResult</returns>
    public static RazorComponentResult RenderComponent<TComponent>(this HttpResponse response, ILogger<IRequestHandler>? logger = null)
    where TComponent : IComponent {
        var root = response.HttpContext.GetRootComponent();
        if (root is not null) {
            var parameters = new Dictionary<string, object?> {
                { nameof(IRootComponent.MainContent), typeof(TComponent) },
            };
            logger?.LogInformation("Rendering page (no model) - Root component: {root} - Content component: {content}",
                root,
                typeof(TComponent));
            return new RazorComponentResult(root, parameters);
        }
        logger?.LogInformation("Rendering partial (no model) - Content component: {content}",
                typeof(TComponent));
        return new RazorComponentResult<TComponent>();
    }

    /// <summary>
    /// Get the current route.
    /// </summary>
    /// <param name="context">HttpContext</param>
    /// <returns>String route</returns>
    public static string GetCurrentRoute(this HttpContext context) {
        return context.Request.Path.ToString().ToLower();
    }

    private static Type? GetRootComponent(this HttpContext context) {
        return context.Items[nameof(IPageRouteForAttribute)] as Type;
    }

}
