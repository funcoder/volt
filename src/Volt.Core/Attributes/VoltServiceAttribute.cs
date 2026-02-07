using Microsoft.Extensions.DependencyInjection;

namespace Volt.Core.Attributes;

/// <summary>
/// Marks a class for automatic registration in the dependency injection container.
/// The framework will discover and register annotated services at startup.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class VoltServiceAttribute : Attribute
{
    /// <summary>
    /// The DI service lifetime for this service. Defaults to <see cref="ServiceLifetime.Scoped"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// Creates a new Volt service registration marker.
    /// </summary>
    /// <param name="lifetime">The DI lifetime for this service.</param>
    public VoltServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        Lifetime = lifetime;
    }
}
