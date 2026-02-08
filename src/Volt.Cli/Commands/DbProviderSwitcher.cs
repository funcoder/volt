using System.Text.RegularExpressions;
using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Handles switching a Volt project's database provider by updating
/// the .csproj package references and the AppDbContext configuration.
/// </summary>
public static class DbProviderSwitcher
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Switches the project's database provider by updating the .csproj and AppDbContext files.
    /// </summary>
    public static bool Switch(ProjectContext context, string newProvider)
    {
        var csprojPath = context.Layout.GetDataCsprojPath();

        if (!File.Exists(csprojPath))
        {
            ConsoleOutput.Error($"Could not find project file: {csprojPath}");
            return false;
        }

        if (!UpdateCsproj(csprojPath, newProvider))
            return false;

        var dbContextPath = context.Layout.ResolveDbContextPath();

        if (!File.Exists(dbContextPath))
        {
            ConsoleOutput.Warning($"AppDbContext.cs not found at {dbContextPath} — update manually.");
            return true;
        }

        return UpdateDbContext(dbContextPath, context.AppName, newProvider);
    }

    private static bool UpdateCsproj(string csprojPath, string newProvider)
    {
        try
        {
            var content = File.ReadAllText(csprojPath);
            var newPackage = GetPackageName(newProvider);
            var updated = RemoveOldProviderPackages(content);
            updated = EnsurePackageReference(updated, newPackage);

            File.WriteAllText(csprojPath, updated);
            ConsoleOutput.FileModified(csprojPath);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ConsoleOutput.Error($"Failed to update {csprojPath}: {ex.Message}");
            return false;
        }
    }

    private static bool UpdateDbContext(string dbContextPath, string appName, string newProvider)
    {
        try
        {
            var content = File.ReadAllText(dbContextPath);
            var connectionString = DbDockerOperations.GetConnectionString(appName, newProvider);
            var useMethod = GetUseMethodCall(newProvider, connectionString);

            var pattern = @"optionsBuilder\.(UseSqlite|UseNpgsql|UseSqlServer)\([^)]*\)";
            var updated = Regex.Replace(content, pattern, useMethod, RegexOptions.None, RegexTimeout);

            File.WriteAllText(dbContextPath, updated);
            ConsoleOutput.FileModified(dbContextPath);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ConsoleOutput.Error($"Failed to update {dbContextPath}: {ex.Message}");
            return false;
        }
    }

    private static string RemoveOldProviderPackages(string content)
    {
        string[] oldPackages =
        [
            "Microsoft.EntityFrameworkCore.Sqlite",
            "Npgsql.EntityFrameworkCore.PostgreSQL",
            "Microsoft.EntityFrameworkCore.SqlServer",
        ];

        var updated = content;

        foreach (var pkg in oldPackages)
        {
            var pattern = $"""<PackageReference Include="{pkg}"[^/]*/>\s*""";
            updated = Regex.Replace(updated, pattern, string.Empty, RegexOptions.None, RegexTimeout);
        }

        return updated;
    }

    private static string EnsurePackageReference(string content, string newPackage)
    {
        if (content.Contains(newPackage, StringComparison.OrdinalIgnoreCase))
            return content;

        var insertPattern = @"(<ItemGroup>\s*(?:<PackageReference[^/]*/>\s*)*?)(\s*</ItemGroup>)";
        var match = Regex.Match(content, insertPattern, RegexOptions.None, RegexTimeout);

        if (match.Success)
        {
            var insertion = $"""    <PackageReference Include="{newPackage}" Version="*" />{Environment.NewLine}""";
            return content[..match.Groups[2].Index] + insertion + content[match.Groups[2].Index..];
        }

        var itemGroupBlock =
            $"""

  <ItemGroup>
    <PackageReference Include="{newPackage}" Version="*" />
  </ItemGroup>
""";
        var projectEndIndex = content.LastIndexOf("</Project>", StringComparison.Ordinal);

        return projectEndIndex >= 0
            ? content[..projectEndIndex] + itemGroupBlock + content[projectEndIndex..]
            : content;
    }

    private static string GetPackageName(string provider) =>
        provider switch
        {
            "postgres" => "Npgsql.EntityFrameworkCore.PostgreSQL",
            "sqlserver" => "Microsoft.EntityFrameworkCore.SqlServer",
            _ => throw new ArgumentException($"Unsupported provider: {provider}"),
        };

    private static string GetUseMethodCall(string provider, string connectionString) =>
        provider switch
        {
            "postgres" => $"""optionsBuilder.UseNpgsql("{connectionString}")""",
            "sqlserver" => $"""optionsBuilder.UseSqlServer("{connectionString}")""",
            _ => $"""optionsBuilder.UseSqlite("{connectionString}")""",
        };
}
