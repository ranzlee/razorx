using Microsoft.Extensions.Primitives;

namespace RxTemplate.Rx;

public static class HxUtilities {

    /// <summary>
    /// Looks for the HX-Request request header.
    /// </summary>
    /// <param name="request">HttpRequest</param>
    /// <returns>True if the header exists.</returns>
    public static bool IsHxRequest(this HttpRequest request) {
        return request.Headers.ContainsKey("hx-request");
    }

    /// <summary>
    /// Looks for the HX-Boosted request header.
    /// </summary>
    /// <param name="request">HttpRequest</param>
    /// <returns>True if the header exists.</returns>
    public static bool IsHxBoosted(this HttpRequest request) {
        return request.Headers.ContainsKey("hx-boosted");
    }

    /// <summary>
    /// Gets the HX-Target send in the request
    /// </summary>
    /// <param name="request">HttpRequest</param>
    /// <returns>The value of the HX-Target header, if it exists, else an empty string.</returns>
    public static string HxTarget(this HttpRequest request) {
        if (!request.Headers.TryGetValue("hx-target", out StringValues value)) {
            return string.Empty;
        }
        return value.ToString();
    }

    /// <summary>
    /// Adds the HX-Retarget response header.
    /// </summary>
    /// <param name="response">HttpResponse</param>
    /// <param name="targetSelector">The ID of the HTML element to re-target for swapping.</param>
    /// <param name="logger">ILogger</param>
    public static void HxRetarget(this HttpResponse response, string targetSelector, ILogger? logger = default) {
        logger?.LogInformation("HX-Retarget {target}.", targetSelector);
        response.Headers.Append("HX-Retarget", targetSelector);
    }

    /// <summary>
    /// Adds the HX-Reswap response header.
    /// </summary>
    /// <param name="response">HttpResponse</param>
    /// <param name="strategy">The htmx swap strategy to use.</param>
    /// <param name="logger">ILogger</param>
    public static void HxReswap(this HttpResponse response, string strategy, ILogger? logger = default) {
        logger?.LogInformation("HX-Reswap {strategy}.", strategy);
        response.Headers.Append("HX-Reswap", strategy);
    }

    /// <summary>
    /// Adds the HX-Replace-Url response header
    /// </summary>
    /// <param name="response">HttpResponse</param>
    /// <param name="url">The URL to set on the client.</param>
    /// <param name="logger">ILogger</param>
    public static void HxReplaceUrl(this HttpResponse response, string url, ILogger? logger = default) {
        logger?.LogInformation("HX-Replace-Url {url}.", url);
        response.Headers.Append("HX-Replace-Url", url);
    }

    /// <summary>
    /// Adds the HX-Redirect response header
    /// </summary>
    /// <param name="response">HttpResponse</param>
    /// <param name="url">The URL to issue a client redirect to.</param>
    /// <param name="logger">ILogger</param>
    public static void HxRedirect(this HttpResponse response, string url, ILogger? logger = default) {
        logger?.LogInformation("HX-Redirect {url}.", url);
        response.Headers.Append("HX-Redirect", url);
    }
}