using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volt.Core;
using Volt.Core.Conventions;

namespace Volt.Data.Conventions;

/// <summary>
/// Applies Volt conventions to a single entity type that inherits from <see cref="Model{T}"/>.
/// Handles table naming (snake_case, pluralized), column naming, timestamp configuration,
/// and soft-delete global query filters.
/// </summary>
public sealed class VoltModelConfiguration
{
    private readonly Type _entityType;
    private readonly VoltDbOptions _options;

    /// <summary>
    /// Creates a new configuration for the given entity type and options.
    /// </summary>
    /// <param name="entityType">The CLR type of the entity being configured.</param>
    /// <param name="options">The Volt database options controlling which conventions are applied.</param>
    public VoltModelConfiguration(Type entityType, VoltDbOptions options)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        ArgumentNullException.ThrowIfNull(options);

        _entityType = entityType;
        _options = options;
    }

    /// <summary>
    /// Applies all enabled conventions to the entity type builder.
    /// </summary>
    /// <param name="builder">The entity type builder for the target entity.</param>
    public void Configure(EntityTypeBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        ConfigureTableName(builder);
        ConfigureColumns(builder);
        ConfigureTimestamps(builder);
        ConfigureSoftDeleteFilter(builder);
    }

    private void ConfigureTableName(EntityTypeBuilder builder)
    {
        var name = _entityType.Name;

        if (_options.UseSnakeCaseColumns)
        {
            name = VoltConventions.ToSnakeCase(name);
        }

        if (_options.UsePluralTableNames)
        {
            name = VoltConventions.Pluralize(name);
        }

        builder.ToTable(name);
    }

    private void ConfigureColumns(EntityTypeBuilder builder)
    {
        if (!_options.UseSnakeCaseColumns)
        {
            return;
        }

        foreach (var property in builder.Metadata.GetProperties())
        {
            var columnName = VoltConventions.ToColumnName(property.Name);
            property.SetColumnName(columnName);
        }
    }

    private void ConfigureTimestamps(EntityTypeBuilder builder)
    {
        if (!_options.UseTimestamps)
        {
            return;
        }

        var createdAt = builder.Metadata.FindProperty("CreatedAt");
        createdAt?.SetDefaultValueSql("CURRENT_TIMESTAMP");

        var updatedAt = builder.Metadata.FindProperty("UpdatedAt");
        updatedAt?.SetDefaultValueSql("CURRENT_TIMESTAMP");
    }

    private void ConfigureSoftDeleteFilter(EntityTypeBuilder builder)
    {
        if (!_options.UseSoftDeletes)
        {
            return;
        }

        // Build: entity => EF.Property<DateTime?>(entity, "DeletedAt") == null
        var parameter = Expression.Parameter(_entityType, "entity");

        var efPropertyMethod = typeof(EF)
            .GetMethod(nameof(EF.Property))!
            .MakeGenericMethod(typeof(DateTime?));

        var propertyAccess = Expression.Call(
            efPropertyMethod,
            parameter,
            Expression.Constant("DeletedAt"));

        var nullConstant = Expression.Constant(null, typeof(DateTime?));
        var comparison = Expression.Equal(propertyAccess, nullConstant);
        var lambda = Expression.Lambda(comparison, parameter);

        builder.HasQueryFilter(lambda);
    }
}
