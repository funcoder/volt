using Microsoft.Extensions.DependencyInjection;
using Volt.Storage.Services;

namespace Volt.Storage.Extensions;

/// <summary>
/// Extension methods for registering Volt Storage services into the dependency injection container.
/// </summary>
public static class VoltStorageExtensions
{
    /// <summary>
    /// Adds Volt Storage services to the service collection.
    /// Configures the default storage backend and registers <see cref="IStorageService"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">
    /// An optional action to configure storage options via <see cref="StorageBuilder"/>.
    /// When <c>null</c>, a local disk storage service is registered with default settings.
    /// </param>
    /// <returns>The service collection for further chaining.</returns>
    public static IServiceCollection AddVoltStorage(
        this IServiceCollection services,
        Action<StorageBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = new StorageBuilder();
        configure?.Invoke(builder);

        var options = builder.Build();
        services.AddSingleton(options);

        var defaultConfig = ResolveDefaultConfig(options);
        var storageService = CreateStorageService(defaultConfig);

        services.AddSingleton<IStorageService>(storageService);

        return services;
    }

    private static StorageServiceConfig ResolveDefaultConfig(StorageOptions options)
    {
        if (!options.Services.TryGetValue(options.DefaultService, out var config))
        {
            throw new InvalidOperationException(
                $"Storage service '{options.DefaultService}' is not configured. " +
                $"Available services: {string.Join(", ", options.Services.Keys)}.");
        }

        return config;
    }

    private static IStorageService CreateStorageService(StorageServiceConfig config)
    {
        return config.Type switch
        {
            StorageServiceType.Disk => new DiskStorageService(config.Path),
            StorageServiceType.S3 => new S3StorageService(
                config.Bucket ?? throw new InvalidOperationException("S3 bucket name is required."),
                config.Region ?? throw new InvalidOperationException("S3 region is required.")),
            _ => throw new InvalidOperationException($"Unsupported storage service type: {config.Type}.")
        };
    }
}
