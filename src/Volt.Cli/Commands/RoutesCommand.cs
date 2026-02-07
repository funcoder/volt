using System.CommandLine;
using System.Text.RegularExpressions;
using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Lists all registered routes in a Volt project by scanning controller files
/// and route registrations.
/// </summary>
public static partial class RoutesCommand
{
    /// <summary>
    /// Creates the <c>volt routes</c> command definition.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("routes", "List all registered routes");

        command.SetAction((_, _) =>
        {
            Execute();
            return Task.CompletedTask;
        });

        return command;
    }

    private static void Execute()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Info($"Routes for {context.GetAppName()}");
        ConsoleOutput.BlankLine();

        var routes = CollectRoutes(context);

        if (routes.Count == 0)
        {
            ConsoleOutput.Warning("No routes found. Create a controller with: volt generate controller <name>");
            return;
        }

        var headers = new[] { "Method", "Path", "Controller#Action" };
        var rows = routes
            .Select(r => new[] { r.Method, r.Path, r.Action })
            .ToList();

        ConsoleOutput.Table(headers, rows);
    }

    private static List<RouteEntry> CollectRoutes(ProjectContext context)
    {
        var routes = new List<RouteEntry>();

        var routeRegistrationPath = FindRouteRegistration(context);
        if (routeRegistrationPath is not null)
        {
            routes.AddRange(ParseRouteRegistration(routeRegistrationPath));
        }

        if (routes.Count == 0)
        {
            routes.AddRange(ScanControllers(context));
        }

        return routes;
    }

    private static string? FindRouteRegistration(ProjectContext context)
    {
        var candidates = new[]
        {
            context.ResolvePath("VoltRouteRegistration.cs"),
            context.ResolvePath("Routes.cs"),
        };

        return candidates.FirstOrDefault(File.Exists);
    }

    private static IEnumerable<RouteEntry> ParseRouteRegistration(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var matches = MapVoltResourcesPattern().Matches(content);

        foreach (Match match in matches)
        {
            var controllerName = match.Groups[1].Value.Replace("Controller", string.Empty);
            var resource = controllerName.ToLowerInvariant();

            yield return new RouteEntry("GET", $"/{resource}", $"{controllerName}#Index");
            yield return new RouteEntry("GET", $"/{resource}/new", $"{controllerName}#New");
            yield return new RouteEntry("POST", $"/{resource}", $"{controllerName}#Create");
            yield return new RouteEntry("GET", $"/{resource}/{{id}}", $"{controllerName}#Show");
            yield return new RouteEntry("GET", $"/{resource}/{{id}}/edit", $"{controllerName}#Edit");
            yield return new RouteEntry("PUT", $"/{resource}/{{id}}", $"{controllerName}#Update");
            yield return new RouteEntry("DELETE", $"/{resource}/{{id}}", $"{controllerName}#Destroy");
        }
    }

    private static IEnumerable<RouteEntry> ScanControllers(ProjectContext context)
    {
        var controllersDir = context.ResolvePath("Controllers");

        if (!Directory.Exists(controllersDir))
        {
            yield break;
        }

        var controllerFiles = Directory.GetFiles(controllersDir, "*Controller.cs");

        foreach (var file in controllerFiles)
        {
            var content = File.ReadAllText(file);
            var controllerName = Path.GetFileNameWithoutExtension(file)
                .Replace("Controller", string.Empty);

            var isResource = content.Contains("ResourceController<", StringComparison.Ordinal)
                || content.Contains("ApiResourceController<", StringComparison.Ordinal);

            if (isResource)
            {
                var resource = controllerName.ToLowerInvariant();
                yield return new RouteEntry("GET", $"/{resource}", $"{controllerName}#Index");
                yield return new RouteEntry("GET", $"/{resource}/new", $"{controllerName}#New");
                yield return new RouteEntry("POST", $"/{resource}", $"{controllerName}#Create");
                yield return new RouteEntry("GET", $"/{resource}/{{id}}", $"{controllerName}#Show");
                yield return new RouteEntry("GET", $"/{resource}/{{id}}/edit", $"{controllerName}#Edit");
                yield return new RouteEntry("PUT", $"/{resource}/{{id}}", $"{controllerName}#Update");
                yield return new RouteEntry("DELETE", $"/{resource}/{{id}}", $"{controllerName}#Destroy");
            }
            else
            {
                var actionMatches = HttpMethodAttributePattern().Matches(content);
                foreach (Match actionMatch in actionMatches)
                {
                    var method = actionMatch.Groups[1].Value.Replace("Http", string.Empty).ToUpperInvariant();
                    var actionName = ExtractActionName(content, actionMatch.Index);
                    yield return new RouteEntry(
                        method,
                        $"/{controllerName.ToLowerInvariant()}",
                        $"{controllerName}#{actionName}");
                }
            }
        }
    }

    private static string ExtractActionName(string content, int attributeIndex)
    {
        var afterAttribute = content[attributeIndex..];
        var methodMatch = PublicMethodPattern().Match(afterAttribute);
        return methodMatch.Success ? methodMatch.Groups[1].Value : "Unknown";
    }

    [GeneratedRegex(@"MapVoltResources<(\w+)>")]
    private static partial Regex MapVoltResourcesPattern();

    [GeneratedRegex(@"\[Http(Get|Post|Put|Delete|Patch)")]
    private static partial Regex HttpMethodAttributePattern();

    [GeneratedRegex(@"public\s+(?:virtual\s+)?(?:async\s+)?(?:\w+(?:<\w+>)?)\s+(\w+)\s*\(")]
    private static partial Regex PublicMethodPattern();

    private sealed record RouteEntry(string Method, string Path, string Action);
}
