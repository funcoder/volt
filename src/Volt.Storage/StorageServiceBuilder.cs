namespace Volt.Storage;

/// <summary>
/// Fluent builder for configuring an individual storage service backend.
/// Supports disk-based and S3-based storage configurations.
/// </summary>
public sealed class StorageServiceBuilder
{
    private StorageServiceType _type = StorageServiceType.Disk;
    private string _path = "./storage";
    private string? _bucket;
    private string? _region;

    /// <summary>
    /// Configures this service as a local disk storage backend.
    /// </summary>
    /// <param name="path">The directory path where files will be stored.</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public StorageServiceBuilder Disk(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _type = StorageServiceType.Disk;
        _path = path;
        return this;
    }

    /// <summary>
    /// Configures this service as an S3-based storage backend.
    /// </summary>
    /// <param name="bucket">The S3 bucket name.</param>
    /// <param name="region">The AWS region (e.g., "us-east-1").</param>
    /// <returns>This builder instance for fluent chaining.</returns>
    public StorageServiceBuilder S3(string bucket, string region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(region);

        _type = StorageServiceType.S3;
        _bucket = bucket;
        _region = region;
        return this;
    }

    /// <summary>
    /// Builds the finalized <see cref="StorageServiceConfig"/> from the current configuration.
    /// </summary>
    /// <returns>The configured service settings.</returns>
    internal StorageServiceConfig Build()
    {
        return new StorageServiceConfig
        {
            Type = _type,
            Path = _path,
            Bucket = _bucket,
            Region = _region
        };
    }
}
