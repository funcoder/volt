using Volt.Core.Conventions;

namespace Volt.Core.Configuration;

/// <summary>
/// Configuration options for the Volt framework.
/// Holds framework-wide settings that control application behavior and conventions.
/// </summary>
public sealed record VoltOptions
{
    /// <summary>
    /// The application name. Defaults to "VoltApp".
    /// </summary>
    public string AppName { get; init; } = "VoltApp";

    /// <summary>
    /// The current environment name (e.g., "Development", "Production").
    /// Defaults to "Development".
    /// </summary>
    public string Environment { get; init; } = "Development";

    /// <summary>
    /// The database provider to use. Defaults to <see cref="DbProvider.Sqlite"/>.
    /// </summary>
    public DbProvider DbProvider { get; init; } = DbProvider.Sqlite;

    /// <summary>
    /// The database connection string. Defaults to a local SQLite file.
    /// </summary>
    public string ConnectionString { get; init; } = "Data Source=volt.db";

    /// <summary>
    /// Whether to enable automatic database migrations on startup.
    /// Defaults to true in Development, should be false in Production.
    /// </summary>
    public bool AutoMigrate { get; init; } = true;

    /// <summary>
    /// Whether to enable soft-delete by default on all models.
    /// </summary>
    public bool SoftDeleteEnabled { get; init; } = true;

    /// <summary>
    /// The default page size for paginated queries.
    /// </summary>
    public int DefaultPageSize { get; init; } = 25;

    /// <summary>
    /// Whether the application is running in a development environment.
    /// </summary>
    public bool IsDevelopment => string.Equals(Environment, "Development", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether the application is running in a production environment.
    /// </summary>
    public bool IsProduction => string.Equals(Environment, "Production", StringComparison.OrdinalIgnoreCase);
}
