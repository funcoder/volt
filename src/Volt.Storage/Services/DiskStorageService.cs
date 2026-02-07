using System.Security.Cryptography;

namespace Volt.Storage.Services;

/// <summary>
/// A storage service that persists files to the local filesystem.
/// Files are stored under a configurable root directory using unique GUID-based keys.
/// </summary>
public sealed class DiskStorageService : IStorageService
{
    private readonly string _rootPath;

    /// <summary>
    /// Creates a new disk storage service that stores files under the given root directory.
    /// The directory is created automatically if it does not exist.
    /// </summary>
    /// <param name="rootPath">The root directory for file storage. Defaults to "./storage".</param>
    public DiskStorageService(string rootPath = "./storage")
    {
        _rootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_rootPath);
    }

    /// <inheritdoc />
    public async Task<VoltAttachment> Store(Stream stream, string filename, string contentType)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        var key = Guid.NewGuid().ToString("N");
        var extension = Path.GetExtension(filename);
        var storedFilename = $"{key}{extension}";
        var filePath = Path.Combine(_rootPath, storedFilename);

        string checksum;
        long byteSize;

        await using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
        {
            using var sha256 = SHA256.Create();
            using var hashStream = new CryptoStream(fileStream, sha256, CryptoStreamMode.Write);
            await stream.CopyToAsync(hashStream);
            await hashStream.FlushFinalBlockAsync();
            checksum = Convert.ToBase64String(sha256.Hash ?? []);
        }

        byteSize = new FileInfo(filePath).Length;

        return new VoltAttachment
        {
            Filename = filename,
            ContentType = contentType,
            ByteSize = byteSize,
            Key = storedFilename,
            ServiceName = "local",
            Checksum = checksum,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public Task<Stream> Retrieve(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var filePath = Path.Combine(_rootPath, key);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Stored file not found for key '{key}'.", filePath);
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return Task.FromResult(stream);
    }

    /// <inheritdoc />
    public Task Delete(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var filePath = Path.Combine(_rootPath, key);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public string Url(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        return $"/storage/{key}";
    }
}
