namespace Volt.Core.Attributes;

/// <summary>
/// Marks a property as having multiple file attachments.
/// The framework will configure storage and association automatically.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class HasManyAttachedAttribute : Attribute
{
    /// <summary>
    /// The storage service name to use for these attachments.
    /// Defaults to the application's configured default service.
    /// </summary>
    public string? ServiceName { get; init; }

    /// <summary>
    /// The content types allowed for these attachments (e.g., "image/png", "image/jpeg").
    /// When empty, all content types are accepted.
    /// </summary>
    public string[] AllowedContentTypes { get; init; } = [];

    /// <summary>
    /// The maximum file size in bytes per individual file. Zero means no limit.
    /// </summary>
    public long MaxSizeBytes { get; init; }

    /// <summary>
    /// The maximum number of attachments allowed. Zero means no limit.
    /// </summary>
    public int MaxCount { get; init; }
}
