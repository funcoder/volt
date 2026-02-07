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

        DbContextRegistrar.RegisterModel(context, appName, modelName);

        var baseTimestamp = DateTime.UtcNow;
        GenerateAttachmentsMigrationIfNeeded(context, appName, parsedFields, baseTimestamp);
        GenerateMigration(context, appName, modelName, parsedFields, baseTimestamp.AddSeconds(1));

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Model '{modelName}' generated.");
    }

    public static void GenerateAttachmentsMigrationIfNeeded(
        ProjectContext context, string appName, IReadOnlyList<FieldDefinition> fields, DateTime timestamp)
    {
        if (!fields.Any(f => f.IsAttachment)) return;

        var migrationsDir = context.ResolvePath("Migrations");
        if (Directory.Exists(migrationsDir))
        {
            var existingFiles = Directory.GetFiles(migrationsDir, "*CreateVoltAttachments.cs");
            if (existingFiles.Length > 0) return;
        }

        var migrationId = $"{timestamp:yyyyMMddHHmmss}_CreateVoltAttachments";
        var migrationData = new { app_name = appName, migration_id = migrationId };
        var migrationPath = context.ResolvePath(
            "Migrations", $"{migrationId}.cs");

        if (TemplateEngine.RenderToFile("Migration.Attachments", migrationData, migrationPath))
        {
            ConsoleOutput.FileCreated("Migrations/CreateVoltAttachments.cs");
        }
    }

    public static void GenerateMigration(
        ProjectContext context, string appName, string modelName, IReadOnlyList<FieldDefinition> fields,
        DateTime timestamp)
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

        var attachmentKeys = fields
            .Where(f => f.IsAttachment)
            .Select(f => new { name = f.Name })
            .ToArray();

        var migrationId = $"{timestamp:yyyyMMddHHmmss}_Create{Pluralize(modelName)}";
        var migrationData = new
        {
            app_name = appName,
            model_name = modelName,
            model_name_plural = Pluralize(modelName),
            table_name = tableName,
            migration_id = migrationId,
            fields = fields.Select(f => new
            {
                name = f.Name,
                type = f.IsAttachment ? "int" : f.Type,
                nullable = (f.Type == "string" || f.IsAttachment) ? "true" : "false",
            }).ToArray(),
            foreign_keys = foreignKeys,
            attachment_keys = attachmentKeys,
        };

        var migrationPath = context.ResolvePath(
            "Migrations", $"{migrationId}.cs");

        if (TemplateEngine.RenderToFile("Migration", migrationData, migrationPath))
        {
            ConsoleOutput.FileCreated($"Migrations/Create{Pluralize(modelName)}.cs");
        }
    }

    public static object BuildTemplateData(
        string appName, string modelName, IReadOnlyList<FieldDefinition> fields)
    {
        var fieldData = fields
            .Where(f => !f.IsReference && !f.IsAttachment)
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

        var attachments = fields
            .Where(f => f.IsAttachment)
            .Select(f => new { name = f.AttachmentName! })
            .ToArray();

        return new
        {
            app_name = appName,
            model_name = modelName,
            fields = fieldData,
            associations = associations,
            has_attachments = attachments.Length > 0,
            attachments = attachments,
        };
    }
}
