namespace Volt.Core.Callbacks;

/// <summary>
/// Implement on a <see cref="Model{T}"/> to run logic after the entity is
/// saved (on both create and update).
/// </summary>
public interface IAfterSave
{
    /// <summary>
    /// Called after the entity has been persisted to the database.
    /// </summary>
    Task AfterSaveAsync(CancellationToken cancellationToken = default);
}
