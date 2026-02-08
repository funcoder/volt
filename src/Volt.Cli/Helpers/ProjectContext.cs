namespace Volt.Cli.Helpers;

/// <summary>
/// Provides project-level context for the Volt CLI by locating
/// and reading the current Volt project configuration.
/// </summary>
public sealed class ProjectContext
{
    private const string VoltPackagePrefix = "VoltFramework.";
    private const string CsprojExtension = ".csproj";
    private const string SlnExtension = ".sln";

    private ProjectContext(string projectRoot, string csprojPath, IProjectLayout layout, string appName)
    {
        ProjectRoot = projectRoot;
        CsprojPath = csprojPath;
        Layout = layout;
        AppName = appName;
    }

    /// <summary>
    /// The root directory of the Volt project (or solution root for multi-project).
    /// </summary>
    public string ProjectRoot { get; }

    /// <summary>
    /// The full path to the project's .csproj file (the Web project in solution layout).
    /// </summary>
    public string CsprojPath { get; }

    /// <summary>
    /// The project layout strategy for resolving paths and namespaces.
    /// </summary>
    public IProjectLayout Layout { get; }

    /// <summary>
    /// The base application name (e.g., "MyApp" not "MyApp.Web").
    /// </summary>
    public string AppName { get; }

    /// <summary>
    /// Walks up the directory tree from the current directory to find a Volt project.
    /// Checks for .sln with Volt projects first, then falls back to single .csproj.
    /// </summary>
    public static ProjectContext? FindProjectRoot(string? startDirectory = null)
    {
        var directory = startDirectory ?? Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(directory))
        {
            var solutionContext = TryDetectSolutionLayout(directory);
            if (solutionContext is not null) return solutionContext;

            var singleContext = TryDetectSingleProjectLayout(directory);
            if (singleContext is not null) return singleContext;

            directory = Directory.GetParent(directory)?.FullName;
        }

        return null;
    }

    /// <summary>
    /// Extracts the application name from the .csproj filename.
    /// Prefer using the AppName property directly.
    /// </summary>
    public string GetAppName() => AppName;

    /// <summary>
    /// Reads the current database provider from appsettings.json or the .csproj file.
    /// </summary>
    public string GetDatabaseProvider()
    {
        var webRoot = Layout.GetWebProjectRoot();
        var appSettingsPath = Path.Combine(webRoot, "appsettings.json");

        if (File.Exists(appSettingsPath))
        {
            var content = File.ReadAllText(appSettingsPath);

            if (content.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
                return "postgres";

            if (content.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
                return "sqlserver";
        }

        var csprojContent = File.ReadAllText(CsprojPath);

        if (csprojContent.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
            return "postgres";

        if (csprojContent.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
            return "sqlserver";

        return "sqlite";
    }

    /// <summary>
    /// Resolves a relative path within the project root to an absolute path.
    /// Prefer using Layout.Resolve*Path() methods instead.
    /// </summary>
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

    private static ProjectContext? TryDetectSolutionLayout(string directory)
    {
        var slnFiles = Directory.GetFiles(directory, $"*{SlnExtension}");

        foreach (var slnFile in slnFiles)
        {
            var appName = Path.GetFileNameWithoutExtension(slnFile);
            var webCsproj = Path.Combine(directory, "src", $"{appName}.Web", $"{appName}.Web.csproj");

            if (!File.Exists(webCsproj)) continue;
            if (!IsVoltProject(webCsproj)) continue;

            var layout = new SolutionLayout(directory, appName);
            return new ProjectContext(directory, webCsproj, layout, appName);
        }

        return null;
    }

    private static ProjectContext? TryDetectSingleProjectLayout(string directory)
    {
        var csprojFiles = Directory.GetFiles(directory, $"*{CsprojExtension}");

        foreach (var csproj in csprojFiles)
        {
            if (!IsVoltProject(csproj)) continue;

            var rawName = Path.GetFileNameWithoutExtension(csproj);
            var appName = StripProjectSuffix(rawName);
            var layout = new SingleProjectLayout(directory, appName);
            return new ProjectContext(directory, csproj, layout, appName);
        }

        return null;
    }

    private static string StripProjectSuffix(string name)
    {
        string[] suffixes = [".Web", ".Data", ".Models", ".Services"];

        foreach (var suffix in suffixes)
        {
            if (name.EndsWith(suffix, StringComparison.Ordinal))
                return name[..^suffix.Length];
        }

        return name;
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
