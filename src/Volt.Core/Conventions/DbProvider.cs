namespace Volt.Core.Conventions;

/// <summary>
/// The supported database providers for the Volt framework.
/// </summary>
public enum DbProvider
{
    /// <summary>SQLite - lightweight file-based database, ideal for development.</summary>
    Sqlite,

    /// <summary>PostgreSQL - robust open-source relational database for production.</summary>
    Postgres,

    /// <summary>SQL Server - Microsoft's enterprise relational database.</summary>
    SqlServer
}
