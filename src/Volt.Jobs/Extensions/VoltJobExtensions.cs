using System.Reflection;
using Coravel;
using Coravel.Invocable;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Volt.Jobs.Extensions;

/// <summary>
/// Extension methods for registering and configuring Volt background jobs
/// in the ASP.NET Core dependency injection container and middleware pipeline.
/// </summary>
public static class VoltJobExtensions
{
    /// <summary>
    /// Registers Coravel's queue and scheduler services, auto-discovers all
    /// <see cref="VoltJob{TPayload}"/> and <see cref="VoltJobWithoutPayload"/>
    /// subclasses from the calling assembly, and registers them as transient services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoltJobs(this IServiceCollection services)
    {
        return AddVoltJobs(services, Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Registers Coravel's queue and scheduler services, auto-discovers all
    /// <see cref="VoltJob{TPayload}"/> and <see cref="VoltJobWithoutPayload"/>
    /// subclasses from the specified assembly, and registers them as transient services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="assembly">The assembly to scan for job types.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoltJobs(this IServiceCollection services, Assembly assembly)
    {
        services.AddQueue();
        services.AddScheduler();

        var jobTypes = DiscoverJobTypes(assembly);

        foreach (var jobType in jobTypes)
        {
            services.AddTransient(jobType);
        }

        services.AddSingleton<IJobQueue, VoltJobQueue>();

        return services;
    }

    /// <summary>
    /// Configures the Coravel scheduler for the application.
    /// Call this in the middleware pipeline after building the <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseVoltJobs(this WebApplication app)
    {
        app.Services.UseScheduler(scheduler => { });
        return app;
    }

    /// <summary>
    /// Configures the Coravel scheduler with a custom configuration action.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <param name="configure">An action to configure the scheduler.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseVoltJobs(
        this WebApplication app,
        Action<Scheduling.VoltSchedulerConfig> configure)
    {
        app.Services.UseScheduler(scheduler =>
        {
            var config = new Scheduling.VoltSchedulerConfig(scheduler);
            configure(config);
        });
        return app;
    }

    private static IEnumerable<Type> DiscoverJobTypes(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsClass: true }
                && IsVoltJobType(type));
    }

    private static bool IsVoltJobType(Type type)
    {
        if (type.BaseType is null)
        {
            return false;
        }

        if (type.BaseType == typeof(VoltJobWithoutPayload))
        {
            return true;
        }

        if (type.BaseType.IsGenericType
            && type.BaseType.GetGenericTypeDefinition() == typeof(VoltJob<>))
        {
            return true;
        }

        return false;
    }
}
