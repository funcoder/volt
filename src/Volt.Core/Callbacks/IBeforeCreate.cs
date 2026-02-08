namespace Volt.Core.Callbacks;

/// <summary>
/// Implement on a <see cref="Model{T}"/> to run logic before a new entity
/// is inserted into the database.
/// </summary>
public interface IBeforeCreate
{
    /// <summary>
    /// Called before the new entity is persisted. Throw to abort the save.
    /// </summary>
    Task BeforeCreateAsync(CancellationToken cancellationToken = default);
}
