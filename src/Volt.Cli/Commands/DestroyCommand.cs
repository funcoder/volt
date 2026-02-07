using System.CommandLine;
using Volt.Cli.Helpers;

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

        var modelName = EnsurePascalCase(name);

        ConsoleOutput.Info($"Destroying model '{modelName}'...");
        ConsoleOutput.BlankLine();

        RemoveFile(context, "Models", $"{modelName}.cs");
        RemoveMigrations(context, modelName);

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Model '{modelName}' destroyed.");
    }

    private static void DestroyController(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var controllerName = EnsureControllerSuffix(name);

        ConsoleOutput.Info($"Destroying controller '{controllerName}'...");
        ConsoleOutput.BlankLine();

        RemoveFile(context, "Controllers", $"{controllerName}.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Controller '{controllerName}' destroyed.");
    }

    private static void DestroyScaffold(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var modelName = EnsurePascalCase(name);
        var controllerName = $"{Pluralize(modelName)}Controller";
        var viewFolder = Pluralize(modelName);

        ConsoleOutput.Info($"Destroying scaffold '{modelName}'...");
        ConsoleOutput.BlankLine();

        RemoveFile(context, "Models", $"{modelName}.cs");
        RemoveFile(context, "Controllers", $"{controllerName}.cs");
        RemoveDirectory(context, "Views", viewFolder);
        RemoveMigrations(context, modelName);
        RemoveFile(context, Path.Combine("Tests", "Models"), $"{modelName}Test.cs");
        RemoveFile(context, Path.Combine("Tests", "Controllers"), $"{controllerName}Test.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Scaffold '{modelName}' destroyed.");
    }

    private static void DestroyJob(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var jobName = EnsurePascalCase(name);

        ConsoleOutput.Info($"Destroying job '{jobName}'...");
        ConsoleOutput.BlankLine();

        RemoveFile(context, "Jobs", $"{jobName}.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Job '{jobName}' destroyed.");
    }

    private static void DestroyMailer(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var mailerName = EnsureMailerSuffix(name);

        ConsoleOutput.Info($"Destroying mailer '{mailerName}'...");
        ConsoleOutput.BlankLine();

        RemoveFile(context, "Mailers", $"{mailerName}.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Mailer '{mailerName}' destroyed.");
    }

    private static void DestroyChannel(string name)
    {
        var context = ProjectContext.Require();
        if (context is null) return;

        var channelName = EnsureChannelSuffix(name);

        ConsoleOutput.Info($"Destroying channel '{channelName}'...");
        ConsoleOutput.BlankLine();

        RemoveFile(context, "Channels", $"{channelName}.cs");

        ConsoleOutput.BlankLine();
        ConsoleOutput.Success($"Channel '{channelName}' destroyed.");
    }

    private static void RemoveFile(ProjectContext context, string folder, string fileName)
    {
        var fullPath = context.ResolvePath(folder, fileName);
        var relativePath = Path.Combine(folder, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            ConsoleOutput.FileDeleted(relativePath);
        }
        else
        {
            ConsoleOutput.FileSkipped($"{relativePath} (not found)");
        }
    }

    private static void RemoveDirectory(ProjectContext context, string parent, string folder)
    {
        var fullPath = context.ResolvePath(parent, folder);
        var relativePath = Path.Combine(parent, folder);

        if (Directory.Exists(fullPath))
        {
            Directory.Delete(fullPath, recursive: true);
            ConsoleOutput.FileDeleted($"{relativePath}/");
        }
        else
        {
            ConsoleOutput.FileSkipped($"{relativePath}/ (not found)");
        }
    }

    private static void RemoveMigrations(ProjectContext context, string modelName)
    {
        var migrationsDir = context.ResolvePath("Migrations");
        if (!Directory.Exists(migrationsDir)) return;

        var pluralName = Pluralize(modelName);
        var migrationFiles = Directory.GetFiles(migrationsDir, $"*Create{pluralName}*");

        foreach (var file in migrationFiles)
        {
            File.Delete(file);
            ConsoleOutput.FileDeleted($"Migrations/{Path.GetFileName(file)}");
        }
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

    private static bool IsVowel(char c) => c is 'a' or 'e' or 'i' or 'o' or 'u';
}
