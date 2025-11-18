namespace Shared.Tools;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Shared Tools CLI");
            Console.WriteLine("Version 1.0.0");
            return 0;
        }

        var command = args[0];

        return command switch
        {
            "--help" or "-h" => ShowHelp(),
            "--version" or "-v" => ShowVersion(),
            _ => HandleUnknownCommand(command)
        };
    }

    static int ShowHelp()
    {
        Console.WriteLine("Shared Tools - Common utilities across applications");
        Console.WriteLine();
        Console.WriteLine("Usage: shared-tools [command] [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --help, -h      Show this help message");
        Console.WriteLine("  --version, -v   Show version information");
        return 0;
    }

    static int ShowVersion()
    {
        Console.WriteLine("Shared Tools v1.0.0");
        return 0;
    }

    static int HandleUnknownCommand(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        Console.Error.WriteLine("Use 'shared-tools --help' for more information");
        return 1;
    }
}
