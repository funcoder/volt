using Volt.Cli.Helpers;
using static Volt.Cli.Helpers.NamingConventions;

namespace Volt.Cli.Commands.Generators;

/// <summary>
/// Generators for controller, migration, job, mailer, and channel artifacts.
/// Each is a small, focused generation operation.
/// </summary>
public static class SimpleGenerators
{
    public static void GenerateController(string name, string[] fields)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var parsedFields = FieldParser.Parse(fields);
        var controllerName = EnsureControllerSuffix(name);
        var modelName = controllerName.Replace("Controller", string.Empty);
        var routePath = ToRoutePath(modelName);

        var data = new
        {
            model_namespace = layout.GetModelNamespace(),
            controller_namespace = layout.GetControllerNamespace(),
            controller_name = controllerName,
            model_name = modelName,
            model_name_plural = Pluralize(modelName),
            route_path = routePath,
            has_model = parsedFields.Count > 0,
            fields = parsedFields.Select(f => new { name = f.Name, type = f.Type }).ToArray(),
        };

        var outputPath = layout.ResolveControllerPath($"{controllerName}.cs");

        if (TemplateEngine.RenderToFile("Controller", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Controllers/{controllerName}.cs");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Controller '{controllerName}' generated.");
    }

    public static async Task GenerateMigrationAsync(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;

        ConsoleOutput.Info($"Generating migration: {name}");

        var efArgs = $"ef migrations add {name}";
        var projectArg = layout.GetEfProjectArg();
        var startupArg = layout.GetEfStartupProjectArg();
        if (projectArg is not null) efArgs += $" {projectArg}";
        if (startupArg is not null) efArgs += $" {startupArg}";

        var exitCode = await ProcessRunner.RunAsync(
            "dotnet", efArgs, layout.GetSolutionOrProjectRoot());

        if (exitCode == 0)
        {
            ConsoleOutput.Success($"Migration '{name}' created.");
        }
        else
        {
            ConsoleOutput.Error("Failed to create migration. Is dotnet-ef tool installed?");
        }
    }

    public static void GenerateJob(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var jobName = EnsurePascalCase(name);

        var data = new
        {
            job_namespace = layout.GetJobNamespace(),
            job_name = jobName,
            fields = Array.Empty<object>(),
        };

        var outputPath = layout.ResolveJobPath($"{jobName}.cs");

        if (TemplateEngine.RenderToFile("Job", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Jobs/{jobName}.cs");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Job '{jobName}' generated.");
    }

    public static void GenerateMailer(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var mailerName = EnsureMailerSuffix(name);

        var data = new
        {
            mailer_namespace = layout.GetMailerNamespace(),
            mailer_name = mailerName,
        };

        var outputPath = layout.ResolveMailerPath($"{mailerName}.cs");

        if (TemplateEngine.RenderToFile("Mailer", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Mailers/{mailerName}.cs");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Mailer '{mailerName}' generated.");
    }

    public static void GenerateChannel(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var channelName = EnsureChannelSuffix(name);
        var channelRoute = channelName
            .Replace("Channel", string.Empty)
            .ToLowerInvariant();

        var data = new
        {
            channel_namespace = layout.GetChannelNamespace(),
            channel_name = channelName,
            channel_route = channelRoute,
        };

        var outputPath = layout.ResolveChannelPath($"{channelName}.cs");

        if (TemplateEngine.RenderToFile("Channel", data, outputPath))
        {
            ConsoleOutput.FileCreated($"Channels/{channelName}.cs");
        }

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Channel '{channelName}' generated.");
    }
}
