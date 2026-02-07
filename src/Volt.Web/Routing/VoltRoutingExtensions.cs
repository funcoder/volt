using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Volt.Core.Conventions;

namespace Volt.Web.Routing;

/// <summary>
/// Extension methods for configuring Volt's conventional RESTful routing.
/// Maps Rails-like resource routes to controller actions.
/// </summary>
public static class VoltRoutingExtensions
{
    /// <summary>
    /// Maps conventional RESTful routes for the specified resource controller.
    /// Generates Index, Show, New, Create, Edit, Update, and Destroy routes.
    /// </summary>
    /// <typeparam name="TController">The controller type to map routes for.</typeparam>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="prefix">An optional route prefix. Defaults to the pluralized controller name.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapVoltResources<TController>(
        this IEndpointRouteBuilder endpoints,
        string? prefix = null) where TController : class
    {
        var resourceName = ResolveResourceName<TController>(prefix);

        endpoints.MapControllerRoute(
            name: $"{resourceName}_index",
            pattern: resourceName,
            defaults: new { controller = ControllerName<TController>(), action = "Index" });

        endpoints.MapControllerRoute(
            name: $"{resourceName}_new",
            pattern: $"{resourceName}/new",
            defaults: new { controller = ControllerName<TController>(), action = "New" });

        endpoints.MapControllerRoute(
            name: $"{resourceName}_create",
            pattern: resourceName,
            defaults: new { controller = ControllerName<TController>(), action = "Create" });

        endpoints.MapControllerRoute(
            name: $"{resourceName}_show",
            pattern: $"{resourceName}/{{id:int}}",
            defaults: new { controller = ControllerName<TController>(), action = "Show" });

        endpoints.MapControllerRoute(
            name: $"{resourceName}_edit",
            pattern: $"{resourceName}/{{id:int}}/edit",
            defaults: new { controller = ControllerName<TController>(), action = "Edit" });

        endpoints.MapControllerRoute(
            name: $"{resourceName}_update",
            pattern: $"{resourceName}/{{id:int}}",
            defaults: new { controller = ControllerName<TController>(), action = "Update" });

        endpoints.MapControllerRoute(
            name: $"{resourceName}_destroy",
            pattern: $"{resourceName}/{{id:int}}",
            defaults: new { controller = ControllerName<TController>(), action = "Destroy" });

        return endpoints;
    }

    /// <summary>
    /// Configures conventional Volt routing on the application pipeline.
    /// Sets up default route patterns and enables attribute routing.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseVoltRouting(this IApplicationBuilder app)
    {
        app.UseRouting();

        return app;
    }

    private static string ControllerName<TController>()
    {
        var name = typeof(TController).Name;

        if (name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase))
        {
            return name[..^"Controller".Length];
        }

        return name;
    }

    private static string ResolveResourceName<TController>(string? prefix)
    {
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            return prefix.TrimStart('/').TrimEnd('/');
        }

        var controllerName = ControllerName<TController>();
        return VoltConventions.Pluralize(VoltConventions.ToSnakeCase(controllerName));
    }
}
