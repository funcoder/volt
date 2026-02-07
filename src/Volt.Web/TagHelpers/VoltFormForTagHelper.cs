using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Volt.Web.TagHelpers;

/// <summary>
/// Tag helper that generates model-bound forms with automatic CSRF protection.
/// Usage: <![CDATA[<volt-form-for action="/posts" method="post">...</volt-form-for>]]>
/// </summary>
[HtmlTargetElement("volt-form-for")]
public sealed class VoltFormForTagHelper : TagHelper
{
    /// <summary>
    /// The form action URL.
    /// </summary>
    [HtmlAttributeName("action")]
    public string FormAction { get; set; } = string.Empty;

    /// <summary>
    /// The HTTP method for the form submission. Defaults to "post".
    /// For PUT and DELETE, a hidden _method field is generated.
    /// </summary>
    [HtmlAttributeName("method")]
    public string Method { get; set; } = "post";

    /// <summary>
    /// Optional CSS class to apply to the generated form element.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// Whether to include HTMX attributes for partial form submission.
    /// When true, adds hx-post/hx-put and hx-swap attributes.
    /// </summary>
    [HtmlAttributeName("htmx")]
    public bool UseHtmx { get; set; }

    /// <summary>
    /// The HTMX swap strategy when <see cref="UseHtmx"/> is enabled. Defaults to "outerHTML".
    /// </summary>
    [HtmlAttributeName("hx-swap")]
    public string HxSwap { get; set; } = "outerHTML";

    /// <summary>
    /// The current view context, automatically provided by the framework.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var normalizedMethod = Method.ToUpperInvariant();
        var usesMethodOverride = normalizedMethod is "PUT" or "DELETE" or "PATCH";

        output.TagName = "form";
        output.Attributes.SetAttribute("action", FormAction);
        output.Attributes.SetAttribute("method", usesMethodOverride ? "post" : Method);

        if (!string.IsNullOrWhiteSpace(CssClass))
        {
            output.Attributes.SetAttribute("class", CssClass);
        }

        if (usesMethodOverride)
        {
            output.PreContent.AppendHtml(
                $"""<input type="hidden" name="_method" value="{normalizedMethod}" />""");
        }

        AppendAntiforgeryToken(output);

        if (UseHtmx)
        {
            var htmxVerb = usesMethodOverride ? normalizedMethod.ToLowerInvariant() : Method;
            output.Attributes.SetAttribute($"hx-{htmxVerb}", FormAction);
            output.Attributes.SetAttribute("hx-swap", HxSwap);
        }
    }

    private void AppendAntiforgeryToken(TagHelperOutput output)
    {
        var antiforgery = ViewContext.HttpContext.RequestServices.GetService<IAntiforgery>();

        if (antiforgery is null)
        {
            return;
        }

        var tokenSet = antiforgery.GetAndStoreTokens(ViewContext.HttpContext);

        if (tokenSet.RequestToken is not null)
        {
            output.PreContent.AppendHtml(
                $"""<input type="hidden" name="{tokenSet.FormFieldName}" value="{tokenSet.RequestToken}" />""");
        }
    }
}
