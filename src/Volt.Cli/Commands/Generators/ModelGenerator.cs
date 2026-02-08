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
        if (name.Contains(':'))
        {
            ConsoleOutput.Error($"Invalid name '{name}'. Did you forget the model name? Usage: volt generate model <Name> [fields...]");
            return;
        }

        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var appName = context.AppName;
        var parsedFields = FieldParser.Parse(fields);
        var modelName = EnsurePascalCase(name);

        var modelData = BuildTemplateData(layout, modelName, parsedFields);
        var modelPath = layout.ResolveModelPath($"{modelName}.cs");

        if (TemplateEngine.RenderToFile("Model", modelData, modelPath))
        {
            ConsoleOutput.FileCreated($"Models/{modelName}.cs");
        }

        DbContextRegistrar.RegisterModel(context, modelName);

        var baseTimestamp = DateTime.UtcNow;
        GenerateAttachmentsMigrationIfNeeded(context, parsedFields, baseTimestamp);
        GenerateMigration(context, modelName, parsedFields, baseTimestamp.AddSeconds(1));

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Model '{modelName}' generated.");
    }

    public static void GenerateAttachmentsMigrationIfNeeded(
        ProjectContext context, IReadOnlyList<FieldDefinition> fields, DateTime timestamp)
    {
        if (!fields.Any(f => f.IsAttachment)) return;

        var layout = context.Layout;
        var migrationsDir = layout.ResolveMigrationPath();
        if (Directory.Exists(migrationsDir))
        {
            var existingFiles = Directory.GetFiles(migrationsDir, "*CreateVoltAttachments.cs");
            if (existingFiles.Length > 0) return;
        }

        var migrationId = $"{timestamp:yyyyMMddHHmmss}_CreateVoltAttachments";
        var migrationData = new
        {
            data_namespace = layout.GetDataNamespace(),
            migration_namespace = layout.GetMigrationNamespace(),
            migration_id = migrationId,
        };
        var migrationPath = layout.ResolveMigrationPath($"{migrationId}.cs");

        if (TemplateEngine.RenderToFile("Migration.Attachments", migrationData, migrationPath))
        {
            ConsoleOutput.FileCreated("Migrations/CreateVoltAttachments.cs");
        }
    }

    public static void GenerateMigration(
        ProjectContext context, string modelName, IReadOnlyList<FieldDefinition> fields,
        DateTime timestamp)
    {
        var layout = context.Layout;
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
            data_namespace = layout.GetDataNamespace(),
            migration_namespace = layout.GetMigrationNamespace(),
            model_name = modelName,
            model_name_plural = Pluralize(modelName),
            table_name = tableName,
            migration_id = migrationId,
            fields = fields.Select(f => new
            {
                name = f.Name,
                column_name = ToSnakeCase(f.Name),
                type = f.IsAttachment ? "int" : f.Type,
                nullable = (f.Type == "string" || f.IsAttachment) ? "true" : "false",
            }).ToArray(),
            foreign_keys = foreignKeys.Select(f => new
            {
                name = f.name,
                column_name = ToSnakeCase(f.name),
                referenced_table = f.referenced_table,
            }).ToArray(),
            attachment_keys = attachmentKeys.Select(f => new
            {
                name = f.name,
                column_name = ToSnakeCase(f.name),
            }).ToArray(),
        };

        var migrationPath = layout.ResolveMigrationPath($"{migrationId}.cs");

        if (TemplateEngine.RenderToFile("Migration", migrationData, migrationPath))
        {
            ConsoleOutput.FileCreated($"Migrations/Create{Pluralize(modelName)}.cs");
        }
    }

    public static object BuildTemplateData(
        IProjectLayout layout, string modelName, IReadOnlyList<FieldDefinition> fields)
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
                new { name = f.Name, type = f.Type, is_navigation = false },
                new { name = f.ReferencedModel!, type = f.ReferencedModel!, is_navigation = true },
            })
            .ToArray();

        var attachments = fields
            .Where(f => f.IsAttachment)
            .Select(f => new { name = f.AttachmentName! })
            .ToArray();

        return new
        {
            model_namespace = layout.GetModelNamespace(),
            model_name = modelName,
            fields = fieldData,
            associations = associations,
            has_attachments = attachments.Length > 0,
            attachments = attachments,
        };
    }
}
