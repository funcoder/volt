using System.CommandLine;

namespace Volt.Cli.Commands;

/// <summary>
/// Defines the <c>volt db</c> command with subcommands for database management.
/// Delegates all operation logic to <see cref="DbOperations"/>.
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
        command.Add(CreateDockerSubcommand());

        return command;
    }

    private static Command CreateMigrateSubcommand()
    {
        var cmd = new Command("migrate", "Run pending database migrations");

        cmd.SetAction(async (_, _) => { await DbOperations.Migrate(); });

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
            await DbOperations.Rollback(steps);
        });

        return cmd;
    }

    private static Command CreateSeedSubcommand()
    {
        var cmd = new Command("seed", "Run database seed data");

        cmd.SetAction(async (_, _) => { await DbOperations.Seed(); });

        return cmd;
    }

    private static Command CreateResetSubcommand()
    {
        var cmd = new Command("reset", "Drop, create, migrate, and seed the database");

        cmd.SetAction(async (_, _) => { await DbOperations.Reset(); });

        return cmd;
    }

    private static Command CreateStatusSubcommand()
    {
        var cmd = new Command("status", "Show migration status");

        cmd.SetAction(async (_, _) => { await DbOperations.Status(); });

        return cmd;
    }

    private static Command CreateConsoleSubcommand()
    {
        var cmd = new Command("console", "Open the database CLI tool");

        cmd.SetAction(async (_, _) => { await DbOperations.DbConsole(); });

        return cmd;
    }

    private static Command CreateProviderSubcommand()
    {
        var cmd = new Command("provider", "Show the current database provider");

        cmd.SetAction((_, _) =>
        {
            DbOperations.Provider();
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
            DbOperations.Use(parseResult.GetValue(providerArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateDockerSubcommand()
    {
        var docker = new Command("docker", "Docker database lifecycle management");

        docker.Add(CreateDockerUpSubcommand());
        docker.Add(CreateDockerDownSubcommand());
        docker.Add(CreateDockerStatusSubcommand());
        docker.Add(CreateDockerLogsSubcommand());

        return docker;
    }

    private static Command CreateDockerUpSubcommand()
    {
        var providerArg = new Argument<string?>("provider")
        {
            Description = "The database provider (postgres or sqlserver). Auto-detects if omitted.",
            Arity = ArgumentArity.ZeroOrOne,
        };

        var cmd = new Command("up", "Start a Docker database container");
        cmd.Add(providerArg);

        cmd.SetAction(async (parseResult, _) =>
        {
            var provider = parseResult.GetValue(providerArg);
            await DbDockerOperations.Up(provider);
        });

        return cmd;
    }

    private static Command CreateDockerDownSubcommand()
    {
        var cmd = new Command("down", "Stop and remove the Docker database container");

        cmd.SetAction(async (_, _) => { await DbDockerOperations.Down(); });

        return cmd;
    }

    private static Command CreateDockerStatusSubcommand()
    {
        var cmd = new Command("status", "Show Docker database container status");

        cmd.SetAction(async (_, _) => { await DbDockerOperations.Status(); });

        return cmd;
    }

    private static Command CreateDockerLogsSubcommand()
    {
        var cmd = new Command("logs", "Tail Docker database container logs");

        cmd.SetAction(async (_, _) => { await DbDockerOperations.Logs(); });

        return cmd;
    }
}
