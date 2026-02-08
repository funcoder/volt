using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Volt.Web.Htmx;

namespace Volt.Web.Middleware;

/// <summary>
/// Extension methods that configure the complete Volt middleware pipeline.
/// Call <see cref="UseVolt"/> to set up all Volt conventions in one step.
/// </summary>
public static class VoltMiddlewareExtensions
{
    /// <summary>
    /// Configures the full Volt middleware pipeline including static files,
    /// routing, HTMX support, antiforgery, error handling, and conventional routing.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The configured web application for chaining.</returns>
    public static WebApplication UseVolt(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/error");
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        app.UseMiddleware<PendingMigrationsMiddleware>();

        app.UseStaticFiles();

        var storagePath = Path.GetFullPath("./storage");
        Directory.CreateDirectory(storagePath);
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storagePath),
            RequestPath = "/storage"
        });

        app.UseRouting();

        app.UseMiddleware<HtmxMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        app.MapControllers();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        return app;
    }
}
