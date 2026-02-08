namespace Volt.Core.Callbacks;

/// <summary>
/// Implement on a <see cref="Model{T}"/> to run logic after an existing
/// entity has been updated in the database.
/// </summary>
public interface IAfterUpdate
{
    /// <summary>
    /// Called after the modified entity has been persisted to the database.
    /// </summary>
    Task AfterUpdateAsync(CancellationToken cancellationToken = default);
}
