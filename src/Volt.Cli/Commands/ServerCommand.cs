using System.CommandLine;
using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Starts the Volt development server by wrapping <c>dotnet watch run</c>
/// with a branded banner and configurable options.
/// </summary>
public static class ServerCommand
{
    /// <summary>
    /// Creates the <c>volt server</c> command definition.
    /// </summary>
    public static Command Create()
    {
        var portOption = new Option<int>("--port", "-p")
        {
            Description = "The port to run the server on",
            DefaultValueFactory = _ => 5000,
        };

        var openOption = new Option<bool>("--open", "-o")
        {
            Description = "Open the browser after starting",
            DefaultValueFactory = _ => false,
        };

        var httpsOption = new Option<bool>("--https")
        {
            Description = "Use HTTPS",
            DefaultValueFactory = _ => false,
        };

        var verboseOption = new Option<bool>("--verbose", "-v")
        {
            Description = "Show verbose output",
            DefaultValueFactory = _ => false,
        };

        var command = new Command("server", "Start the Volt development server");
        command.Aliases.Add("s");
        command.Add(portOption);
        command.Add(openOption);
        command.Add(httpsOption);
        command.Add(verboseOption);

        command.SetAction(async (parseResult, _) =>
        {
            var port = parseResult.GetValue(portOption);
            var open = parseResult.GetValue(openOption);
            var https = parseResult.GetValue(httpsOption);
            var verbose = parseResult.GetValue(verboseOption);
            await ExecuteAsync(port, open, https, verbose);
        });

        return command;
    }

    private static async Task ExecuteAsync(int port, bool open, bool https, bool verbose)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Banner();

        var scheme = https ? "https" : "http";
        var url = $"{scheme}://localhost:{port}";

        ConsoleOutput.Info($"Starting Volt server at {url}");
        ConsoleOutput.Info($"Project: {context.AppName}");
        ConsoleOutput.Info("Press Ctrl+C to stop.");
        ConsoleOutput.BlankLine();

        if (open)
        {
            OpenBrowser(url);
        }

        var arguments = BuildArguments(port, https, verbose);
        var webRoot = context.Layout.GetWebProjectRoot();
        var exitCode = await ProcessRunner.RunAsync(
            "dotnet", arguments, webRoot);

        if (exitCode != 0)
        {
            ConsoleOutput.Error($"Server exited with code {exitCode}.");
        }
    }

    private static string BuildArguments(int port, bool https, bool verbose)
    {
        var scheme = https ? "https" : "http";
        var args = $"watch run --non-interactive --urls \"{scheme}://localhost:{port}\"";

        if (verbose)
        {
            args += " --verbose";
        }

        return args;
    }

    private static void OpenBrowser(string url)
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                System.Diagnostics.Process.Start("open", url);
            }
            else if (OperatingSystem.IsLinux())
            {
                System.Diagnostics.Process.Start("xdg-open", url);
            }
            else if (OperatingSystem.IsWindows())
            {
                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
        catch
        {
            ConsoleOutput.Warning("Could not open browser automatically.");
        }
    }
}
