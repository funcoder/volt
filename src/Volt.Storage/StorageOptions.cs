namespace Volt.Storage;

/// <summary>
/// Configuration options for Volt Storage.
/// Controls which storage services are available and which one is used by default.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>
    /// The name of the default storage service to use when none is specified.
    /// Defaults to "local".
    /// </summary>
    public string DefaultService { get; set; } = "local";

    /// <summary>
    /// A dictionary of named storage service configurations.
    /// Each key is the service name and the value holds its provider-specific settings.
    /// </summary>
    public Dictionary<string, StorageServiceConfig> Services { get; init; } = new()
    {
        ["local"] = new StorageServiceConfig { Type = StorageServiceType.Disk, Path = "./storage" }
    };
}

/// <summary>
/// Configuration for a single storage service backend.
/// </summary>
public sealed class StorageServiceConfig
{
    /// <summary>
    /// The type of storage provider (Disk or S3).
    /// </summary>
    public StorageServiceType Type { get; init; } = StorageServiceType.Disk;

    /// <summary>
    /// The local directory path for disk-based storage.
    /// Only used when <see cref="Type"/> is <see cref="StorageServiceType.Disk"/>.
    /// </summary>
    public string Path { get; init; } = "./storage";

    /// <summary>
    /// The S3 bucket name for S3-based storage.
    /// Only used when <see cref="Type"/> is <see cref="StorageServiceType.S3"/>.
    /// </summary>
    public string? Bucket { get; init; }

    /// <summary>
    /// The AWS region for S3-based storage (e.g., "us-east-1").
    /// Only used when <see cref="Type"/> is <see cref="StorageServiceType.S3"/>.
    /// </summary>
    public string? Region { get; init; }
}

/// <summary>
/// The type of storage backend provider.
/// </summary>
public enum StorageServiceType
{
    /// <summary>
    /// Local disk storage.
    /// </summary>
    Disk,

    /// <summary>
    /// Amazon S3 or S3-compatible object storage.
    /// </summary>
    S3
}
