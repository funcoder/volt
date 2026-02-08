using System.CommandLine;
using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Creates a new Volt project by delegating to <c>dotnet new</c>
/// and running <c>dotnet restore</c>.
/// </summary>
public static class NewCommand
{
    private static readonly string[] ValidDatabases = ["sqlite", "postgres", "sqlserver"];

    /// <summary>
    /// Creates the <c>volt new &lt;name&gt;</c> command definition.
    /// </summary>
    public static Command Create()
    {
        var nameArgument = new Argument<string>("name")
        {
            Description = "The name of the new Volt project",
        };

        var databaseOption = new Option<string>("--database", "-d")
        {
            Description = "The database provider to use (sqlite, postgres, sqlserver)",
            DefaultValueFactory = _ => "sqlite",
        };

        var apiOption = new Option<bool>("--api")
        {
            Description = "Create an API-only project (no views)",
            DefaultValueFactory = _ => false,
        };

        var simpleOption = new Option<bool>("--simple")
        {
            Description = "Create a single-project app instead of a multi-project solution",
            DefaultValueFactory = _ => false,
        };

        var command = new Command("new", "Create a new Volt project");
        command.Add(nameArgument);
        command.Add(databaseOption);
        command.Add(apiOption);
        command.Add(simpleOption);

        command.SetAction(async (parseResult, _) =>
        {
            var name = parseResult.GetValue(nameArgument);
            var database = parseResult.GetValue(databaseOption);
            var api = parseResult.GetValue(apiOption);
            var simple = parseResult.GetValue(simpleOption);
            await ExecuteAsync(name!, database!, api, simple);
        });

        return command;
    }

    private static async Task ExecuteAsync(string name, string database, bool api, bool simple)
    {
        ConsoleOutput.Banner();

        if (!ValidDatabases.Contains(database, StringComparer.OrdinalIgnoreCase))
        {
            ConsoleOutput.Error(
                $"Invalid database provider '{database}'. " +
                $"Valid options: {string.Join(", ", ValidDatabases)}");
            return;
        }

        ConsoleOutput.Info($"Creating new Volt project: {name}");
        ConsoleOutput.Info($"Database: {database}");

        if (api)
        {
            ConsoleOutput.Info("Mode: API-only");
        }
        else if (!simple)
        {
            ConsoleOutput.Info("Mode: Multi-project solution");
        }

        ConsoleOutput.BlankLine();

        var projectDir = Path.Combine(Directory.GetCurrentDirectory(), name);

        if (Directory.Exists(projectDir))
        {
            ConsoleOutput.Error($"Directory '{name}' already exists.");
            return;
        }

        var templateArgs = BuildTemplateArgs(name, database, api, simple);
        var exitCode = await ProcessRunner.RunAsync("dotnet", $"new {templateArgs}");

        if (exitCode != 0)
        {
            ConsoleOutput.Error(
                "Failed to create project. Ensure the Volt template is installed: " +
                "dotnet new install VoltFramework.Templates");
            return;
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Info("Restoring packages...");

        var restoreCode = await ProcessRunner.RunAsync("dotnet", "restore", projectDir);

        if (restoreCode != 0)
        {
            ConsoleOutput.Warning("Package restore completed with warnings.");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Project '{name}' created successfully!");
        ConsoleOutput.BlankLine();
        ConsoleOutput.Info("Next steps:");
        ConsoleOutput.Plain($"  cd {name}");
        ConsoleOutput.Plain("  volt server");
    }

    private static string BuildTemplateArgs(string name, string database, bool api, bool simple)
    {
        var templateName = api ? "volt-api" : (simple ? "volt" : "volt-sln");
        return $"{templateName} -n {name} --database {database}";
    }
}
