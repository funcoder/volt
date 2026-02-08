namespace Volt.Cli.Helpers;

/// <summary>
/// Strategy interface for resolving file paths and namespaces across
/// different project layouts (single-project vs multi-project solution).
/// </summary>
public interface IProjectLayout
{
    bool IsSolutionLayout { get; }

    string ResolveModelPath(params string[] parts);
    string ResolveControllerPath(params string[] parts);
    string ResolveViewPath(params string[] parts);
    string ResolveMigrationPath(params string[] parts);
    string ResolveSeedPath(params string[] parts);
    string ResolveJobPath(params string[] parts);
    string ResolveMailerPath(params string[] parts);
    string ResolveChannelPath(params string[] parts);
    string ResolveServicePath(params string[] parts);
    string ResolveTestPath(params string[] parts);
    string ResolveDbContextPath();
    string GetWebProjectRoot();
    string? GetEfProjectArg();
    string? GetEfStartupProjectArg();
    string GetSolutionOrProjectRoot();

    string GetModelNamespace();
    string GetControllerNamespace();
    string GetDataNamespace();
    string GetServiceNamespace();
    string GetTestNamespace();
    string GetMigrationNamespace();
    string GetJobNamespace();
    string GetMailerNamespace();
    string GetChannelNamespace();
}
