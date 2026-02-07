using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Volt.Data;

namespace Volt.Testing.Database;

/// <summary>
/// Creates in-memory <see cref="VoltDbContext"/> instances for unit tests.
/// Each call to <see cref="Create"/> or <see cref="CreateWithData"/> returns
/// a fresh database to ensure test isolation.
/// </summary>
/// <typeparam name="TContext">The <see cref="VoltDbContext"/> subclass to create.</typeparam>
public sealed class TestDbContextFactory<TContext> where TContext : VoltDbContext
{
    private readonly Func<DbContextOptions<TContext>, IOptions<VoltDbOptions>, TContext> _activator;

    /// <summary>
    /// Initializes a new factory with a constructor delegate for the context type.
    /// </summary>
    /// <param name="activator">
    /// A function that creates a new <typeparamref name="TContext"/> given EF Core options and Volt options.
    /// </param>
    public TestDbContextFactory(
        Func<DbContextOptions<TContext>, IOptions<VoltDbOptions>, TContext> activator)
    {
        ArgumentNullException.ThrowIfNull(activator);
        _activator = activator;
    }

    /// <summary>
    /// Creates a new in-memory <typeparamref name="TContext"/> with an isolated database.
    /// </summary>
    /// <returns>A fresh database context backed by an in-memory database.</returns>
    public TContext Create()
    {
        var (dbOptions, voltOptions) = BuildOptions();
        var context = _activator(dbOptions, voltOptions);
        context.Database.EnsureCreated();
        return context;
    }

    /// <summary>
    /// Creates a new in-memory <typeparamref name="TContext"/> and seeds it with test data.
    /// </summary>
    /// <param name="seed">An action that adds seed data to the context.</param>
    /// <returns>A fresh database context with the seeded data.</returns>
    public TContext CreateWithData(Action<TContext> seed)
    {
        ArgumentNullException.ThrowIfNull(seed);

        var context = Create();
        seed(context);
        context.SaveChanges();
        return context;
    }

    private static (DbContextOptions<TContext> DbOptions, IOptions<VoltDbOptions> VoltOptions) BuildOptions()
    {
        var databaseName = $"VoltTest_{Guid.NewGuid():N}";

        var dbOptions = new DbContextOptionsBuilder<TContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        var voltOptions = Options.Create(new VoltDbOptions());

        return (dbOptions, voltOptions);
    }
}
