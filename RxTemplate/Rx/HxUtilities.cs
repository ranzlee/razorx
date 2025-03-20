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
    /// Adds the HX-Retarget response header.
    /// </summary>
    /// <param name="response">HttpResponse</param>
    /// <param name="targetSelector">The ID of the HTML element to re-target for swapping.</param>
    /// <param name="logger">ILogger</param>
    public static void HxRetarget(this HttpResponse response, string targetSelector, ILogger? logger = default) {
        logger?.LogInformation("HX-Retarget {target}.", targetSelector);
        response.Headers.Append("HX-Retarget", targetSelector);
    }

    public static void HxReswap(this HttpResponse response, string strategy, ILogger? logger = default) {
        logger?.LogInformation("HX-Reswap {strategy}.", strategy);
        response.Headers.Append("HX-Reswap", strategy);
    }

    public static void HxReplaceUrl(this HttpResponse response, string url, ILogger? logger = default) {
        logger?.LogInformation("HX-Replace-Url {url}.", url);
        response.Headers.Append("HX-Replace-Url", url);
    }
}