using System.CommandLine;

namespace Shared.EnvFileGenerator.Commands;

/// <summary>
/// Command for updating existing .env files with new configuration sections.
/// Merges generated configuration with existing .env file, preserving current values.
/// </summary>
public static class EnvUpdateCommand
{
    /// <summary>
    /// Creates the 'env update' command with all its options.
    /// </summary>
    public static Command Create()
    {
        var command = new Command("update", "Update existing .env file with new configuration sections");

        var pathOption = new Option<string>("--path")
        {
            Aliases = { "-p" },
            Description = "Path to the project to scan (scans bin folder)",
            DefaultValueFactory = _ => "."
        };

        var envFileOption = new Option<string>("--env-file")
        {
            Aliases = { "-e" },
            Description = "Path to the .env file to update",
            DefaultValueFactory = _ => ".env"
        };

        var recursiveOption = new Option<bool>("--recursive")
        {
            Aliases = { "-r" },
            Description = "Recursively scan referenced assemblies",
            DefaultValueFactory = _ => true
        };

        var backupOption = new Option<bool>("--backup")
        {
            Aliases = { "-b" },
            Description = "Create a backup of the original .env file",
            DefaultValueFactory = _ => true
        };

        var configurationOption = new Option<string>("--config")
        {
            Aliases = { "-c" },
            Description = "Build configuration (Debug|Release)",
            DefaultValueFactory = _ => "Debug"
        };

        var includeDescriptionsOption = new Option<bool>("--descriptions")
        {
            Aliases = { "-d" },
            Description = "Include descriptions as comments in the output",
            DefaultValueFactory = _ => true
        };

        command.Add(pathOption);
        command.Add(envFileOption);
        command.Add(recursiveOption);
        command.Add(backupOption);
        command.Add(configurationOption);
        command.Add(includeDescriptionsOption);

        command.SetAction(async (parseResult, cancellationToken) =>
        {
            var path = parseResult.GetValue(pathOption)!;
            var envFile = parseResult.GetValue(envFileOption)!;
            var recursive = parseResult.GetValue(recursiveOption);
            var backup = parseResult.GetValue(backupOption);
            var config = parseResult.GetValue(configurationOption)!;
            var includeDescriptions = parseResult.GetValue(includeDescriptionsOption);

            var generator = new Services.EnvFileGenerator();
            await generator.UpdateAsync(
                path,
                envFile,
                recursive,
                includeDescriptions,
                backup,
                config,
                cancellationToken);

            return 0;
        });

        return command;
    }
}
