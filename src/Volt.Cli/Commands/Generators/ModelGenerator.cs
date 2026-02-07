using Volt.Cli.Helpers;
using static Volt.Cli.Helpers.NamingConventions;

namespace Volt.Cli.Commands.Generators;

/// <summary>
/// Generates model classes and their associated EF Core migrations.
/// </summary>
public static class ModelGenerator
{
    public static void Generate(string name, string[] fields)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var appName = context.GetAppName();
        var parsedFields = FieldParser.Parse(fields);
        var modelName = EnsurePascalCase(name);

        var modelData = BuildTemplateData(appName, modelName, parsedFields);
        var modelPath = context.ResolvePath("Models", $"{modelName}.cs");

        if (TemplateEngine.RenderToFile("Model", modelData, modelPath))
        {
            ConsoleOutput.FileCreated($"Models/{modelName}.cs");
        }

        GenerateMigration(context, appName, modelName, parsedFields);

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Model '{modelName}' generated.");
    }

    public static void GenerateMigration(
        ProjectContext context, string appName, string modelName, IReadOnlyList<FieldDefinition> fields)
    {
        var tableName = ToTableName(modelName);
        var foreignKeys = fields
            .Where(f => f.IsReference)
            .Select(f => new
            {
                name = f.Name,
                referenced_table = ToTableName(f.ReferencedModel!),
            })
            .ToArray();

        var migrationData = new
        {
            app_name = appName,
            model_name = modelName,
            model_name_plural = Pluralize(modelName),
            table_name = tableName,
            fields = fields.Select(f => new
            {
                name = f.Name,
                type = f.Type,
                nullable = f.Type == "string" ? "true" : "false",
            }).ToArray(),
            foreign_keys = foreignKeys,
        };

        var migrationPath = context.ResolvePath(
            "Migrations", $"{DateTime.UtcNow:yyyyMMddHHmmss}_Create{Pluralize(modelName)}.cs");

        if (TemplateEngine.RenderToFile("Migration", migrationData, migrationPath))
        {
            ConsoleOutput.FileCreated($"Migrations/Create{Pluralize(modelName)}.cs");
        }
    }

    public static object BuildTemplateData(
        string appName, string modelName, IReadOnlyList<FieldDefinition> fields)
    {
        var fieldData = fields
            .Where(f => !f.IsReference)
            .Select(f => new
            {
                name = f.Name,
                type = f.Type,
                @default = f.Type == "string" ? "\"\"" : (string?)null,
            })
            .ToArray();

        var associations = fields
            .Where(f => f.IsReference)
            .SelectMany(f => new[]
            {
                new { name = f.Name, type = f.Type },
                new { name = f.ReferencedModel!, type = f.ReferencedModel! },
            })
            .ToArray();

        return new
        {
            app_name = appName,
            model_name = modelName,
            fields = fieldData,
            associations = associations,
        };
    }
}
