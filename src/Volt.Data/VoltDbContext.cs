using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Volt.Core;
using Volt.Core.Conventions;
using Volt.Data.Conventions;

namespace Volt.Data;

/// <summary>
/// Base <see cref="DbContext"/> for Volt applications.
/// Applies Rails-like conventions including automatic timestamps,
/// soft-delete global query filters, snake_case columns, and pluralized table names.
/// </summary>
public abstract class VoltDbContext : DbContext
{
    private readonly VoltDbOptions _voltOptions;

    /// <summary>
    /// Initializes a new <see cref="VoltDbContext"/> with the specified EF Core and Volt options.
    /// </summary>
    /// <param name="options">The EF Core <see cref="DbContextOptions"/>.</param>
    /// <param name="voltOptions">The Volt-specific configuration options.</param>
    protected VoltDbContext(DbContextOptions options, IOptions<VoltDbOptions> voltOptions)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(voltOptions);
        _voltOptions = voltOptions.Value;
    }

    /// <summary>
    /// Initializes a new <see cref="VoltDbContext"/> with EF Core options and default Volt conventions.
    /// Use this constructor when Volt options are not registered in DI.
    /// </summary>
    /// <param name="options">The EF Core <see cref="DbContextOptions"/>.</param>
    protected VoltDbContext(DbContextOptions options)
        : base(options)
    {
        _voltOptions = new VoltDbOptions();
    }

    /// <summary>
    /// Applies Volt conventions to the model during configuration.
    /// Call <c>base.OnModelCreating(modelBuilder)</c> from any override to preserve conventions.
    /// </summary>
    /// <param name="modelBuilder">The model builder provided by EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Apply snake_case columns to ALL entities (including non-Model types like VoltAttachment)
            if (_voltOptions.UseSnakeCaseColumns)
            {
                foreach (var property in entityType.GetProperties())
                {
                    property.SetColumnName(VoltConventions.ToColumnName(property.Name));
                }
            }

            if (!IsVoltModel(entityType.ClrType))
            {
                continue;
            }

            var configurator = new VoltModelConfiguration(
                entityType.ClrType,
                _voltOptions);

            configurator.Configure(modelBuilder.Entity(entityType.ClrType));
        }
    }

    /// <summary>
    /// Saves all changes, automatically setting CreatedAt and UpdatedAt timestamps
    /// on tracked <see cref="Model{T}"/> entities when timestamps are enabled.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_voltOptions.UseTimestamps)
        {
            ApplyTimestamps();
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Saves all changes synchronously, automatically setting timestamps when enabled.
    /// </summary>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges()
    {
        if (_voltOptions.UseTimestamps)
        {
            ApplyTimestamps();
        }

        return base.SaveChanges();
    }

    private void ApplyTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (!IsVoltModel(entry.Entity.GetType()))
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    SetPropertyIfExists(entry, "CreatedAt", now);
                    SetPropertyIfExists(entry, "UpdatedAt", now);
                    break;

                case EntityState.Modified:
                    SetPropertyIfExists(entry, "UpdatedAt", now);
                    break;
            }
        }
    }

    private static void SetPropertyIfExists(EntityEntry entry, string propertyName, object value)
    {
        var property = entry.Properties
            .FirstOrDefault(p => p.Metadata.Name == propertyName);

        if (property is not null)
        {
            property.CurrentValue = value;
        }
    }

    private static bool IsVoltModel(Type type)
    {
        var current = type;
        while (current is not null)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(Model<>))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }
}
