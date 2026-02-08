namespace Volt.Core.Callbacks;

/// <summary>
/// Implement on a <see cref="Model{T}"/> to run logic after a new entity
/// has been inserted into the database.
/// </summary>
public interface IAfterCreate
{
    /// <summary>
    /// Called after the new entity has been persisted to the database.
    /// </summary>
    Task AfterCreateAsync(CancellationToken cancellationToken = default);
}
