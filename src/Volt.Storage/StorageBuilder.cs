namespace Volt.Storage;

/// <summary>
/// Fluent builder for configuring Volt Storage services.
/// Allows registering named storage backends and selecting the default service.
/// </summary>
public sealed class StorageBuilder
{
    private readonly StorageOptions _options = new();

    /// <summary>
    /// Registers a named storage service using the provided configuration action.
    /// </summary>
    /// <param name="name">The unique name for this storage service.</param>
    /// <param name="configure">An action that configures the service via a <see cref="StorageServiceBuilder"/>.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public StorageBuilder Service(string name, Action<StorageServiceBuilder> configure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(configure);

        var serviceBuilder = new StorageServiceBuilder();
        configure(serviceBuilder);

        _options.Services[name] = serviceBuilder.Build();
        return this;
    }

    /// <summary>
    /// Sets the name of the default storage service.
    /// This service is used when no explicit service is specified for an attachment.
    /// </summary>
    /// <param name="serviceName">The name of a previously registered service.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public StorageBuilder Default(string serviceName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        _options.DefaultService = serviceName;
        return this;
    }

    /// <summary>
    /// Builds the finalized <see cref="StorageOptions"/> from the current configuration.
    /// </summary>
    /// <returns>The configured storage options.</returns>
    internal StorageOptions Build()
    {
        return _options;
    }
}
