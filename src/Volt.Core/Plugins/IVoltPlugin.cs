using Microsoft.Extensions.DependencyInjection;

namespace Volt.Core.Plugins;

/// <summary>
/// Interface for Volt framework plugins that hook into the application pipeline.
/// Plugins can register services and configure middleware during startup.
/// </summary>
public interface IVoltPlugin
{
    /// <summary>
    /// The unique name of this plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The version of this plugin.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Registers services required by this plugin into the DI container.
    /// Called during application startup before the pipeline is built.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    void ConfigureServices(IServiceCollection services);
}
