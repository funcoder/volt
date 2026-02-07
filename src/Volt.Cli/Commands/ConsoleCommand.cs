using System.CommandLine;
using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Launches an interactive C# REPL for the Volt project.
/// Attempts to use <c>csharprepl</c> or <c>dotnet-script</c> if available.
/// </summary>
public static class ConsoleCommand
{
    /// <summary>
    /// Creates the <c>volt console</c> command definition.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("console", "Open an interactive C# REPL");
        command.Aliases.Add("c");

        command.SetAction(async (_, _) =>
        {
            await ExecuteAsync();
        });

        return command;
    }

    private static async Task ExecuteAsync()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Banner();
        ConsoleOutput.Info($"Loading project: {context.GetAppName()}");
        ConsoleOutput.BlankLine();

        if (ProcessRunner.IsCommandAvailable("csharprepl"))
        {
            await LaunchCSharpRepl(context);
            return;
        }

        if (ProcessRunner.IsCommandAvailable("dotnet-script"))
        {
            await LaunchDotnetScript(context);
            return;
        }

        ShowInstallInstructions();
    }

    private static async Task LaunchCSharpRepl(ProjectContext context)
    {
        ConsoleOutput.Info("Launching CSharpRepl...");
        ConsoleOutput.BlankLine();

        await ProcessRunner.RunInteractiveAsync(
            "csharprepl",
            $"-r \"{context.CsprojPath}\"",
            context.ProjectRoot);
    }

    private static async Task LaunchDotnetScript(ProjectContext context)
    {
        ConsoleOutput.Info("Launching dotnet-script...");
        ConsoleOutput.BlankLine();

        await ProcessRunner.RunInteractiveAsync(
            "dotnet-script",
            workingDirectory: context.ProjectRoot);
    }

    private static void ShowInstallInstructions()
    {
        ConsoleOutput.Warning("No C# REPL found.");
        ConsoleOutput.BlankLine();
        ConsoleOutput.Info("Install CSharpRepl (recommended):");
        ConsoleOutput.Plain("  dotnet tool install -g csharprepl");
        ConsoleOutput.BlankLine();
        ConsoleOutput.Info("Or install dotnet-script:");
        ConsoleOutput.Plain("  dotnet tool install -g dotnet-script");
    }
}
