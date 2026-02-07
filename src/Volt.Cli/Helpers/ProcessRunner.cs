using System.Diagnostics;

namespace Volt.Cli.Helpers;

/// <summary>
/// Runs external processes (dotnet, database CLIs, etc.) with output forwarding.
/// Wraps <see cref="Process"/> to provide a consistent interface for the CLI commands.
/// </summary>
public static class ProcessRunner
{
    /// <summary>
    /// Runs a process and waits for it to complete, forwarding stdout and stderr to the console.
    /// </summary>
    /// <param name="fileName">The executable to run.</param>
    /// <param name="arguments">The command-line arguments.</param>
    /// <param name="workingDirectory">The working directory for the process. Defaults to current directory.</param>
    /// <returns>The process exit code.</returns>
    public static async Task<int> RunAsync(
        string fileName,
        string arguments,
        string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                ConsoleOutput.Plain(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                ConsoleOutput.Plain(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        return process.ExitCode;
    }

    /// <summary>
    /// Runs a process interactively (no output redirection), allowing user interaction
    /// with tools like database consoles or REPLs.
    /// </summary>
    /// <param name="fileName">The executable to run.</param>
    /// <param name="arguments">The command-line arguments.</param>
    /// <param name="workingDirectory">The working directory for the process.</param>
    /// <returns>The process exit code.</returns>
    public static async Task<int> RunInteractiveAsync(
        string fileName,
        string arguments = "",
        string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory(),
            UseShellExecute = false,
        };

        using var process = new Process { StartInfo = startInfo };

        process.Start();
        await process.WaitForExitAsync();

        return process.ExitCode;
    }

    /// <summary>
    /// Checks whether a command-line tool is available on the system PATH.
    /// </summary>
    /// <param name="command">The command name to check (e.g., "csharprepl", "sqlite3").</param>
    /// <returns>True if the command is available; otherwise false.</returns>
    public static bool IsCommandAvailable(string command)
    {
        try
        {
            var whichCommand = OperatingSystem.IsWindows() ? "where" : "which";
            var startInfo = new ProcessStartInfo
            {
                FileName = whichCommand,
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            process?.WaitForExit();

            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
