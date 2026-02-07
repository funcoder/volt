namespace Volt.Data.Seeding;

/// <summary>
/// Base class for Volt seed data providers. Subclasses override
/// <see cref="SeedAsync"/> to insert or upsert seed records.
/// </summary>
public abstract class VoltSeeder : IVoltSeeder
{
    /// <inheritdoc />
    public abstract Task SeedAsync(VoltDbContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seeds the database synchronously. Override this for simple seed
    /// scenarios; the default implementation calls <see cref="SeedAsync"/>.
    /// </summary>
    /// <param name="context">The database context to seed.</param>
    public virtual void Seed(VoltDbContext context)
    {
        SeedAsync(context, CancellationToken.None).GetAwaiter().GetResult();
    }
}
