namespace Volt.Core.Callbacks;

/// <summary>
/// Implement on a <see cref="Model{T}"/> to run logic before an entity
/// is deleted from the database (hard or soft delete).
/// </summary>
public interface IBeforeDestroy
{
    /// <summary>
    /// Called before the entity is deleted. Throw to abort the save.
    /// </summary>
    Task BeforeDestroyAsync(CancellationToken cancellationToken = default);
}
