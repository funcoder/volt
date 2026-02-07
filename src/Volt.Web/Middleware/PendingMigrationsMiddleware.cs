using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Volt.Data;

namespace Volt.Web.Middleware;

/// <summary>
/// Development-only middleware that intercepts database exceptions caused by
/// pending migrations and renders a friendly page with instructions to apply them.
/// Similar to Rails' "Migrations are pending" page.
/// </summary>
public sealed class PendingMigrationsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public PendingMigrationsMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_environment.IsDevelopment())
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex) when (IsDatabaseException(ex))
        {
            var pending = await GetPendingMigrations(context.RequestServices);

            if (pending.Count > 0)
            {
                await WritePendingMigrationsPage(context, pending);
                return;
            }

            throw;
        }
    }

    private static bool IsDatabaseException(Exception ex)
    {
        return ContainsDatabaseError(ex) || (ex.InnerException is not null && ContainsDatabaseError(ex.InnerException));
    }

    private static bool ContainsDatabaseError(Exception ex)
    {
        var message = ex.Message;
        var typeName = ex.GetType().Name;

        return typeName.Contains("Sqlite", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Postgres", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("SqlException", StringComparison.OrdinalIgnoreCase)
            || message.Contains("no such table", StringComparison.OrdinalIgnoreCase)
            || message.Contains("does not exist", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Invalid object name", StringComparison.OrdinalIgnoreCase)
            || message.Contains("relation", StringComparison.OrdinalIgnoreCase)
            || message.Contains("not included in the model", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<IReadOnlyList<string>> GetPendingMigrations(IServiceProvider services)
    {
        try
        {
            var dbContext = services.GetService<VoltDbContext>();
            if (dbContext is null) return [];

            var pending = await dbContext.Database.GetPendingMigrationsAsync();
            return pending.ToList();
        }
        catch
        {
            return [];
        }
    }

    private static async Task WritePendingMigrationsPage(
        HttpContext context, IReadOnlyList<string> pending)
    {
        context.Response.StatusCode = 503;
        context.Response.ContentType = "text/html; charset=utf-8";

        var html = PendingMigrationsPage.Render(pending);
        await context.Response.WriteAsync(html);
    }
}
