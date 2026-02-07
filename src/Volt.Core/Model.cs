namespace Volt.Core;

/// <summary>
/// Base class for all Volt models. Provides common properties for identity,
/// timestamps, and soft-delete tracking.
/// </summary>
/// <typeparam name="T">The concrete model type (CRTP pattern).</typeparam>
public abstract class Model<T> where T : Model<T>
{
    /// <summary>
    /// The primary key identifier for the model.
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// The timestamp when this record was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The timestamp when this record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The timestamp when this record was soft-deleted, or null if active.
    /// </summary>
    public DateTime? DeletedAt { get; init; }
}
