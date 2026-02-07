using Volt.Cli.Helpers;
using static Volt.Cli.Helpers.NamingConventions;

namespace Volt.Cli.Commands.Generators;

/// <summary>
/// Generates a complete CRUD scaffold: model, resource controller, views, migration, and tests.
/// </summary>
public static class ScaffoldGenerator
{
    public static void Generate(string name, string[] fields)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var appName = context.GetAppName();
        var parsedFields = FieldParser.Parse(fields);
        var modelName = EnsurePascalCase(name);
        var controllerName = $"{Pluralize(modelName)}Controller";
        var routePath = ToRoutePath(modelName);

        ConsoleOutput.Info($"Scaffolding '{modelName}'...");
        ConsoleOutput.BlankLine();

        GenerateModel(context, appName, modelName, parsedFields);
        GenerateResourceController(context, appName, modelName, controllerName, parsedFields);
        GenerateViews(context, appName, modelName, parsedFields, routePath);
        var baseTimestamp = DateTime.UtcNow;
        ModelGenerator.GenerateAttachmentsMigrationIfNeeded(context, appName, parsedFields, baseTimestamp);
        ModelGenerator.GenerateMigration(context, appName, modelName, parsedFields, baseTimestamp.AddSeconds(1));
        GenerateTests(context, appName, modelName, controllerName, parsedFields, routePath);

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Scaffold for '{modelName}' generated.");
    }

    private static void GenerateModel(
        ProjectContext context, string appName, string modelName, IReadOnlyList<FieldDefinition> fields)
    {
        var modelData = ModelGenerator.BuildTemplateData(appName, modelName, fields);
        var modelPath = context.ResolvePath("Models", $"{modelName}.cs");

        if (TemplateEngine.RenderToFile("Model", modelData, modelPath))
        {
            ConsoleOutput.FileCreated($"Models/{modelName}.cs");
        }
    }

    private static void GenerateResourceController(
        ProjectContext context, string appName, string modelName, string controllerName,
        IReadOnlyList<FieldDefinition> fields)
    {
        var hasAttachments = fields.Any(f => f.IsAttachment);

        var permittedFields = fields
            .Where(f => !f.IsAttachment)
            .Select(f => new { name = f.Name, type = f.Type })
            .ToArray();

        var attachments = fields
            .Where(f => f.IsAttachment)
            .Select(f => new
            {
                name = f.AttachmentName!,
                form_name = char.ToLowerInvariant(f.AttachmentName![0]) + f.AttachmentName[1..],
            })
            .ToArray();

        var data = new
        {
            app_name = appName,
            controller_name = controllerName,
            model_name = modelName,
            fields = fields.Select(f => new { name = f.Name, type = f.Type }).ToArray(),
            permitted_fields = permittedFields,
            has_attachments = hasAttachments,
            attachments = attachments,
        };

        var outputPath = context.ResolvePath("Controllers", $"{controllerName}.cs");

        if (TemplateEngine.RenderToFile("ResourceController", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Controllers/{controllerName}.cs");
        }
    }

    private static void GenerateViews(
        ProjectContext context, string appName, string modelName,
        IReadOnlyList<FieldDefinition> fields, string routePath)
    {
        var viewFolder = Pluralize(modelName);
        var hasAttachments = fields.Any(f => f.IsAttachment);

        var fieldData = fields.Select(f => new
        {
            name = f.Name,
            type = f.RawType,
            is_attachment = f.IsAttachment,
            is_image_attachment = f.IsImageAttachment,
            attachment_name = f.AttachmentName ?? f.Name,
        }).ToArray();

        var viewModel = new
        {
            app_name = appName,
            model_name = modelName,
            model_name_plural = Pluralize(modelName),
            route_path = routePath,
            fields = fieldData,
            has_attachments = hasAttachments,
        };

        var viewTemplates = new[] { "View.Index", "View.Show", "View.Form" };
        var viewFileNames = new[] { "Index.cshtml", "Show.cshtml", "_Form.cshtml" };

        for (var i = 0; i < viewTemplates.Length; i++)
        {
            var outputPath = context.ResolvePath("Views", viewFolder, viewFileNames[i]);

            if (TemplateEngine.RenderToFile(viewTemplates[i], viewModel, outputPath))
            {
                ConsoleOutput.FileCreated($"Views/{viewFolder}/{viewFileNames[i]}");
            }
        }
    }

    private static void GenerateTests(
        ProjectContext context, string appName, string modelName, string controllerName,
        IReadOnlyList<FieldDefinition> fields, string routePath)
    {
        if (!HasTestProject(context))
        {
            ConsoleOutput.Warning("No test project found. Skipping test generation.");
            return;
        }

        var nonAttachmentFields = fields.Where(f => !f.IsAttachment).ToArray();

        var fieldData = nonAttachmentFields.Select(f => new
        {
            name = f.Name,
            type = f.Type,
            default_value = GetDefaultTestValue(f.Type),
        }).ToArray();

        var modelTestData = new
        {
            app_name = appName,
            model_name = modelName,
            fields = fieldData,
        };

        var modelTestPath = context.ResolvePath("Tests", "Models", $"{modelName}Test.cs");

        if (TemplateEngine.RenderToFile("Test.Model", modelTestData, modelTestPath))
        {
            ConsoleOutput.FileCreated($"Tests/Models/{modelName}Test.cs");
        }

        var controllerTestData = new
        {
            app_name = appName,
            controller_name = controllerName,
            model_name = modelName,
            route_path = routePath,
            fields = fieldData,
        };

        var controllerTestPath = context.ResolvePath("Tests", "Controllers", $"{controllerName}Test.cs");

        if (TemplateEngine.RenderToFile("Test.Controller", controllerTestData, controllerTestPath))
        {
            ConsoleOutput.FileCreated($"Tests/Controllers/{controllerName}Test.cs");
        }
    }

    private static bool HasTestProject(ProjectContext context)
    {
        var testsDir = context.ResolvePath("Tests");
        if (!Directory.Exists(testsDir)) return false;

        var testCsprojs = Directory.GetFiles(testsDir, "*.Tests.csproj");
        return testCsprojs.Length > 0;
    }
}
