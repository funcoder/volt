using System.CommandLine;
using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Provides database management commands including migrations, seeding, and provider switching.
/// Wraps <c>dotnet ef</c> commands with Volt-specific conventions.
/// </summary>
public static class DbCommand
{
    /// <summary>
    /// Creates the <c>volt db</c> command with all subcommands.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("db", "Database management commands");

        command.Add(CreateMigrateSubcommand());
        command.Add(CreateRollbackSubcommand());
        command.Add(CreateSeedSubcommand());
        command.Add(CreateResetSubcommand());
        command.Add(CreateStatusSubcommand());
        command.Add(CreateConsoleSubcommand());
        command.Add(CreateProviderSubcommand());
        command.Add(CreateUseSubcommand());

        return command;
    }

    private static Command CreateMigrateSubcommand()
    {
        var cmd = new Command("migrate", "Run pending database migrations");

        cmd.SetAction(async (_, _) => { await Migrate(); });

        return cmd;
    }

    private static Command CreateRollbackSubcommand()
    {
        var stepsOption = new Option<int>("--steps", "-n")
        {
            Description = "Number of migrations to roll back",
            DefaultValueFactory = _ => 1,
        };

        var cmd = new Command("rollback", "Roll back the last migration");
        cmd.Add(stepsOption);

        cmd.SetAction(async (parseResult, _) =>
        {
            var steps = parseResult.GetValue(stepsOption);
            await Rollback(steps);
        });

        return cmd;
    }

    private static Command CreateSeedSubcommand()
    {
        var cmd = new Command("seed", "Run database seed data");

        cmd.SetAction(async (_, _) => { await Seed(); });

        return cmd;
    }

    private static Command CreateResetSubcommand()
    {
        var cmd = new Command("reset", "Drop, create, migrate, and seed the database");

        cmd.SetAction(async (_, _) => { await Reset(); });

        return cmd;
    }

    private static Command CreateStatusSubcommand()
    {
        var cmd = new Command("status", "Show migration status");

        cmd.SetAction(async (_, _) => { await Status(); });

        return cmd;
    }

    private static Command CreateConsoleSubcommand()
    {
        var cmd = new Command("console", "Open the database CLI tool");

        cmd.SetAction(async (_, _) => { await DbConsole(); });

        return cmd;
    }

    private static Command CreateProviderSubcommand()
    {
        var cmd = new Command("provider", "Show the current database provider");

        cmd.SetAction((_, _) =>
        {
            Provider();
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateUseSubcommand()
    {
        var providerArg = new Argument<string>("provider")
        {
            Description = "The database provider to switch to (sqlite, postgres, sqlserver)",
        };

        var cmd = new Command("use", "Switch the database provider");
        cmd.Add(providerArg);

        cmd.SetAction((parseResult, _) =>
        {
            Use(parseResult.GetValue(providerArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static async Task Migrate()
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

    private static async Task Rollback(int steps)
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

    private static async Task Seed()
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

    private static async Task Reset()
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

    private static async Task Status()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Info("Migration status:");
        ConsoleOutput.BlankLine();

        await ProcessRunner.RunAsync(
            "dotnet", "ef migrations list", context.ProjectRoot);
    }

    private static async Task DbConsole()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var provider = context.GetDatabaseProvider();

        ConsoleOutput.Info($"Opening {provider} console...");
        ConsoleOutput.BlankLine();

        var (command, commandArgs) = ResolveDbConsoleCommand(provider, context);

        if (command is null)
        {
            ConsoleOutput.Error($"No console tool found for provider '{provider}'.");
            return;
        }

        if (!ProcessRunner.IsCommandAvailable(command))
        {
            ConsoleOutput.Error($"'{command}' is not installed or not on PATH.");
            ShowDbConsoleInstallHelp(provider);
            return;
        }

        await ProcessRunner.RunInteractiveAsync(command, commandArgs, context.ProjectRoot);
    }

    private static void Provider()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var provider = context.GetDatabaseProvider();
        ConsoleOutput.Info($"Current database provider: {provider}");
    }

    private static void Use(string provider)
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

    private static (string? command, string args) ResolveDbConsoleCommand(
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

    private static void ShowDbConsoleInstallHelp(string provider)
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
