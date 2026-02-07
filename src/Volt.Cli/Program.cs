using System.CommandLine;
using Volt.Cli.Commands;

var rootCommand = new RootCommand("Volt - Rails-like framework for .NET");

rootCommand.Add(NewCommand.Create());
rootCommand.Add(GenerateCommand.Create());
rootCommand.Add(ServerCommand.Create());
rootCommand.Add(ConsoleCommand.Create());
rootCommand.Add(RoutesCommand.Create());
rootCommand.Add(DbCommand.Create());
rootCommand.Add(DestroyCommand.Create());

var config = new CommandLineConfiguration(rootCommand);
return await config.InvokeAsync(args);
