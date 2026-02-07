using Microsoft.AspNetCore.Http;

namespace Volt.Web.Htmx;

/// <summary>
/// Extension methods for working with HTMX requests and responses.
/// Provides helpers to detect HTMX requests and set HTMX response headers.
/// </summary>
public static class HtmxExtensions
{
    private const string HxRequestHeader = "HX-Request";
    private const string HxTriggerHeader = "HX-Trigger";
    private const string HxRedirectHeader = "HX-Redirect";
    private const string HxRefreshHeader = "HX-Refresh";
    private const string HxRetargetHeader = "HX-Retarget";
    private const string HxReswapHeader = "HX-Reswap";
    private const string HxPushUrlHeader = "HX-Push-Url";

    /// <summary>
    /// Determines whether the current request was initiated by HTMX.
    /// Checks for the presence of the HX-Request header.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <returns>True if the request includes the HX-Request header.</returns>
    public static bool IsHtmxRequest(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Headers.ContainsKey(HxRequestHeader);
    }

    /// <summary>
    /// Sets the HX-Trigger response header, which triggers a client-side event.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="eventName">The name of the event to trigger on the client.</param>
    /// <returns>The response for chaining.</returns>
    public static HttpResponse HxTrigger(this HttpResponse response, string eventName)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

        response.Headers.Append(HxTriggerHeader, eventName);
        return response;
    }

    /// <summary>
    /// Sets the HX-Redirect response header, causing the client to perform
    /// a full page navigation to the specified URL.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="url">The URL to redirect to.</param>
    /// <returns>The response for chaining.</returns>
    public static HttpResponse HxRedirect(this HttpResponse response, string url)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        response.Headers.Append(HxRedirectHeader, url);
        return response;
    }

    /// <summary>
    /// Sets the HX-Refresh response header, causing the client to perform
    /// a full page refresh.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <returns>The response for chaining.</returns>
    public static HttpResponse HxRefresh(this HttpResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        response.Headers.Append(HxRefreshHeader, "true");
        return response;
    }

    /// <summary>
    /// Sets the HX-Retarget response header, overriding the target element
    /// for the HTMX swap on the client.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cssSelector">The CSS selector for the new target element.</param>
    /// <returns>The response for chaining.</returns>
    public static HttpResponse HxRetarget(this HttpResponse response, string cssSelector)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(cssSelector);

        response.Headers.Append(HxRetargetHeader, cssSelector);
        return response;
    }

    /// <summary>
    /// Sets the HX-Reswap response header, overriding the swap strategy
    /// for the HTMX response on the client.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="swapStrategy">The swap strategy (e.g., "innerHTML", "outerHTML").</param>
    /// <returns>The response for chaining.</returns>
    public static HttpResponse HxReswap(this HttpResponse response, string swapStrategy)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(swapStrategy);

        response.Headers.Append(HxReswapHeader, swapStrategy);
        return response;
    }

    /// <summary>
    /// Sets the HX-Push-Url response header, which pushes a new URL into
    /// the browser's history stack.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="url">The URL to push into browser history.</param>
    /// <returns>The response for chaining.</returns>
    public static HttpResponse HxPushUrl(this HttpResponse response, string url)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        response.Headers.Append(HxPushUrlHeader, url);
        return response;
    }
}
