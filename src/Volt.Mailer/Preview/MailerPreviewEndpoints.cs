using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Volt.Mailer.Preview;

/// <summary>
/// Minimal API endpoints for previewing mailer templates in development.
/// Mounted at /volt/mailers to provide a browsable index of all mailers
/// and their methods, with rendered HTML previews.
/// </summary>
public static class MailerPreviewEndpoints
{
    /// <summary>
    /// Maps the mailer preview routes onto the given endpoint route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder (typically a <see cref="WebApplication"/>).</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/volt/mailers", HandleIndex)
            .ExcludeFromDescription();

        app.MapGet("/volt/mailers/{mailer}/{method}", HandlePreview)
            .ExcludeFromDescription();
    }

    /// <summary>
    /// Renders an HTML index page listing all discovered mailers and their public methods.
    /// </summary>
    private static IResult HandleIndex(HttpContext context)
    {
        var mailers = DiscoverMailers(context);
        var html = BuildIndexHtml(mailers);
        return Results.Content(html, "text/html");
    }

    /// <summary>
    /// Renders a preview of a specific mailer method's template.
    /// </summary>
    private static async Task<IResult> HandlePreview(
        HttpContext context,
        string mailer,
        string method)
    {
        var mailerType = DiscoverMailers(context)
            .FirstOrDefault(t => string.Equals(
                t.Name, mailer, StringComparison.OrdinalIgnoreCase));

        if (mailerType is null)
        {
            return Results.NotFound($"Mailer '{mailer}' not found.");
        }

        var methodInfo = mailerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .FirstOrDefault(m => string.Equals(
                m.Name, method, StringComparison.OrdinalIgnoreCase));

        if (methodInfo is null)
        {
            return Results.NotFound($"Method '{method}' not found on mailer '{mailer}'.");
        }

        var templatePath = Path.Combine(
            "Views", "Mailers", mailerType.Name, $"{methodInfo.Name}.cshtml");

        if (!File.Exists(templatePath))
        {
            return Results.NotFound($"Template not found at: {templatePath}");
        }

        var templateContent = await File.ReadAllTextAsync(templatePath);
        var previewHtml = BuildPreviewHtml(mailerType.Name, methodInfo.Name, templateContent);

        return Results.Content(previewHtml, "text/html");
    }

    /// <summary>
    /// Discovers all non-abstract <see cref="VoltMailer"/> subclasses registered in the
    /// application's service provider.
    /// </summary>
    private static IReadOnlyList<Type> DiscoverMailers(HttpContext context)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly is null)
        {
            return [];
        }

        return entryAssembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true }
                        && t.IsSubclassOf(typeof(VoltMailer)))
            .OrderBy(t => t.Name)
            .ToList();
    }

    private static string BuildIndexHtml(IReadOnlyList<Type> mailers)
    {
        var sb = new StringBuilder();
        sb.Append("<!DOCTYPE html><html><head><title>Volt Mailer Previews</title>");
        sb.Append("<style>body{font-family:system-ui,sans-serif;max-width:800px;margin:2rem auto;padding:0 1rem}");
        sb.Append("h1{color:#333}h2{color:#555;margin-top:2rem}a{color:#06c;text-decoration:none}");
        sb.Append("a:hover{text-decoration:underline}ul{list-style:none;padding:0}li{padding:.3rem 0}</style>");
        sb.Append("</head><body><h1>Volt Mailer Previews</h1>");

        if (mailers.Count == 0)
            sb.Append("<p>No mailers found. Create a class that extends <code>VoltMailer</code>.</p>");

        foreach (var mailer in mailers)
        {
            sb.Append($"<h2>{mailer.Name}</h2><ul>");
            var methods = mailer
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .OrderBy(m => m.Name);
            foreach (var m in methods)
                sb.Append($"<li><a href=\"/volt/mailers/{mailer.Name}/{m.Name}\">{m.Name}</a></li>");
            sb.Append("</ul>");
        }

        sb.Append("</body></html>");
        return sb.ToString();
    }

    private static string BuildPreviewHtml(string mailerName, string methodName, string template)
    {
        var encoded = System.Net.WebUtility.HtmlEncode(template);
        return $$"""
            <!DOCTYPE html><html><head><title>Preview: {{mailerName}}.{{methodName}}</title>
            <style>body{font-family:system-ui,sans-serif;margin:0}
            .toolbar{background:#333;color:#fff;padding:.5rem 1rem;display:flex;align-items:center;gap:1rem}
            .toolbar a{color:#adf}pre{background:#f5f5f5;padding:1rem;margin:1rem;overflow-x:auto;border-radius:4px}</style>
            </head><body>
            <div class="toolbar"><a href="/volt/mailers">&larr; All Mailers</a>
            <strong>{{mailerName}}.{{methodName}}</strong></div>
            <h3 style="margin:1rem">Raw Template</h3>
            <pre>{{encoded}}</pre></body></html>
            """;
    }
}
