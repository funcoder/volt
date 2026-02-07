using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Volt.Web.TagHelpers;

/// <summary>
/// Tag helper that generates anchor links using Volt conventions.
/// Usage: <![CDATA[<volt-link-to controller="Posts" action="Show" id="1">View Post</volt-link-to>]]>
/// </summary>
[HtmlTargetElement("volt-link-to")]
public sealed class VoltLinkToTagHelper : TagHelper
{
    private readonly IUrlHelperFactory _urlHelperFactory;

    /// <summary>
    /// The controller name for the link target.
    /// </summary>
    [HtmlAttributeName("controller")]
    public string? TargetController { get; set; }

    /// <summary>
    /// The action name for the link target.
    /// </summary>
    [HtmlAttributeName("action")]
    public string TargetAction { get; set; } = "Index";

    /// <summary>
    /// The optional entity id for the link target.
    /// </summary>
    [HtmlAttributeName("id")]
    public int? EntityId { get; set; }

    /// <summary>
    /// Optional CSS class to apply to the generated anchor element.
    /// </summary>
    [HtmlAttributeName("class")]
    public string? CssClass { get; set; }

    /// <summary>
    /// The current view context, automatically provided by the framework.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="VoltLinkToTagHelper"/> class.
    /// </summary>
    /// <param name="urlHelperFactory">The URL helper factory for generating URLs.</param>
    public VoltLinkToTagHelper(IUrlHelperFactory urlHelperFactory)
    {
        _urlHelperFactory = urlHelperFactory;
    }

    /// <inheritdoc />
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(output);

        var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);

        var routeValues = EntityId.HasValue
            ? new { id = EntityId.Value }
            : (object?)null;

        var url = urlHelper.Action(TargetAction, TargetController, routeValues) ?? "#";

        output.TagName = "a";
        output.Attributes.SetAttribute("href", url);

        if (!string.IsNullOrWhiteSpace(CssClass))
        {
            output.Attributes.SetAttribute("class", CssClass);
        }
    }
}
