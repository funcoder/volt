using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Volt.Web.StrongParams;

/// <summary>
/// Action filter that enforces strong parameters by stripping non-permitted
/// properties from the JSON request body before model binding occurs.
/// Attach to controllers or actions to restrict allowed input fields.
/// </summary>
public sealed class PermittedParamsFilter : IAsyncActionFilter
{
    private readonly HashSet<string> _permitted;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermittedParamsFilter"/> class.
    /// </summary>
    /// <param name="permittedParams">The property names that are allowed through the filter.</param>
    public PermittedParamsFilter(IEnumerable<string> permittedParams)
    {
        ArgumentNullException.ThrowIfNull(permittedParams);
        _permitted = new HashSet<string>(permittedParams, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (HasJsonContentType(context.HttpContext.Request))
        {
            await FilterJsonBody(context);
        }
        else if (HasFormContentType(context.HttpContext.Request))
        {
            FilterFormBody(context);
        }

        await next();
    }

    private async Task FilterJsonBody(ActionExecutingContext context)
    {
        context.HttpContext.Request.EnableBuffering();

        using var reader = new StreamReader(context.HttpContext.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.HttpContext.Request.Body.Position = 0;

        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var filtered = FilterJsonProperties(document.RootElement);

            var filteredBytes = JsonSerializer.SerializeToUtf8Bytes(filtered);
            context.HttpContext.Request.Body = new MemoryStream(filteredBytes);
            context.HttpContext.Request.ContentLength = filteredBytes.Length;
        }
        catch (JsonException)
        {
            context.Result = new BadRequestObjectResult(new { error = "Invalid JSON body." });
        }
    }

    private Dictionary<string, JsonElement> FilterJsonProperties(JsonElement element)
    {
        var result = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

        if (element.ValueKind != JsonValueKind.Object)
        {
            return result;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (_permitted.Contains(property.Name))
            {
                result[property.Name] = property.Value.Clone();
            }
        }

        return result;
    }

    private void FilterFormBody(ActionExecutingContext context)
    {
        var arguments = context.ActionArguments;

        foreach (var key in arguments.Keys.ToArray())
        {
            if (!_permitted.Contains(key))
            {
                arguments.Remove(key);
            }
        }
    }

    private static bool HasJsonContentType(Microsoft.AspNetCore.Http.HttpRequest request)
    {
        return request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true;
    }

    private static bool HasFormContentType(Microsoft.AspNetCore.Http.HttpRequest request)
    {
        return request.HasFormContentType;
    }
}
