using System.CommandLine;
using Shared.EnvFileGenerator.Commands;

var rootCommand = new RootCommand("Shared CLI - Tools for managing shared project configurations");

var envCommand = new Command("env", "Manage environment variables and .env files")
{
    EnvGenerateCommand.Create(),
    EnvListCommand.Create(),
    EnvUpdateCommand.Create()
};

rootCommand.Add(envCommand);

return rootCommand.Parse(args).Invoke();
