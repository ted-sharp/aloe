# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is Aloe.Utils.CommandLine - a lightweight C# utility library for preprocessing command-line arguments in .NET applications. The core functionality is in `ArgsHelper.cs` which provides methods to:

1. Complete boolean flags (`--debug` → `--debug true`)
2. Split concatenated short options (`-uadmin` → `-u admin`)
3. Use longest-match prioritization for overlapping short options

The library is designed to work seamlessly with Microsoft.Extensions.Configuration and supports .NET 10.0.

## Solution Structure

- **Main Library**: `src/Aloe.Utils.CommandLine/` - Contains the core `ArgsHelper` class
- **Tests**: `src/Aloe.Utils.CommandLine.Tests/` - XUnit tests for the library
- **Sample**: `src/Aloe.Utils.CommandLine.Samples/` - Demonstration of integration with DI/Configuration
- **Solution**: `src/Aloe.Utils.CommandLine.sln` - Main solution file

## Development Commands

### Building and Testing
```bash
# Build the entire solution
dotnet build src/Aloe.Utils.CommandLine.sln

# Build specific project
dotnet build src/Aloe.Utils.CommandLine/Aloe.Utils.CommandLine.csproj -c Release

# Run tests
dotnet test src/Aloe.Utils.CommandLine.Tests/

# Run sample application
dotnet run --project src/Aloe.Utils.CommandLine.Samples/ -- --debug -uadmin
```

### Publishing
```bash
# Use the provided publish script (Windows)
src/_publish.cmd

# Manual publish commands:
dotnet publish src/Aloe.Utils.CommandLine/Aloe.Utils.CommandLine.csproj -c Release -o publish/AloeUtilsCommandLine
dotnet pack src/Aloe.Utils.CommandLine/Aloe.Utils.CommandLine.csproj -c Release -o publish
```

### Cleaning
```bash
# Clean working directories
src/_clean_workdirs.cmd

# Manual clean
dotnet clean src/Aloe.Utils.CommandLine.sln
```

## Code Architecture

The library follows a simple, focused design:

1. **ArgsHelper** (`src/Aloe.Utils.CommandLine/ArgsHelper.cs`): Static utility class with the main `PreprocessArgs` method
2. **Processing Logic**: 
   - Flag completion has priority over short option splitting
   - Longest-match algorithm for overlapping short options (e.g., `-ab` vs `-a`)
   - Separator handling (`=`) is left to ConfigurationBuilder

## Key Implementation Details

- Uses HashSet for O(1) lookup performance on flag and short option sets
- Deterministic processing via OrderByDescending for longest-match
- Null-safe with ArgumentNullException guards
- Japanese documentation comments in source code

## Testing Strategy

Tests are comprehensive and cover:
- Basic flag completion
- Short option splitting 
- Longest-match prioritization
- Mixed argument scenarios
- Edge cases and error conditions

## NuGet Package Configuration

- Targets .NET 10.0
- AOT and trimming compatible
- Includes StyleCop and code analysis
- Generates documentation XML and symbol packages
- Version defined in `.csproj` file