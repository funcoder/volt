namespace Volt.Core.Callbacks;

/// <summary>
/// Implement on a <see cref="Model{T}"/> to run logic after an entity
/// has been deleted from the database (hard or soft delete).
/// </summary>
public interface IAfterDestroy
{
    /// <summary>
    /// Called after the entity has been deleted from the database.
    /// </summary>
    Task AfterDestroyAsync(CancellationToken cancellationToken = default);
}
