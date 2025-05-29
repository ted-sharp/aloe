# Aloe.Utils.Configuration.Json

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Configuration.Json.svg)](https://www.nuget.org/packages/Aloe.Utils.Configuration.Json)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Configuration.Json.svg)](https://www.nuget.org/packages/Aloe.Utils.Configuration.Json)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-configuration-json.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Configuration.Json` is a lightweight utility for flexible handling of JSON configuration files.

## Main Features

* Extension methods for loading multiple JSON configurations
* Extension methods for conditional loading

## Supported Environments

* .NET 9 and later
* Used in conjunction with Microsoft.Extensions.Configuration.Json

## Install

Install via NuGet Package Manager:

```cmd
Install-Package Aloe.Utils.Configuration.Json
```

Or using .NET CLI:

```cmd
dotnet add package Aloe.Utils.Configuration.Json
```

## Usage

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Aloe.Utils.Configuration.Json;

var isDebug = true;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    // Pattern can be specified
    .AddAllJsonFiles("*.json")
    // Multiple files can be specified
    .AddJsonFiles(["appsettings.PostgreSql.json", "appsettings.Serilog.json" ])
    // Conditions can be specified within the method chain
    .AddJsonFileIf(isDebug, "appsettings.Debug.json");
```

## Note

### Build only once when setting `reloadOnChange: true`

In `ConfigurationBuilder`, when `reloadOnChange: true` is set, file monitoring is enabled and the files will not be released by GC.
Therefore, configuration files should be loaded only once at application startup.

## License

MIT License

## Contributing

Bug reports and feature requests are welcome on GitHub Issues. Pull requests are also appreciated.
