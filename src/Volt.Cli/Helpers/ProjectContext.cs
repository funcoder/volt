namespace Volt.Cli.Helpers;

/// <summary>
/// Provides project-level context for the Volt CLI by locating
/// and reading the current Volt project configuration.
/// </summary>
public sealed class ProjectContext
{
    private const string VoltPackagePrefix = "VoltFramework.";
    private const string CsprojExtension = ".csproj";

    private ProjectContext(string projectRoot, string csprojPath)
    {
        ProjectRoot = projectRoot;
        CsprojPath = csprojPath;
    }

    /// <summary>
    /// The root directory of the Volt project.
    /// </summary>
    public string ProjectRoot { get; }

    /// <summary>
    /// The full path to the project's .csproj file.
    /// </summary>
    public string CsprojPath { get; }

    /// <summary>
    /// Walks up the directory tree from the current directory to find a .csproj
    /// file that references Volt packages. Returns null if no Volt project is found.
    /// </summary>
    /// <param name="startDirectory">The directory to start searching from. Defaults to the current directory.</param>
    /// <returns>A <see cref="ProjectContext"/> if a Volt project is found; otherwise null.</returns>
    public static ProjectContext? FindProjectRoot(string? startDirectory = null)
    {
        var directory = startDirectory ?? Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(directory))
        {
            var csprojFiles = Directory.GetFiles(directory, $"*{CsprojExtension}");

            foreach (var csproj in csprojFiles)
            {
                if (IsVoltProject(csproj))
                {
                    return new ProjectContext(directory, csproj);
                }
            }

            directory = Directory.GetParent(directory)?.FullName;
        }

        return null;
    }

    /// <summary>
    /// Extracts the application name from the .csproj filename.
    /// For example, "MyApp.csproj" returns "MyApp".
    /// </summary>
    /// <returns>The application name derived from the project file.</returns>
    public string GetAppName()
    {
        return Path.GetFileNameWithoutExtension(CsprojPath);
    }

    /// <summary>
    /// Reads the current database provider from appsettings.json or the .csproj file.
    /// Returns "sqlite" as the default if no provider is configured.
    /// </summary>
    /// <returns>The database provider name (sqlite, postgres, or sqlserver).</returns>
    public string GetDatabaseProvider()
    {
        var appSettingsPath = Path.Combine(ProjectRoot, "appsettings.json");

        if (File.Exists(appSettingsPath))
        {
            var content = File.ReadAllText(appSettingsPath);

            if (content.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
            {
                return "postgres";
            }

            if (content.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                return "sqlserver";
            }
        }

        var csprojContent = File.ReadAllText(CsprojPath);

        if (csprojContent.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return "postgres";
        }

        if (csprojContent.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return "sqlserver";
        }

        return "sqlite";
    }

    /// <summary>
    /// Resolves a relative path within the project root to an absolute path.
    /// </summary>
    /// <param name="relativePath">The relative path from the project root.</param>
    /// <returns>The full absolute path.</returns>
    public string ResolvePath(params string[] relativePath)
    {
        var parts = new string[relativePath.Length + 1];
        parts[0] = ProjectRoot;
        Array.Copy(relativePath, 0, parts, 1, relativePath.Length);
        return Path.Combine(parts);
    }

    /// <summary>
    /// Requires a Volt project context. If no project is found, prints an error
    /// message and returns null to signal that the command should abort.
    /// </summary>
    /// <returns>The project context, or null if not inside a Volt project.</returns>
    public static ProjectContext? Require()
    {
        var context = FindProjectRoot();

        if (context is null)
        {
            ConsoleOutput.Error(
                "Not inside a Volt project. Run this command from a Volt project directory, " +
                "or create a new project with: volt new <name>");
        }

        return context;
    }

    private static bool IsVoltProject(string csprojPath)
    {
        try
        {
            var content = File.ReadAllText(csprojPath);
            return content.Contains(VoltPackagePrefix, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }
}
