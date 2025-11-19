# Shared.Tools

A CLI utility toolkit for Shared projects with tools for configuration management, environment setup, and development productivity.

## Features

- **Environment Configuration Generation**: Automatically generate `.env` files from `IOptions` implementations
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Easy Installation**: Install as a global .NET tool

## Installation

Install as a global tool:

```bash
dotnet tool install -g Shared.Tools
```

Or install locally in a project:

```bash
dotnet tool install Shared.Tools
```

## Usage

### Generate .env File

Scan your project for `IOptions` implementations and generate a `.env.example` file:

```bash
shared-tools env generate
```

#### Options

- `-p, --path <PATH>` - Project path to scan (default: current directory)
- `-o, --output <PATH>` - Output .env file path (default: .env.example)
- `-c, --config <CONFIG>` - Build configuration: Debug or Release (default: Debug)
- `-r, --recursive` - Recursively scan referenced assemblies (default: true)
- `-d, --descriptions` - Include type descriptions as comments (default: true)
- `-f, --force` - Overwrite existing file without confirmation

#### Examples

Generate .env file for current project:
```bash
shared-tools env generate
```

Generate for specific project with Release configuration:
```bash
shared-tools env generate -p ./src/MyApp -c Release -o .env.production
```

Generate with custom output path:
```bash
shared-tools env generate -o ./config/.env.local
```

### How It Works

The tool scans your compiled project assemblies for classes implementing `Shared.Abstractions.Options.IOptions` interface:

```csharp
public class SmtpOptions : IOptions
{
    public static string SectionName => "Smtp";

    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}
```

This generates:

```env
# ==========================================
# Smtp
# ==========================================
# Type: string
SMTP__HOST=

# Type: int
SMTP__PORT=0

# Type: string
SMTP__USERNAME=

# Type: string
SMTP__PASSWORD=

```

## Configuration Classes

Your options classes must:

1. Implement `Shared.Abstractions.Options.IOptions`
2. Define a static `SectionName` property
3. Have public read/write properties

```csharp
public class DatabaseOptions : IOptions
{
    public static string SectionName => "Database";

    public string ConnectionString { get; set; }
    public int CommandTimeout { get; set; } = 30;
}
```

## Building from Source

```bash
dotnet build src/Modules/Shared.Tools/Shared.Tools.csproj
```

## Requirements

- .NET 9.0 or later
- Project must be built before running `env generate` (executables in `bin/Debug` or `bin/Release`)

## Troubleshooting

**Error: "Build output folder not found"**
- Make sure to build your project first: `dotnet build`
- Check that the Build configuration (Debug/Release) matches the `-c` flag

**Error: "No IOptions implementations found"**
- Ensure your options classes implement `IOptions` from `Shared.Abstractions.Options`
- Verify the assembly is being loaded correctly
- Check namespace matches `Shared.Abstractions.Options.IOptions`

## License

Apache License 2.0
