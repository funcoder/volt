namespace Volt.Cli.Helpers;

/// <summary>
/// Multi-project solution layout for Volt apps.
/// Routes files to the appropriate project within the solution structure.
/// </summary>
public sealed class SolutionLayout : IProjectLayout
{
    private readonly string _solutionRoot;
    private readonly string _appName;

    public SolutionLayout(string solutionRoot, string appName)
    {
        _solutionRoot = solutionRoot;
        _appName = appName;
    }

    public bool IsSolutionLayout => true;

    public string ResolveModelPath(params string[] parts) =>
        CombinePath(ModelsProjectRoot(), parts);

    public string ResolveControllerPath(params string[] parts) =>
        CombinePath(Path.Combine(WebProjectRoot(), "Controllers"), parts);

    public string ResolveViewPath(params string[] parts) =>
        CombinePath(Path.Combine(WebProjectRoot(), "Views"), parts);

    public string ResolveMigrationPath(params string[] parts) =>
        CombinePath(Path.Combine(DataProjectRoot(), "Migrations"), parts);

    public string ResolveSeedPath(params string[] parts) =>
        CombinePath(Path.Combine(DataProjectRoot(), "Seeds"), parts);

    public string ResolveJobPath(params string[] parts) =>
        CombinePath(Path.Combine(ServicesProjectRoot(), "Jobs"), parts);

    public string ResolveMailerPath(params string[] parts) =>
        CombinePath(Path.Combine(ServicesProjectRoot(), "Mailers"), parts);

    public string ResolveChannelPath(params string[] parts) =>
        CombinePath(Path.Combine(ServicesProjectRoot(), "Channels"), parts);

    public string ResolveServicePath(params string[] parts) =>
        CombinePath(ServicesProjectRoot(), parts);

    public string ResolveTestPath(params string[] parts) =>
        CombinePath(Path.Combine(_solutionRoot, "tests", $"{_appName}.Tests"), parts);

    public string ResolveDbContextPath() =>
        Path.Combine(DataProjectRoot(), "AppDbContext.cs");

    public string GetWebProjectRoot() => WebProjectRoot();

    public string? GetEfProjectArg() => $"--project src/{_appName}.Data";

    public string? GetEfStartupProjectArg() => $"--startup-project src/{_appName}.Web";

    public string GetSolutionOrProjectRoot() => _solutionRoot;

    public string GetModelNamespace() => $"{_appName}.Models";
    public string GetControllerNamespace() => $"{_appName}.Web.Controllers";
    public string GetDataNamespace() => $"{_appName}.Data";
    public string GetServiceNamespace() => $"{_appName}.Services";
    public string GetTestNamespace() => $"{_appName}.Tests";
    public string GetMigrationNamespace() => $"{_appName}.Data.Migrations";
    public string GetJobNamespace() => $"{_appName}.Services.Jobs";
    public string GetMailerNamespace() => $"{_appName}.Services.Mailers";
    public string GetChannelNamespace() => $"{_appName}.Services.Channels";

    private string ModelsProjectRoot() =>
        Path.Combine(_solutionRoot, "src", $"{_appName}.Models");

    private string DataProjectRoot() =>
        Path.Combine(_solutionRoot, "src", $"{_appName}.Data");

    private string ServicesProjectRoot() =>
        Path.Combine(_solutionRoot, "src", $"{_appName}.Services");

    private string WebProjectRoot() =>
        Path.Combine(_solutionRoot, "src", $"{_appName}.Web");

    private static string CombinePath(string root, string[] parts)
    {
        if (parts.Length == 0) return root;

        var segments = new string[parts.Length + 1];
        segments[0] = root;
        Array.Copy(parts, 0, segments, 1, parts.Length);
        return Path.Combine(segments);
    }
}
