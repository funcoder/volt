namespace Volt.Cli.Helpers;

/// <summary>
/// Backwards-compatible layout for single-project Volt apps.
/// All paths resolve relative to the single project root.
/// </summary>
public sealed class SingleProjectLayout : IProjectLayout
{
    private readonly string _projectRoot;
    private readonly string _appName;

    public SingleProjectLayout(string projectRoot, string appName)
    {
        _projectRoot = projectRoot;
        _appName = appName;
    }

    public bool IsSolutionLayout => false;

    public string ResolveModelPath(params string[] parts) =>
        CombinePath(_projectRoot, "Models", parts);

    public string ResolveControllerPath(params string[] parts) =>
        CombinePath(_projectRoot, "Controllers", parts);

    public string ResolveViewPath(params string[] parts) =>
        CombinePath(_projectRoot, "Views", parts);

    public string ResolveMigrationPath(params string[] parts) =>
        CombinePath(_projectRoot, "Migrations", parts);

    public string ResolveSeedPath(params string[] parts) =>
        CombinePath(_projectRoot, "Seeds", parts);

    public string ResolveJobPath(params string[] parts) =>
        CombinePath(_projectRoot, "Jobs", parts);

    public string ResolveMailerPath(params string[] parts) =>
        CombinePath(_projectRoot, "Mailers", parts);

    public string ResolveChannelPath(params string[] parts) =>
        CombinePath(_projectRoot, "Channels", parts);

    public string ResolveServicePath(params string[] parts) =>
        CombinePath(_projectRoot, "Services", parts);

    public string ResolveTestPath(params string[] parts) =>
        CombinePath(_projectRoot, "Tests", parts);

    public string ResolveDbContextPath() =>
        Path.Combine(_projectRoot, "Data", "AppDbContext.cs");

    public string GetWebProjectRoot() => _projectRoot;

    public string? GetEfProjectArg() => null;

    public string? GetEfStartupProjectArg() => null;

    public string GetSolutionOrProjectRoot() => _projectRoot;

    public string GetModelNamespace() => $"{_appName}.Models";
    public string GetControllerNamespace() => $"{_appName}.Controllers";
    public string GetDataNamespace() => $"{_appName}.Data";
    public string GetServiceNamespace() => $"{_appName}.Services";
    public string GetTestNamespace() => $"{_appName}.Tests";
    public string GetMigrationNamespace() => $"{_appName}.Migrations";
    public string GetJobNamespace() => $"{_appName}.Jobs";
    public string GetMailerNamespace() => $"{_appName}.Mailers";
    public string GetChannelNamespace() => $"{_appName}.Channels";

    public string GetDataCsprojPath()
    {
        var csprojFiles = Directory.GetFiles(_projectRoot, "*.csproj");
        return csprojFiles.Length > 0 ? csprojFiles[0] : Path.Combine(_projectRoot, $"{_appName}.csproj");
    }

    private static string CombinePath(string root, string folder, string[] parts)
    {
        var segments = new string[parts.Length + 2];
        segments[0] = root;
        segments[1] = folder;
        Array.Copy(parts, 0, segments, 2, parts.Length);
        return Path.Combine(segments);
    }
}
