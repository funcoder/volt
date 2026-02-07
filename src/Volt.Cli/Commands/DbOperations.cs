using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Database operation implementations for migration, seeding, rollback, reset, and provider management.
/// Called by <see cref="DbCommand"/> subcommand handlers.
/// </summary>
public static class DbOperations
{
    public static async Task Migrate()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Info("Running database migrations...");
        ConsoleOutput.BlankLine();

        var exitCode = await ProcessRunner.RunAsync(
            "dotnet", "ef database update", context.ProjectRoot);

        if (exitCode == 0)
        {
            ConsoleOutput.Success("Migrations applied successfully.");
        }
        else
        {
            ConsoleOutput.Error("Migration failed. Is dotnet-ef tool installed?");
            ConsoleOutput.Info("Install with: dotnet tool install --global dotnet-ef");
        }
    }

    public static async Task Rollback(int steps)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Info($"Rolling back {steps} migration(s)...");
        ConsoleOutput.BlankLine();

        for (var i = 0; i < steps; i++)
        {
            var exitCode = await ProcessRunner.RunAsync(
                "dotnet", "ef migrations remove --force", context.ProjectRoot);

            if (exitCode != 0)
            {
                ConsoleOutput.Error($"Rollback failed at step {i + 1}.");
                return;
            }
        }

        ConsoleOutput.Success($"Rolled back {steps} migration(s).");
    }

    public static async Task Seed()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Info("Running database seeds...");
        ConsoleOutput.BlankLine();

        var exitCode = await ProcessRunner.RunAsync(
            "dotnet", "run -- --seed", context.ProjectRoot);

        if (exitCode == 0)
        {
            ConsoleOutput.Success("Seeds completed successfully.");
        }
        else
        {
            ConsoleOutput.Error("Seed failed. Ensure Seeds/SeedData.cs exists.");
        }
    }

    public static async Task Reset()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Warning("Resetting database (drop + create + migrate + seed)...");
        ConsoleOutput.BlankLine();

        var dropCode = await ProcessRunner.RunAsync(
            "dotnet", "ef database drop --force", context.ProjectRoot);

        if (dropCode != 0)
        {
            ConsoleOutput.Error("Failed to drop database.");
            return;
        }

        ConsoleOutput.Success("Database dropped.");

        var migrateCode = await ProcessRunner.RunAsync(
            "dotnet", "ef database update", context.ProjectRoot);

        if (migrateCode != 0)
        {
            ConsoleOutput.Error("Failed to apply migrations after reset.");
            return;
        }

        ConsoleOutput.Success("Migrations applied.");

        var seedCode = await ProcessRunner.RunAsync(
            "dotnet", "run -- --seed", context.ProjectRoot);

        if (seedCode == 0)
        {
            ConsoleOutput.Success("Seeds completed.");
        }
        else
        {
            ConsoleOutput.Warning("Seeds skipped or failed.");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success("Database reset complete.");
    }

    public static async Task Status()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Info("Migration status:");
        ConsoleOutput.BlankLine();

        await ProcessRunner.RunAsync(
            "dotnet", "ef migrations list", context.ProjectRoot);
    }

    public static async Task DbConsole()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var provider = context.GetDatabaseProvider();

        ConsoleOutput.Info($"Opening {provider} console...");
        ConsoleOutput.BlankLine();

        var (command, commandArgs) = ResolveConsoleCommand(provider, context);

        if (command is null)
        {
            ConsoleOutput.Error($"No console tool found for provider '{provider}'.");
            return;
        }

        if (!ProcessRunner.IsCommandAvailable(command))
        {
            ConsoleOutput.Error($"'{command}' is not installed or not on PATH.");
            ShowConsoleInstallHelp(provider);
            return;
        }

        await ProcessRunner.RunInteractiveAsync(command, commandArgs, context.ProjectRoot);
    }

    public static void Provider()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var provider = context.GetDatabaseProvider();
        ConsoleOutput.Info($"Current database provider: {provider}");
    }

    public static void Use(string provider)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var validProviders = new[] { "sqlite", "postgres", "sqlserver" };

        if (!validProviders.Contains(provider, StringComparer.OrdinalIgnoreCase))
        {
            ConsoleOutput.Error(
                $"Invalid provider '{provider}'. " +
                $"Valid options: {string.Join(", ", validProviders)}");
            return;
        }

        ConsoleOutput.Warning(
            "Provider switching requires manual configuration updates.");
        ConsoleOutput.BlankLine();
        ConsoleOutput.Info("Steps to switch providers:");
        ConsoleOutput.Plain("  1. Update the connection string in appsettings.json");
        ConsoleOutput.Plain("  2. Update the DbContext configuration in Program.cs");
        ConsoleOutput.Plain("  3. Add the appropriate NuGet package:");

        switch (provider.ToLowerInvariant())
        {
            case "sqlite":
                ConsoleOutput.Plain("     dotnet add package Microsoft.EntityFrameworkCore.Sqlite");
                break;
            case "postgres":
                ConsoleOutput.Plain("     dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL");
                break;
            case "sqlserver":
                ConsoleOutput.Plain("     dotnet add package Microsoft.EntityFrameworkCore.SqlServer");
                break;
        }

        ConsoleOutput.Plain("  4. Re-run migrations: volt db migrate");
    }

    private static (string? command, string args) ResolveConsoleCommand(
        string provider, ProjectContext context)
    {
        return provider.ToLowerInvariant() switch
        {
            "sqlite" => ("sqlite3", FindSqliteDatabase(context)),
            "postgres" => ("psql", string.Empty),
            "sqlserver" => ("sqlcmd", string.Empty),
            _ => (null, string.Empty),
        };
    }

    private static string FindSqliteDatabase(ProjectContext context)
    {
        var dbFiles = Directory.GetFiles(context.ProjectRoot, "*.db");
        return dbFiles.Length > 0 ? dbFiles[0] : "app.db";
    }

    private static void ShowConsoleInstallHelp(string provider)
    {
        switch (provider.ToLowerInvariant())
        {
            case "sqlite":
                ConsoleOutput.Info("Install SQLite: brew install sqlite (macOS) or apt install sqlite3 (Linux)");
                break;
            case "postgres":
                ConsoleOutput.Info("Install PostgreSQL client: brew install postgresql (macOS)");
                break;
            case "sqlserver":
                ConsoleOutput.Info("Install sqlcmd: https://learn.microsoft.com/sql/tools/sqlcmd/sqlcmd-utility");
                break;
        }
    }
}
