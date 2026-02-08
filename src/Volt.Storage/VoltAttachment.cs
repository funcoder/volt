using System.ComponentModel.DataAnnotations.Schema;

namespace Volt.Storage;

/// <summary>
/// Represents a file attachment stored by Volt Storage.
/// Tracks the metadata needed to locate and serve the stored blob.
/// </summary>
[Table("volt_attachments")]
public class VoltAttachment
{
    [Column("id")]
    public int Id { get; init; }

    [Column("filename")]
    public string Filename { get; init; } = "";

    [Column("content_type")]
    public string ContentType { get; init; } = "";

    [Column("byte_size")]
    public long ByteSize { get; init; }

    [Column("key")]
    public string Key { get; init; } = "";

    [Column("service_name")]
    public string ServiceName { get; init; } = "local";

    [Column("checksum")]
    public string? Checksum { get; init; }

    [Column("created_at")]
    public DateTime CreatedAt { get; init; }
}
