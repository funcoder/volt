using Volt.Core.Conventions;

namespace Volt.Data;

/// <summary>
/// Configuration options for the Volt database layer.
/// Provides a fluent API for enabling Rails-like conventions on EF Core.
/// </summary>
public sealed class VoltDbOptions
{
    /// <summary>
    /// The database provider to use. Defaults to <see cref="DbProvider.Sqlite"/>.
    /// </summary>
    public DbProvider DefaultProvider { get; private set; } = DbProvider.Sqlite;

    /// <summary>
    /// Whether to automatically set CreatedAt and UpdatedAt timestamps on save.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool UseTimestamps { get; private set; } = true;

    /// <summary>
    /// Whether to enable soft deletes via a global query filter on DeletedAt.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool UseSoftDeletes { get; private set; }

    /// <summary>
    /// Whether to pluralize table names (e.g. "User" becomes "users").
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool UsePluralTableNames { get; private set; } = true;

    /// <summary>
    /// Whether to use snake_case for column names (e.g. "FirstName" becomes "first_name").
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool UseSnakeCaseColumns { get; private set; } = true;

    /// <summary>
    /// Whether to run model lifecycle callbacks (IBeforeSave, IAfterCreate, etc.).
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool UseCallbacks { get; private set; } = true;

    /// <summary>
    /// The database connection string. When <c>null</c>, the provider default is used.
    /// </summary>
    public string? ConnectionString { get; private set; }

    /// <summary>
    /// Sets the database provider.
    /// </summary>
    /// <param name="provider">The provider to use.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public VoltDbOptions Provider(DbProvider provider)
    {
        DefaultProvider = provider;
        return this;
    }

    /// <summary>
    /// Sets the database connection string.
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public VoltDbOptions Connection(string connectionString)
    {
        ConnectionString = connectionString;
        return this;
    }

    /// <summary>
    /// Enables or disables automatic timestamp management.
    /// </summary>
    /// <param name="enabled">Whether to enable timestamps.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public VoltDbOptions Timestamps(bool enabled = true)
    {
        UseTimestamps = enabled;
        return this;
    }

    /// <summary>
    /// Enables or disables soft-delete filtering.
    /// </summary>
    /// <param name="enabled">Whether to enable soft deletes.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public VoltDbOptions SoftDeletes(bool enabled = true)
    {
        UseSoftDeletes = enabled;
        return this;
    }

    /// <summary>
    /// Enables or disables plural table names.
    /// </summary>
    /// <param name="enabled">Whether to pluralize table names.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public VoltDbOptions Pluralize(bool enabled = true)
    {
        UsePluralTableNames = enabled;
        return this;
    }

    /// <summary>
    /// Enables or disables snake_case column naming.
    /// </summary>
    /// <param name="enabled">Whether to use snake_case columns.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public VoltDbOptions SnakeCase(bool enabled = true)
    {
        UseSnakeCaseColumns = enabled;
        return this;
    }

    /// <summary>
    /// Enables or disables model lifecycle callbacks.
    /// </summary>
    /// <param name="enabled">Whether to run callbacks.</param>
    /// <returns>This options instance for fluent chaining.</returns>
    public VoltDbOptions Callbacks(bool enabled = true)
    {
        UseCallbacks = enabled;
        return this;
    }
}
