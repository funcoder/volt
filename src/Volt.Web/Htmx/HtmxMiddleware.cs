using Microsoft.AspNetCore.Http;

namespace Volt.Web.Htmx;

/// <summary>
/// Middleware that detects HTMX requests via the HX-Request header and adjusts
/// response behavior. When an HTMX request is detected, the response layout is
/// suppressed to return only partial HTML content.
/// </summary>
public sealed class HtmxMiddleware
{
    private const string HxRequestHeader = "HX-Request";
    private const string VoltHtmxContextKey = "Volt.IsHtmxRequest";

    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmxMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    public HtmxMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Processes the HTTP request and marks it as an HTMX request when the
    /// HX-Request header is present.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var isHtmx = context.Request.Headers.ContainsKey(HxRequestHeader);
        context.Items[VoltHtmxContextKey] = isHtmx;

        if (isHtmx)
        {
            context.Request.Headers.TryGetValue("HX-Target", out var target);
            context.Items["Volt.HxTarget"] = target.ToString();
        }

        await _next(context);
    }
}
