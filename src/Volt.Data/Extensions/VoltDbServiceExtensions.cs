using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Volt.Core.Conventions;

namespace Volt.Data.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register
/// the Volt database layer with the dependency injection container.
/// </summary>
public static class VoltDbServiceExtensions
{
    /// <summary>
    /// Registers a <typeparamref name="TContext"/> with EF Core using Volt conventions.
    /// Configures the database provider, connection string, and convention options.
    /// </summary>
    /// <typeparam name="TContext">
    /// The concrete <see cref="VoltDbContext"/> subclass to register.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure <see cref="VoltDbOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoltDb<TContext>(
        this IServiceCollection services,
        Action<VoltDbOptions> configure)
        where TContext : VoltDbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var voltOptions = new VoltDbOptions();
        configure(voltOptions);

        services.Configure<VoltDbOptions>(opts =>
        {
            configure(opts);
        });

        services.AddDbContext<TContext>((_, dbOptions) =>
        {
            ConfigureProvider(dbOptions, voltOptions);
        });

        return services;
    }

    /// <summary>
    /// Registers a <typeparamref name="TContext"/> with EF Core using Volt conventions,
    /// also registering it as the base <see cref="VoltDbContext"/> service type.
    /// </summary>
    /// <typeparam name="TContext">
    /// The concrete <see cref="VoltDbContext"/> subclass to register.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An action to configure <see cref="VoltDbOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddVoltDbWithBase<TContext>(
        this IServiceCollection services,
        Action<VoltDbOptions> configure)
        where TContext : VoltDbContext
    {
        services.AddVoltDb<TContext>(configure);
        services.AddScoped<VoltDbContext>(sp => sp.GetRequiredService<TContext>());
        return services;
    }

    private static void ConfigureProvider(DbContextOptionsBuilder dbOptions, VoltDbOptions voltOptions)
    {
        var connectionString = voltOptions.ConnectionString;

        switch (voltOptions.DefaultProvider)
        {
            case DbProvider.Sqlite:
                dbOptions.UseSqlite(connectionString ?? "Data Source=volt.db");
                break;

            case DbProvider.Postgres:
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "A connection string is required for the PostgreSQL provider. " +
                        "Call .Connection(\"...\") on VoltDbOptions.");
                }
                dbOptions.UseNpgsql(connectionString);
                break;

            case DbProvider.SqlServer:
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "A connection string is required for the SQL Server provider. " +
                        "Call .Connection(\"...\") on VoltDbOptions.");
                }
                dbOptions.UseSqlServer(connectionString);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(voltOptions),
                    voltOptions.DefaultProvider,
                    $"Unsupported database provider: {voltOptions.DefaultProvider}");
        }

        if (voltOptions.UseSnakeCaseColumns)
        {
            dbOptions.UseSnakeCaseNamingConvention();
        }
    }
}
