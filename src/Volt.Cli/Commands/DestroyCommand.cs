using System.CommandLine;
using Volt.Cli.Helpers;
using static Volt.Cli.Helpers.NamingConventions;

namespace Volt.Cli.Commands;

/// <summary>
/// Reverses a generate command by deleting the files that were created.
/// Supports destroying models, controllers, scaffolds, jobs, mailers, and channels.
/// </summary>
public static class DestroyCommand
{
    /// <summary>
    /// Creates the <c>volt destroy</c> command with all subcommands.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("destroy", "Remove generated files");
        command.Aliases.Add("d");

        command.Add(CreateModelSubcommand());
        command.Add(CreateControllerSubcommand());
        command.Add(CreateScaffoldSubcommand());
        command.Add(CreateJobSubcommand());
        command.Add(CreateMailerSubcommand());
        command.Add(CreateChannelSubcommand());

        return command;
    }

    private static Command CreateModelSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The model name to destroy" };

        var cmd = new Command("model", "Remove a model and its migration");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            DestroyModel(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateControllerSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The controller name to destroy" };

        var cmd = new Command("controller", "Remove a controller");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            DestroyController(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateScaffoldSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The resource name to destroy" };

        var cmd = new Command("scaffold", "Remove all scaffold files");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            DestroyScaffold(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateJobSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The job name to destroy" };

        var cmd = new Command("job", "Remove a job");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            DestroyJob(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateMailerSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The mailer name to destroy" };

        var cmd = new Command("mailer", "Remove a mailer");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            DestroyMailer(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateChannelSubcommand()
    {
        var nameArg = new Argument<string>("name") { Description = "The channel name to destroy" };

        var cmd = new Command("channel", "Remove a channel");
        cmd.Add(nameArg);

        cmd.SetAction((parseResult, _) =>
        {
            DestroyChannel(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static void DestroyModel(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var modelName = EnsurePascalCase(name);

        ConsoleOutput.Info($"Destroying model '{modelName}'...");
        ConsoleOutput.BlankLine();

        RemoveFileAtPath(layout.ResolveModelPath($"{modelName}.cs"), $"Models/{modelName}.cs");
        RemoveMigrations(layout, modelName);

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Model '{modelName}' destroyed.");
    }

    private static void DestroyController(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var controllerName = EnsureControllerSuffix(name);

        ConsoleOutput.Info($"Destroying controller '{controllerName}'...");
        ConsoleOutput.BlankLine();

        RemoveFileAtPath(
            layout.ResolveControllerPath($"{controllerName}.cs"),
            $"Controllers/{controllerName}.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Controller '{controllerName}' destroyed.");
    }

    private static void DestroyScaffold(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var modelName = EnsurePascalCase(name);
        var controllerName = $"{Pluralize(modelName)}Controller";
        var viewFolder = Pluralize(modelName);

        ConsoleOutput.Info($"Destroying scaffold '{modelName}'...");
        ConsoleOutput.BlankLine();

        RemoveFileAtPath(layout.ResolveModelPath($"{modelName}.cs"), $"Models/{modelName}.cs");
        RemoveFileAtPath(
            layout.ResolveControllerPath($"{controllerName}.cs"),
            $"Controllers/{controllerName}.cs");
        RemoveDirectoryAtPath(layout.ResolveViewPath(viewFolder), $"Views/{viewFolder}/");
        RemoveMigrations(layout, modelName);
        RemoveFileAtPath(
            layout.ResolveTestPath("Models", $"{modelName}Test.cs"),
            $"Tests/Models/{modelName}Test.cs");
        RemoveFileAtPath(
            layout.ResolveTestPath("Controllers", $"{controllerName}Test.cs"),
            $"Tests/Controllers/{controllerName}Test.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Scaffold '{modelName}' destroyed.");
    }

    private static void DestroyJob(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var jobName = EnsurePascalCase(name);

        ConsoleOutput.Info($"Destroying job '{jobName}'...");
        ConsoleOutput.BlankLine();

        RemoveFileAtPath(layout.ResolveJobPath($"{jobName}.cs"), $"Jobs/{jobName}.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Job '{jobName}' destroyed.");
    }

    private static void DestroyMailer(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var mailerName = EnsureMailerSuffix(name);

        ConsoleOutput.Info($"Destroying mailer '{mailerName}'...");
        ConsoleOutput.BlankLine();

        RemoveFileAtPath(layout.ResolveMailerPath($"{mailerName}.cs"), $"Mailers/{mailerName}.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Mailer '{mailerName}' destroyed.");
    }

    private static void DestroyChannel(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var layout = context.Layout;
        var channelName = EnsureChannelSuffix(name);

        ConsoleOutput.Info($"Destroying channel '{channelName}'...");
        ConsoleOutput.BlankLine();

        RemoveFileAtPath(layout.ResolveChannelPath($"{channelName}.cs"), $"Channels/{channelName}.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Channel '{channelName}' destroyed.");
    }

    private static void RemoveFileAtPath(string fullPath, string displayPath)
    {
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            ConsoleOutput.FileDeleted(displayPath);
        }
        else
        {
            ConsoleOutput.FileSkipped($"{displayPath} (not found)");
        }
    }

    private static void RemoveDirectoryAtPath(string fullPath, string displayPath)
    {
        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, recursive: true);
            ConsoleOutput.FileDeleted(displayPath);
        }
        else
        {
            ConsoleOutput.FileSkipped($"{displayPath} (not found)");
        }
    }

    private static void RemoveMigrations(IProjectLayout layout, string modelName)
    {
        var migrationsDir = layout.ResolveMigrationPath();
        if (!Directory.Exists(migrationsDir)) return;

        var pluralName = Pluralize(modelName);
        var migrationFiles = Directory.GetFiles(migrationsDir, $"*Create{pluralName}*");

        foreach (var file in migrationFiles)
        {
            File.Delete(file);
            ConsoleOutput.FileDeleted($"Migrations/{Path.GetFileName(file)}");
        }
    }
}
