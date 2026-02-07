namespace Volt.Core.Attributes;

/// <summary>
/// Marks a property as having a single file attachment.
/// The framework will configure storage and association automatically.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class HasOneAttachedAttribute : Attribute
{
    /// <summary>
    /// The storage service name to use for this attachment.
    /// Defaults to the application's configured default service.
    /// </summary>
    public string? ServiceName { get; init; }

    /// <summary>
    /// The content types allowed for this attachment (e.g., "image/png", "image/jpeg").
    /// When empty, all content types are accepted.
    /// </summary>
    public string[] AllowedContentTypes { get; init; } = [];

    /// <summary>
    /// The maximum file size in bytes. Zero means no limit.
    /// </summary>
    public long MaxSizeBytes { get; init; }
}
