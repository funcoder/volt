using System.CommandLine;
using Volt.Cli.Commands;
using Volt.Cli.Helpers;

var rootCommand = new RootCommand("Volt - Rails-like framework for .NET");

rootCommand.Add(NewCommand.Create());
rootCommand.Add(GenerateCommand.Create());
rootCommand.Add(ServerCommand.Create());
rootCommand.Add(ConsoleCommand.Create());
rootCommand.Add(RoutesCommand.Create());
rootCommand.Add(DbCommand.Create());
rootCommand.Add(DestroyCommand.Create());

rootCommand.SetAction((_, _) =>
{
    ConsoleOutput.Banner();
    return Task.CompletedTask;
});

var config = new CommandLineConfiguration(rootCommand);
return await config.InvokeAsync(args);
