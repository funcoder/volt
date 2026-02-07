namespace Volt.Storage;

/// <summary>
/// Represents a file attachment stored by Volt Storage.
/// Tracks the metadata needed to locate and serve the stored blob.
/// </summary>
public class VoltAttachment
{
    /// <summary>
    /// The primary key identifier for this attachment.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The original filename as provided during upload.
    /// </summary>
    public string Filename { get; init; } = "";

    /// <summary>
    /// The MIME content type of the stored file (e.g., "image/png").
    /// </summary>
    public string ContentType { get; init; } = "";

    /// <summary>
    /// The size of the stored file in bytes.
    /// </summary>
    public long ByteSize { get; init; }

    /// <summary>
    /// The unique storage key used to locate the blob in the storage backend.
    /// </summary>
    public string Key { get; init; } = "";

    /// <summary>
    /// The name of the storage service that holds this blob (e.g., "local", "s3").
    /// Defaults to "local".
    /// </summary>
    public string ServiceName { get; init; } = "local";

    /// <summary>
    /// An optional checksum (e.g., MD5 or SHA256) of the file contents for integrity verification.
    /// </summary>
    public string? Checksum { get; init; }

    /// <summary>
    /// The timestamp when this attachment record was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
