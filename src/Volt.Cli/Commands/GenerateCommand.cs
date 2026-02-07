using System.CommandLine;
using Volt.Cli.Helpers;

namespace Volt.Cli.Commands;

/// <summary>
/// Generates code artifacts (models, controllers, scaffolds, migrations, jobs, mailers, channels)
/// using Scriban templates and Volt conventions.
/// </summary>
public static class GenerateCommand
{
    /// <summary>
    /// Creates the <c>volt generate</c> command with all subcommands.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("generate", "Generate code artifacts");
        command.Aliases.Add("g");

        command.Add(CreateModelSubcommand());
        command.Add(CreateControllerSubcommand());
        command.Add(CreateScaffoldSubcommand());
        command.Add(CreateMigrationSubcommand());
        command.Add(CreateJobSubcommand());
        command.Add(CreateMailerSubcommand());
        command.Add(CreateChannelSubcommand());

        return command;
    }

    private static Command CreateModelSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The model name (PascalCase)" };
        var fieldsArg = new Argument<string[]>("fields")
        {
            Description = "Field definitions as name:type pairs",
            Arity = ArgumentArity.ZeroOrMore,
        };

        var cmd = new Command("model", "Generate a model class and migration");
        cmd.Add(nameArg);
        cmd.Add(fieldsArg);

        cmd.SetAction((parseResult, _) =>
        {
            GenerateModel(
                parseResult.GetValue(nameArg)!,
                parseResult.GetValue(fieldsArg) ?? []);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateControllerSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The controller name (PascalCase)" };
        var fieldsArg = new Argument<string[]>("fields")
        {
            Description = "Field definitions as name:type pairs",
            Arity = ArgumentArity.ZeroOrMore,
        };

        var cmd = new Command("controller", "Generate a controller class");
        cmd.Add(nameArg);
        cmd.Add(fieldsArg);

        cmd.SetAction((parseResult, _) =>
        {
            GenerateController(
                parseResult.GetValue(nameArg)!,
                parseResult.GetValue(fieldsArg) ?? []);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateScaffoldSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The resource name (PascalCase)" };
        var fieldsArg = new Argument<string[]>("fields")
        {
            Description = "Field definitions as name:type pairs",
            Arity = ArgumentArity.ZeroOrMore,
        };

        var cmd = new Command("scaffold", "Generate model, controller, views, and tests");
        cmd.Add(nameArg);
        cmd.Add(fieldsArg);

        cmd.SetAction((parseResult, _) =>
        {
            GenerateScaffold(
                parseResult.GetValue(nameArg)!,
                parseResult.GetValue(fieldsArg) ?? []);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateMigrationSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The migration name" };

        var cmd = new Command("migration", "Generate a database migration");
        cmd.Add(nameArg);

        cmd.SetAction(async (parseResult, _) =>
        {
            await GenerateMigrationAsync(parseResult.GetValue(nameArg)!);
        });

        return cmd;
    }

    private static Command CreateJobSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The job name (PascalCase)" };

        var cmd = new Command("job", "Generate a background job class");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            GenerateJob(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateMailerSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The mailer name (PascalCase)" };

        var cmd = new Command("mailer", "Generate a mailer class");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            GenerateMailer(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateChannelSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The channel name (PascalCase)" };

        var cmd = new Command("channel", "Generate a real-time channel class");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            GenerateChannel(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static void GenerateModel(string name, string[] fields)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var appName = context.GetAppName();
        var parsedFields = FieldParser.Parse(fields);
        var modelName = EnsurePascalCase(name);

        var modelData = BuildModelTemplateData(appName, modelName, parsedFields);
        var modelPath = context.ResolvePath("Models", $"{modelName}.cs");

        if (TemplateEngine.RenderToFile("Model", modelData, modelPath))
        {
            ConsoleOutput.FileCreated($"Models/{modelName}.cs");
        }

        GenerateMigrationForModel(context, appName, modelName, parsedFields);

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Model '{modelName}' generated.");
    }

    private static void GenerateController(string name, string[] fields)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var appName = context.GetAppName();
        var parsedFields = FieldParser.Parse(fields);
        var controllerName = EnsureControllerSuffix(name);
        var modelName = controllerName.Replace("Controller", string.Empty);
        var routePath = ToRoutePath(modelName);

        var data = new
        {
            app_name = appName,
            controller_name = controllerName,
            model_name = modelName,
            model_name_plural = Pluralize(modelName),
            route_path = routePath,
            has_model = parsedFields.Count > 0,
            fields = parsedFields.Select(f => new { name = f.Name, type = f.Type }).ToArray(),
        };

        var outputPath = context.ResolvePath("Controllers", $"{controllerName}.cs");

        if (TemplateEngine.RenderToFile("Controller", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Controllers/{controllerName}.cs");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Controller '{controllerName}' generated.");
    }

    private static void GenerateScaffold(string name, string[] fields)
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

        GenerateScaffoldModel(context, appName, modelName, parsedFields);
        GenerateScaffoldResourceController(context, appName, modelName, controllerName, parsedFields);
        GenerateScaffoldViews(context, appName, modelName, parsedFields, routePath);
        GenerateMigrationForModel(context, appName, modelName, parsedFields);
        GenerateScaffoldTests(context, appName, modelName, controllerName, parsedFields, routePath);

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Scaffold for '{modelName}' generated.");
    }

    private static void GenerateScaffoldModel(
        ProjectContext context, string appName, string modelName, IReadOnlyList<FieldDefinition> fields)
    {
        var modelData = BuildModelTemplateData(appName, modelName, fields);
        var modelPath = context.ResolvePath("Models", $"{modelName}.cs");

        if (TemplateEngine.RenderToFile("Model", modelData, modelPath))
        {
            ConsoleOutput.FileCreated($"Models/{modelName}.cs");
        }
    }

    private static void GenerateScaffoldResourceController(
        ProjectContext context, string appName, string modelName, string controllerName,
        IReadOnlyList<FieldDefinition> fields)
    {
        var data = new
        {
            app_name = appName,
            controller_name = controllerName,
            model_name = modelName,
            fields = fields.Select(f => new { name = f.Name, type = f.Type }).ToArray(),
        };

        var outputPath = context.ResolvePath("Controllers", $"{controllerName}.cs");

        if (TemplateEngine.RenderToFile("ResourceController", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Controllers/{controllerName}.cs");
        }
    }

    private static void GenerateScaffoldViews(
        ProjectContext context, string appName, string modelName,
        IReadOnlyList<FieldDefinition> fields, string routePath)
    {
        var viewFolder = Pluralize(modelName);
        var fieldData = fields.Select(f => new
        {
            name = f.Name,
            type = f.RawType,
        }).ToArray();

        var viewModel = new
        {
            app_name = appName,
            model_name = modelName,
            model_name_plural = Pluralize(modelName),
            route_path = routePath,
            fields = fieldData,
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

    private static void GenerateScaffoldTests(
        ProjectContext context, string appName, string modelName, string controllerName,
        IReadOnlyList<FieldDefinition> fields, string routePath)
    {
        var fieldData = fields.Select(f => new
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

    private static async Task GenerateMigrationAsync(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        ConsoleOutput.Info($"Generating migration: {name}");

        var exitCode = await ProcessRunner.RunAsync(
            "dotnet", $"ef migrations add {name}", context.ProjectRoot);

        if (exitCode == 0)
        {
            ConsoleOutput.Success($"Migration '{name}' created.");
        }
        else
        {
            ConsoleOutput.Error("Failed to create migration. Is dotnet-ef tool installed?");
        }
    }

    private static void GenerateJob(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var appName = context.GetAppName();
        var jobName = EnsurePascalCase(name);

        var data = new
        {
            app_name = appName,
            job_name = jobName,
            fields = Array.Empty<object>(),
        };

        var outputPath = context.ResolvePath("Jobs", $"{jobName}.cs");

        if (TemplateEngine.RenderToFile("Job", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Jobs/{jobName}.cs");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Job '{jobName}' generated.");
    }

    private static void GenerateMailer(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var appName = context.GetAppName();
        var mailerName = EnsureMailerSuffix(name);

        var data = new
        {
            app_name = appName,
            mailer_name = mailerName,
        };

        var outputPath = context.ResolvePath("Mailers", $"{mailerName}.cs");

        if (TemplateEngine.RenderToFile("Mailer", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Mailers/{mailerName}.cs");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Mailer '{mailerName}' generated.");
    }

    private static void GenerateChannel(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var appName = context.GetAppName();
        var channelName = EnsureChannelSuffix(name);
        var channelRoute = channelName
            .Replace("Channel", string.Empty)
            .ToLowerInvariant();

        var data = new
        {
            app_name = appName,
            channel_name = channelName,
            channel_route = channelRoute,
        };

        var outputPath = context.ResolvePath("Channels", $"{channelName}.cs");

        if (TemplateEngine.RenderToFile("Channel", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Channels/{channelName}.cs");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Channel '{channelName}' generated.");
    }

    private static void GenerateMigrationForModel(
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

    private static object BuildModelTemplateData(
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

    private static string EnsurePascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private static string EnsureControllerSuffix(string name)
    {
        var pascalName = EnsurePascalCase(name);
        return pascalName.EndsWith("Controller", StringComparison.Ordinal)
            ? pascalName
            : $"{pascalName}Controller";
    }

    private static string EnsureMailerSuffix(string name)
    {
        var pascalName = EnsurePascalCase(name);
        return pascalName.EndsWith("Mailer", StringComparison.Ordinal)
            ? pascalName
            : $"{pascalName}Mailer";
    }

    private static string EnsureChannelSuffix(string name)
    {
        var pascalName = EnsurePascalCase(name);
        return pascalName.EndsWith("Channel", StringComparison.Ordinal)
            ? pascalName
            : $"{pascalName}Channel";
    }

    private static string ToRoutePath(string modelName)
    {
        return ToTableName(modelName);
    }

    private static string ToTableName(string modelName)
    {
        return Pluralize(ToSnakeCase(modelName));
    }

    private static string Pluralize(string singular)
    {
        if (string.IsNullOrEmpty(singular)) return singular;

        if (singular.EndsWith("s", StringComparison.Ordinal)
            || singular.EndsWith("x", StringComparison.Ordinal)
            || singular.EndsWith("z", StringComparison.Ordinal)
            || singular.EndsWith("sh", StringComparison.Ordinal)
            || singular.EndsWith("ch", StringComparison.Ordinal))
        {
            return $"{singular}es";
        }

        if (singular.EndsWith('y') && singular.Length > 1 && !IsVowel(singular[^2]))
        {
            return $"{singular[..^1]}ies";
        }

        return $"{singular}s";
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var result = new System.Text.StringBuilder();
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c) && i > 0)
            {
                result.Append('_');
            }
            result.Append(char.ToLowerInvariant(c));
        }
        return result.ToString();
    }

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u';

    private static string GetDefaultTestValue(string type) => type switch
    {
        "string" => "\"Test\"",
        "int" => "1",
        "bool" => "true",
        "decimal" => "1.0m",
        "DateTime" => "DateTime.UtcNow",
        _ => "default",
    };
}
