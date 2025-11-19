using System.CommandLine;
using Shared.EnvFileGenerator.Commands;

var rootCommand = new RootCommand("Shared Tools - CLI utilities for shared projects");

var envCommand = new Command("env", "Environment variable management commands");
envCommand.Add(EnvGenerateCommand.Create());

rootCommand.Add(envCommand);

return rootCommand.Parse(args).Invoke();
