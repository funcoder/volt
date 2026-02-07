namespace Volt.Storage;

/// <summary>
/// Defines the contract for a storage backend that can store, retrieve, and delete blobs.
/// Implementations may target local disk, S3, Azure Blob Storage, or other providers.
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Stores a file from the given stream and returns the resulting attachment metadata.
    /// </summary>
    /// <param name="stream">The readable stream containing the file data.</param>
    /// <param name="filename">The original filename (e.g., "photo.jpg").</param>
    /// <param name="contentType">The MIME content type (e.g., "image/jpeg").</param>
    /// <returns>A <see cref="VoltAttachment"/> describing the stored file.</returns>
    Task<VoltAttachment> Store(Stream stream, string filename, string contentType);

    /// <summary>
    /// Retrieves the file data for the given storage key.
    /// </summary>
    /// <param name="key">The unique storage key returned when the file was stored.</param>
    /// <returns>A readable stream of the file contents.</returns>
    Task<Stream> Retrieve(string key);

    /// <summary>
    /// Deletes the file associated with the given storage key.
    /// </summary>
    /// <param name="key">The unique storage key of the file to delete.</param>
    Task Delete(string key);

    /// <summary>
    /// Returns a URL that can be used to access the file at the given storage key.
    /// </summary>
    /// <param name="key">The unique storage key of the file.</param>
    /// <returns>A URL string for accessing the file.</returns>
    string Url(string key);
}
