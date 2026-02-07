using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Volt.RealTime.Extensions;

/// <summary>
/// Extension methods for registering and configuring Volt real-time channels
/// in the ASP.NET Core dependency injection container and endpoint routing.
/// </summary>
public static class VoltRealTimeExtensions
{
    private static readonly Type MapHubMethod = typeof(HubEndpointRouteBuilderExtensions);

    /// <summary>
    /// Registers SignalR services, the channel broadcaster, and configures
    /// hub options from <see cref="VoltChannelOptions"/>.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configure">Optional action to customize channel options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoltRealTime(
        this IServiceCollection services,
        Action<VoltChannelOptions>? configure = null)
    {
        var options = new VoltChannelOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);

        services.AddSignalR(hubOptions =>
        {
            hubOptions.EnableDetailedErrors = options.EnableDetailedErrors;
            hubOptions.KeepAliveInterval = options.KeepAliveInterval;
            hubOptions.ClientTimeoutInterval = options.ClientTimeoutInterval;
        });

        services.AddSingleton<IChannelBroadcaster, VoltChannelBroadcaster>();

        return services;
    }

    /// <summary>
    /// Discovers all <see cref="VoltChannel"/> subclasses from the calling assembly
    /// and maps them to SignalR hub endpoints by convention.
    /// Convention: ChatChannel maps to /volt/channels/chat.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapVoltChannels(this WebApplication app)
    {
        return MapVoltChannels(app, Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Discovers all <see cref="VoltChannel"/> subclasses from the specified assembly
    /// and maps them to SignalR hub endpoints by convention.
    /// </summary>
    /// <param name="app">The web application to configure.</param>
    /// <param name="assembly">The assembly to scan for channel types.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapVoltChannels(this WebApplication app, Assembly assembly)
    {
        var options = app.Services.GetRequiredService<VoltChannelOptions>();
        var channelTypes = DiscoverChannelTypes(assembly);

        foreach (var channelType in channelTypes)
        {
            var route = BuildRoute(options.BasePath, channelType);
            MapHub(app, channelType, route);
        }

        return app;
    }

    private static IEnumerable<Type> DiscoverChannelTypes(Assembly assembly)
    {
        return assembly
            .GetTypes()
            .Where(type =>
                type is { IsAbstract: false, IsClass: true }
                && type.IsSubclassOf(typeof(VoltChannel)));
    }

    private static string BuildRoute(string basePath, Type channelType)
    {
        var name = channelType.Name;

        if (name.EndsWith("Channel", StringComparison.Ordinal))
        {
            name = name[..^"Channel".Length];
        }

        return $"{basePath.TrimEnd('/')}/{name.ToLowerInvariant()}";
    }

    private static void MapHub(IEndpointRouteBuilder endpoints, Type channelType, string route)
    {
        var method = MapHubMethod
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m =>
                m.Name == "MapHub"
                && m.IsGenericMethod
                && m.GetParameters().Length == 2)
            .MakeGenericMethod(channelType);

        method.Invoke(null, [endpoints, route]);
    }
}
