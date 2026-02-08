namespace Volt.Core.Callbacks;

/// <summary>
/// Implement on a <see cref="Model{T}"/> to run logic before the entity is
/// saved (on both create and update).
/// </summary>
public interface IBeforeSave
{
    /// <summary>
    /// Called before the entity is persisted to the database.
    /// Throw to abort the save.
    /// </summary>
    Task BeforeSaveAsync(CancellationToken cancellationToken = default);
}
