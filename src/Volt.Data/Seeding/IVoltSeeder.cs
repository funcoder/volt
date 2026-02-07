namespace Volt.Data.Seeding;

/// <summary>
/// Contract for classes that provide seed data to a <see cref="VoltDbContext"/>.
/// Implement this interface once per logical data set (e.g. <c>UserSeeder</c>,
/// <c>ProductSeeder</c>) and register it via <c>AddVoltDb</c>.
/// </summary>
public interface IVoltSeeder
{
    /// <summary>
    /// Seeds the database with initial data.
    /// Implementations should be idempotent — safe to call more than once.
    /// </summary>
    /// <param name="context">The database context to seed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous seed operation.</returns>
    Task SeedAsync(VoltDbContext context, CancellationToken cancellationToken = default);
}
