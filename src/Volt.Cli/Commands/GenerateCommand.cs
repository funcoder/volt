using System.CommandLine;
using Volt.Cli.Commands.Generators;

namespace Volt.Cli.Commands;

/// <summary>
/// Defines the <c>volt generate</c> command with subcommands for each artifact type.
/// Delegates all generation logic to specialized generator classes.
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
        command.Add(CreateAiContextSubcommand());

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
            ModelGenerator.Generate(
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
            SimpleGenerators.GenerateController(
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
            ScaffoldGenerator.Generate(
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
            await SimpleGenerators.GenerateMigrationAsync(parseResult.GetValue(nameArg)!);
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
            SimpleGenerators.GenerateJob(parseResult.GetValue(nameArg)!);
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
            SimpleGenerators.GenerateMailer(parseResult.GetValue(nameArg)!);
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
            SimpleGenerators.GenerateChannel(parseResult.GetValue(nameArg)!);
            return Task.CompletedTask;
        });

        return cmd;
    }

    private static Command CreateAiContextSubcommand()
    {
        var cmd = new Command("ai-context", "Generate AI assistant context files (CLAUDE.md, .cursorrules, copilot-instructions.md)");

        cmd.SetAction((_, _) =>
        {
            AiContextGenerator.Generate();
            return Task.CompletedTask;
        });

        return cmd;
    }
}
