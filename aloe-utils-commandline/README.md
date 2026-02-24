# Aloe.Utils.CommandLine

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.CommandLine.svg)](https://www.nuget.org/packages/Aloe.Utils.CommandLine)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.CommandLine.svg)](https://www.nuget.org/packages/Aloe.Utils.CommandLine)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-commandline.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

`Aloe.Utils.CommandLine` is a lightweight utility for flexible preprocessing of command-line arguments.  
It supports completion of boolean options like `--flag` and splitting of concatenated short options with values like `-uadmin`.  
When combined with .NET's `IConfigurationBuilder.AddCommandLine(...)`, it makes your application's configuration setup concise and robust.

## Key Features

* Completes standalone options like --flag → --flag true
* Splits concatenated options like -uadmin → -u admin
* Zero-dependency lightweight utility
* Perfect for integration with DI / IConfiguration / appsettings.json

## Requirements

* .NET 10 or later
* Compatible with Microsoft.Extensions.Configuration

## Install

Install via NuGet Package Manager:

```cmd
Install-Package Aloe.Utils.CommandLine
```

Or using .NET CLI:

```cmd
dotnet add package Aloe.Utils.CommandLine
```

## Usage

### 1. Basic Usage

```csharp
using Aloe.Utils.CommandLine;

var processedArgs = ArgsHelper.PreprocessArgs(
    args,
    flagArgs: ["--debug", "--standalone" ],
    shortArgs: ["-u", "-p" ]
);
```

Input example:

```cmd
myapp.exe -uadmin --debug
```

Output result:

```csharp
new[] { "-u", "admin", "--debug", "true" }
```

### 2. Practical Example

Here's a practical example showing how to manage command-line arguments, configuration files, secrets, and environment variables in an integrated way:

```csharp
public static class AppConfig
{
    // Command-line arguments for flags
    public static readonly List<string> FlagArgs =
    [
        "--standalone",
        "--debug",
        "--verbose",
    ];

    // Short-form command-line arguments
    public static readonly List<string> ShortArgs =
    [
        "-u",
        "-p",
        "-c",
    ];

    // Mapping between command-line arguments and configuration properties
    public static readonly Dictionary<string, string> Aliases = new()
    {
        { "--standalone", "AppSettings:IsStandalone" },
        { "--debug", "AppSettings:IsDebug" },
        { "-u", "AppSettings:Username" },
        { "-p", "AppSettings:Password" },
    };

    public static IConfigurationRoot CreateConfigurationRoot(string[] args)
    {
        // Preprocess command-line arguments
        var processedArgs = ArgsHelper.PreprocessArgs(
            args,
            FlagArgs,
            ShortArgs);

        // Build configuration
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            // Configuration files (lowest priority)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            // Development secrets (medium priority)
            .AddUserSecrets<Program>(optional: true)
            // Environment variables (high priority)
            .AddEnvironmentVariables()
            // Command-line arguments (highest priority)
            .AddCommandLine(processedArgs, Aliases);

        return builder.Build();
    }
}
```

This example implements the following features:

* Definition of flag options and short-form options
* Mapping between command-line arguments and configuration properties
* Integration of multiple configuration sources (JSON, secrets, environment variables, command-line)
* Separation of development and production settings

### 3. Reading Configuration

```csharp
// Example of reading configuration
var config = AppConfig.CreateConfigurationRoot(args);

// Getting configuration values
var isStandalone = config.GetValue<bool>("AppSettings:IsStandalone");
var username = config.GetValue<string>("AppSettings:Username");
```

## License

MIT License

## Contributing

Please report bugs and feature requests through GitHub Issues. Pull requests are welcome.
