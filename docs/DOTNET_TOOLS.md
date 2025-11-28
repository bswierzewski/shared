# .NET Tools Configuration Guide

This guide explains how to install and configure .NET tools for the Shared project, specifically the `Shared.EnvFileGenerator` tool that auto-generates environment configuration files.

## Prerequisites

- .NET 9.0 SDK or higher
- GitHub account with access to the package repository
- A Personal Access Token (PAT) for NuGet authentication

## Setup Instructions

### 1. Create or Update Global NuGet Configuration

The project uses GitHub Packages for distributing custom .NET tools. You need to authenticate with GitHub to install these tools.

#### Option A: First Time Setup (Creating Global Config)

Create a global `nuget.config` file in your user profile directory:

**Windows:**

```powershell
$configPath = "$env:APPDATA\NuGet\nuget.config"
```

**macOS/Linux:**

```bash
configPath=~/.config/NuGet/nuget.config
```

#### Option B: Update Existing Config

If you already have a `nuget.config` file, add or update the GitHub Packages source.

### 2. Add GitHub Authentication Token

You need a GitHub Personal Access Token (PAT) with `read:packages` permission.

**Step 1: Generate a GitHub Personal Access Token**

1. Go to https://github.com/settings/tokens
2. Click "Generate new token (classic)"
3. Select scopes: `read:packages`
4. Generate and copy the token (save it securely!)

**Step 2: Configure NuGet with Your Token**

**Windows (PowerShell):**

```powershell
dotnet nuget add source `
  --username YOUR_GITHUB_USERNAME `
  --password YOUR_GITHUB_TOKEN `
  --name github `
  https://nuget.pkg.github.com/bswierzewski/index.json --store-password-in-clear-text
```

**macOS/Linux (Bash):**

```bash
dotnet nuget add source \
  --username YOUR_GITHUB_USERNAME \
  --password YOUR_GITHUB_TOKEN \
  --name github \
  https://nuget.pkg.github.com/bswierzewski/index.json --store-password-in-clear-text
```

Replace:

- `YOUR_GITHUB_USERNAME` - your GitHub username
- `YOUR_GITHUB_TOKEN` - the PAT you generated in Step 1

### 3. Install the Tool

#### First Time Installation

```bash
dotnet tool install Shared.EnvFileGenerator --prerelease
```

This installs the tool globally so you can use it from any directory.

#### Updating an Existing Installation

If you already have the tool installed, update it to the latest version:

```bash
dotnet tool update Shared.EnvFileGenerator --prerelease
```

### 4. Verify Installation

```bash
shared-env-gen --help
```

You should see the tool's help output if installed correctly.

## Configuration File Reference

Your global `nuget.config` should look similar to this:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- GitHub Packages source for Shared packages -->
    <add key="github" value="https://nuget.pkg.github.com/bswierzewski/index.json" />

    <!-- NuGet.org (official repository) -->
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>

  <!-- Package source mapping -->
  <packageSourceMapping>
    <!-- GitHub packages -->
    <packageSource key="github">
      <package pattern="Shared*" />
    </packageSource>
    <!-- Everything else from NuGet.org -->
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>

  <!-- Store credentials securely -->
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
```

## Manifest Generation

The `Shared.EnvFileGenerator` tool reads from class definitions with configuration options and automatically generates `.env.example` files.

### How It Works

1. **Configuration Classes** - Create classes in your project that inherit from `IOptions` interface:

```csharp
using Shared.Abstractions.Options;

public class SupabaseOptions : IOptions
{
    public static string SectionName => "Modules:Users:Supabase";

    public string Authority { get; set; }
    public string ApiKey { get; set; }
}
```

2. **Auto-Generation** - The tool scans your projects and generates configuration files:

```bash
shared-env-gen
```

This creates/updates `.env.example` files in test project directories with all available configuration options.

3. **Configuration Reference** - The generated `.env.example` file includes:
   - Configuration section names (from `SectionName`)
   - Property names and types
   - XML documentation comments (as descriptions)
   - Default example values (if provided)

## Troubleshooting

### Issue: "Unable to find package"

**Solution:** Verify your GitHub token has `read:packages` scope and is correctly configured:

```bash
dotnet nuget update source github --username YOUR_GITHUB_USERNAME --password YOUR_GITHUB_TOKEN --store-password-in-clear-text
```

### Issue: "401 Unauthorized"

**Solution:** Your token may have expired or been revoked. Generate a new PAT and update your nuget.config.

### Issue: Tool not found in PATH

**Solution:** Ensure global tools are installed to a directory in your PATH:

```bash
dotnet tool list --global
```

Check the installation location and add it to your PATH if needed.

## Additional Resources

- [NuGet Configuration](https://learn.microsoft.com/en-us/nuget/reference/nuget-config-file)
- [GitHub Packages Documentation](https://docs.github.com/en/packages)
- [.NET Tools Documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools)
