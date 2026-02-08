namespace Volt.Core.Callbacks;

/// <summary>
/// Implement on a <see cref="Model{T}"/> to run logic before an existing
/// entity is updated in the database.
/// </summary>
public interface IBeforeUpdate
{
    /// <summary>
    /// Called before the modified entity is persisted. Throw to abort the save.
    /// </summary>
    Task BeforeUpdateAsync(CancellationToken cancellationToken = default);
}
