using Volt.Cli.Helpers;
using static Volt.Cli.Helpers.NamingConventions;

namespace Volt.Cli.Commands.Generators;

/// <summary>
/// Generates AI assistant context files (CLAUDE.md, .cursorrules, copilot-instructions.md)
/// by scanning the project's Models/ and Controllers/ directories.
/// </summary>
public static class AiContextGenerator
{
    public static void Generate()
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var appName = context.GetAppName();
        var models = DiscoverModels(context);
        var routes = DiscoverRoutes(context);

        var data = new
        {
            app_name = appName,
            models = models,
            routes = routes,
        };

        GenerateFile(context, "AiContext.Claude", data, "CLAUDE.md");
        GenerateFile(context, "AiContext.Cursor", data, ".cursorrules");
        GenerateCopilotFile(context, data);

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success("AI context files generated.");
    }

    private static void GenerateFile(
        ProjectContext context, string templateName, object data, string fileName)
    {
        var outputPath = context.ResolvePath(fileName);

        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        var content = TemplateEngine.Render(templateName, data);
        var directory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, content);
        ConsoleOutput.FileCreated(fileName);
    }

    private static void GenerateCopilotFile(ProjectContext context, object data)
    {
        var outputPath = context.ResolvePath(".github", "copilot-instructions.md");
        var directory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }

        var content = TemplateEngine.Render("AiContext.Copilot", data);
        File.WriteAllText(outputPath, content);
        ConsoleOutput.FileCreated(".github/copilot-instructions.md");
    }

    private static object[] DiscoverModels(ProjectContext context)
    {
        var modelsDir = context.ResolvePath("Models");
        if (!Directory.Exists(modelsDir)) return [];

        var modelFiles = Directory.GetFiles(modelsDir, "*.cs");
        var models = new List<object>();

        foreach (var file in modelFiles)
        {
            var modelName = Path.GetFileNameWithoutExtension(file);
            if (modelName == "Model") continue;

            var tableName = ToTableName(modelName);
            var fields = ExtractProperties(file);

            models.Add(new
            {
                name = modelName,
                table = tableName,
                fields = fields,
            });
        }

        return models.ToArray();
    }

    private static object[] DiscoverRoutes(ProjectContext context)
    {
        var controllersDir = context.ResolvePath("Controllers");
        if (!Directory.Exists(controllersDir)) return [];

        var controllerFiles = Directory.GetFiles(controllersDir, "*Controller.cs");
        var routes = new List<object>();

        foreach (var file in controllerFiles)
        {
            var controllerName = Path.GetFileNameWithoutExtension(file);
            if (controllerName == "HomeController") continue;

            var modelName = controllerName.Replace("Controller", "");
            var routePath = "/" + ToTableName(
                modelName.EndsWith("s", StringComparison.Ordinal)
                    ? modelName
                    : modelName);

            routes.Add(new
            {
                path = routePath,
                controller = controllerName,
            });
        }

        return routes.ToArray();
    }

    private static string ExtractProperties(string filePath)
    {
        try
        {
            var lines = File.ReadAllLines(filePath);
            var props = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("public ", StringComparison.Ordinal)
                    && trimmed.Contains("{ get;", StringComparison.Ordinal)
                    && !trimmed.Contains("class ", StringComparison.Ordinal))
                {
                    var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 3)
                    {
                        var propName = parts[2];
                        var propType = parts[1];
                        if (propName != "Id" && propName != "CreatedAt"
                            && propName != "UpdatedAt" && propName != "DeletedAt")
                        {
                            props.Add($"{propName}:{propType}");
                        }
                    }
                }
            }

            return props.Count > 0 ? string.Join(", ", props) : "no custom fields";
        }
        catch
        {
            return "unable to read";
        }
    }
}
