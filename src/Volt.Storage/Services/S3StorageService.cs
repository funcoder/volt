namespace Volt.Storage.Services;

/// <summary>
/// A storage service stub for Amazon S3 or S3-compatible object storage.
/// Full implementation requires the Volt.Storage.S3 package.
/// </summary>
public sealed class S3StorageService : IStorageService
{
    private readonly string _bucket;
    private readonly string _region;

    /// <summary>
    /// Creates a new S3 storage service targeting the specified bucket and region.
    /// </summary>
    /// <param name="bucket">The S3 bucket name.</param>
    /// <param name="region">The AWS region (e.g., "us-east-1").</param>
    public S3StorageService(string bucket, string region)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bucket);
        ArgumentException.ThrowIfNullOrWhiteSpace(region);

        _bucket = bucket;
        _region = region;
    }

    /// <summary>
    /// The S3 bucket name this service is configured for.
    /// </summary>
    public string Bucket => _bucket;

    /// <summary>
    /// The AWS region this service is configured for.
    /// </summary>
    public string Region => _region;

    /// <inheritdoc />
    public Task<VoltAttachment> Store(Stream stream, string filename, string contentType)
    {
        throw new NotImplementedException("Install Volt.Storage.S3 package for S3 support");
    }

    /// <inheritdoc />
    public Task<Stream> Retrieve(string key)
    {
        throw new NotImplementedException("Install Volt.Storage.S3 package for S3 support");
    }

    /// <inheritdoc />
    public Task Delete(string key)
    {
        throw new NotImplementedException("Install Volt.Storage.S3 package for S3 support");
    }

    /// <inheritdoc />
    public string Url(string key)
    {
        throw new NotImplementedException("Install Volt.Storage.S3 package for S3 support");
    }
}
