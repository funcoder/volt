using System.Text.RegularExpressions;
using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Docker container lifecycle management for database providers.
/// Handles spinning up, tearing down, and inspecting Docker containers
/// for PostgreSQL and SQL Server, including automatic provider switching.
/// </summary>
public static class DbDockerOperations
{
    private const string PostgresImage = "postgres:17";
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string PostgresUser = "postgres";
    private const string DefaultPostgresPassword = "volt_dev";
    private const string DefaultSqlServerPassword = "Volt_Dev123!";
    private static readonly Regex ValidContainerName = new(@"^[a-z0-9][a-z0-9_.-]*$");

    internal static string PostgresPassword =>
        Environment.GetEnvironmentVariable("VOLT_POSTGRES_PASSWORD") ?? DefaultPostgresPassword;

    internal static string SqlServerPassword =>
        Environment.GetEnvironmentVariable("VOLT_SQLSERVER_PASSWORD") ?? DefaultSqlServerPassword;

    public static async Task Up(string? provider)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        if (!RequireDocker()) return;

        var currentProvider = context.GetDatabaseProvider();
        var targetProvider = ResolveTargetProvider(provider, currentProvider);
        if (targetProvider is null) return;

        if (currentProvider != targetProvider)
        {
            ConsoleOutput.Info($"Switching provider from {currentProvider} to {targetProvider}...");
            ConsoleOutput.BlankLine();

            if (!DbProviderSwitcher.Switch(context, targetProvider))
            {
                ConsoleOutput.Error("Provider switch failed.");
                return;
            }

            var restoreCode = await ProcessRunner.RunAsync(
                "dotnet", "restore", context.Layout.GetSolutionOrProjectRoot());

            if (restoreCode != 0)
            {
                ConsoleOutput.Error("dotnet restore failed after provider switch.");
                return;
            }

            ConsoleOutput.Success($"Provider switched from {currentProvider} to {targetProvider}.");
            ConsoleOutput.BlankLine();
        }

        var containerName = GetContainerName(context.AppName, targetProvider);
        if (containerName is null) return;

        var existing = await FindRunningContainer(containerName);

        if (existing is not null)
        {
            ConsoleOutput.Success($"Container already running: {containerName}");
            PrintConnectionInfo(context.AppName, targetProvider);
            return;
        }

        ConsoleOutput.Info($"Starting {targetProvider} container: {containerName}...");

        var dockerArgs = BuildDockerRunArgs(context.AppName, targetProvider, containerName);
        var exitCode = await ProcessRunner.RunAsync("docker", dockerArgs);

        if (exitCode != 0)
        {
            ConsoleOutput.Error("Failed to start Docker container.");
            return;
        }

        ConsoleOutput.Info("Waiting for database to be ready...");
        var ready = await WaitForReady(containerName, targetProvider);

        if (!ready)
        {
            ConsoleOutput.Warning(
                "Container started but readiness check timed out. It may still be initializing.");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"{GetProviderDisplayName(targetProvider)} container started: {containerName}");
        PrintConnectionInfo(context.AppName, targetProvider);

        if (currentProvider != targetProvider)
        {
            ConsoleOutput.BlankLine();
            ConsoleOutput.Warning("Delete existing migrations and run: volt db migrate");
        }
    }

    public static async Task Down()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        if (!RequireDocker()) return;

        var containerName = await FindProjectContainer(context.AppName);

        if (containerName is null)
        {
            ConsoleOutput.Info("No running Volt database container found for this project.");
            return;
        }

        ConsoleOutput.Info($"Stopping container: {containerName}...");

        var stopCode = await ProcessRunner.RunAsync("docker", $"stop {containerName}");
        if (stopCode != 0)
        {
            ConsoleOutput.Error($"Failed to stop container: {containerName}");
            return;
        }

        var rmCode = await ProcessRunner.RunAsync("docker", $"rm {containerName}");
        if (rmCode != 0)
        {
            ConsoleOutput.Error($"Failed to remove container: {containerName}");
            return;
        }

        ConsoleOutput.Success($"Container stopped and removed: {containerName}");
    }

    public static async Task Status()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        if (!RequireDocker()) return;

        var prefix = GetContainerNamePrefix(context.AppName);

        ConsoleOutput.Info($"Docker containers for {context.AppName}:");
        ConsoleOutput.BlankLine();

        var format = """table {{.Names}}\t{{.Status}}\t{{.Ports}}""";
        await ProcessRunner.RunAsync(
            "docker",
            $"ps -a --filter name={prefix} --format \"{format}\"");
    }

    public static async Task Logs()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        if (!RequireDocker()) return;

        var containerName = await FindProjectContainer(context.AppName);

        if (containerName is null)
        {
            ConsoleOutput.Info("No running Volt database container found for this project.");
            return;
        }

        ConsoleOutput.Info($"Tailing logs for: {containerName}");
        ConsoleOutput.BlankLine();

        await ProcessRunner.RunInteractiveAsync("docker", $"logs --tail 50 -f {containerName}");
    }

    internal static string GetConnectionString(string appName, string provider) =>
        provider switch
        {
            "postgres" =>
                $"Host=localhost;Port=5432;Database={appName}_development;Username={PostgresUser};Password={PostgresPassword}",
            "sqlserver" =>
                $"Server=localhost,1433;Database={appName}_development;User Id=sa;Password={SqlServerPassword};TrustServerCertificate=true",
            _ =>
                $"Data Source={appName}.db",
        };

    internal static string GetProviderDisplayName(string provider) =>
        provider switch
        {
            "postgres" => "PostgreSQL",
            "sqlserver" => "SQL Server",
            _ => provider,
        };

    private static bool RequireDocker()
    {
        if (ProcessRunner.IsCommandAvailable("docker")) return true;

        ConsoleOutput.Error("Docker is not installed or not on PATH.");
        ConsoleOutput.Info("Install Docker Desktop: https://www.docker.com/products/docker-desktop");
        return false;
    }

    private static string? ResolveTargetProvider(string? requested, string current)
    {
        if (requested is not null)
        {
            var normalized = requested.ToLowerInvariant();

            if (normalized == "sqlite")
            {
                ConsoleOutput.Error("SQLite doesn't need Docker — it runs as a local file.");
                return null;
            }

            if (normalized is not "postgres" and not "sqlserver")
            {
                ConsoleOutput.Error($"Invalid provider '{requested}'. Valid options: postgres, sqlserver");
                return null;
            }

            return normalized;
        }

        if (current is "postgres" or "sqlserver")
        {
            return current;
        }

        ConsoleOutput.Error("Current provider is sqlite. Specify a provider: volt db docker up postgres");
        ConsoleOutput.Info("Valid providers: postgres, sqlserver");
        return null;
    }

    private static string? GetContainerName(string appName, string provider)
    {
        var snakeName = NamingConventions.ToSnakeCase(appName).ToLowerInvariant();
        var containerName = $"{snakeName}_{provider}_dev";

        if (!ValidContainerName.IsMatch(containerName))
        {
            ConsoleOutput.Error(
                $"Cannot create a valid Docker container name from app name '{appName}'. " +
                "App name must start with a letter and contain only alphanumeric characters.");
            return null;
        }

        return containerName;
    }

    private static string GetContainerNamePrefix(string appName) =>
        NamingConventions.ToSnakeCase(appName).ToLowerInvariant();

    private static async Task<string?> FindRunningContainer(string containerName)
    {
        var (exitCode, output) = await ProcessRunner.RunCapturedAsync(
            "docker", $"ps -q --filter name=^{containerName}$");

        return exitCode == 0 && !string.IsNullOrWhiteSpace(output) ? containerName : null;
    }

    private static async Task<string?> FindProjectContainer(string appName)
    {
        var prefix = GetContainerNamePrefix(appName);

        var (exitCode, output) = await ProcessRunner.RunCapturedAsync(
            "docker", $"ps -q --filter name={prefix}");

        if (exitCode != 0 || string.IsNullOrWhiteSpace(output)) return null;

        var namesFormat = "{{.Names}}";
        var (_, nameOutput) = await ProcessRunner.RunCapturedAsync(
            "docker", $"ps --filter name={prefix} --format \"{namesFormat}\"");

        var name = nameOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }

    private static string BuildDockerRunArgs(string appName, string provider, string containerName) =>
        provider switch
        {
            "postgres" =>
                $"run -d --name {containerName} " +
                $"-p 5432:5432 " +
                $"-e POSTGRES_USER={PostgresUser} " +
                $"-e POSTGRES_PASSWORD={PostgresPassword} " +
                $"-e POSTGRES_DB={appName}_development " +
                PostgresImage,
            "sqlserver" =>
                $"run -d --name {containerName} " +
                $"-p 1433:1433 " +
                $"-e ACCEPT_EULA=Y " +
                $"-e \"MSSQL_SA_PASSWORD={SqlServerPassword}\" " +
                SqlServerImage,
            _ => throw new ArgumentException($"Unsupported provider: {provider}"),
        };

    private static async Task<bool> WaitForReady(string containerName, string provider)
    {
        const int maxAttempts = 30;
        const int delayMs = 1000;

        for (var i = 0; i < maxAttempts; i++)
        {
            await Task.Delay(delayMs);

            var ready = provider switch
            {
                "postgres" => await CheckPostgresReady(containerName),
                "sqlserver" => await CheckSqlServerReady(containerName),
                _ => false,
            };

            if (ready) return true;
        }

        return false;
    }

    private static async Task<bool> CheckPostgresReady(string containerName)
    {
        var (exitCode, _) = await ProcessRunner.RunCapturedAsync(
            "docker", $"exec {containerName} pg_isready -U {PostgresUser}");

        return exitCode == 0;
    }

    private static async Task<bool> CheckSqlServerReady(string containerName)
    {
        var (exitCode, _) = await ProcessRunner.RunCapturedAsync(
            "docker",
            $"""exec {containerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "{SqlServerPassword}" -Q "SELECT 1" """);

        if (exitCode == 0) return true;

        var (exitCode2, _) = await ProcessRunner.RunCapturedAsync(
            "docker",
            $"""exec {containerName} /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "{SqlServerPassword}" -C -Q "SELECT 1" """);

        return exitCode2 == 0;
    }

    private static void PrintConnectionInfo(string appName, string provider)
    {
        var connectionString = GetConnectionString(appName, provider);
        ConsoleOutput.Info($"Connection: {connectionString}");
    }
}
