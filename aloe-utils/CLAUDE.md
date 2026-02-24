# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is a hub repository for Aloe.Utils utility libraries that serves as both a collection point and template for related repositories. The repository manages multiple .NET utility libraries focused on common programming tasks, text processing, Japanese language utilities, and Windows-specific operations.

## Project Architecture

- **Hub Structure with Junction Links**: This repository contains junction links to 15+ separate utility libraries, each maintained in their own repositories but accessible locally through Windows junction symbolic links
- **Template Repository**: Serves as a template for creating new utility libraries in the Aloe.Utils ecosystem  
- **.NET 9.0 Target**: All utilities target .NET 9.0 and are published as NuGet packages
- **Library Categories**:
  - Core utilities (CommandLine, Configuration, SafeIO)
  - Text processing (Text, Json)
  - Drawing operations (Drawing, Drawing.Wpf)
  - Japanese language support (Wafu.* libraries)
  - Windows-specific utilities (Win32.ScCommand)
- **Junction Management**: Use `create_junctions.cmd` and `remove_junctions.cmd` to manage symbolic links

## Development Commands

### Build Commands
```bash
# Build a specific project (from src directory)
dotnet build .\Aloe.Utils.Template\Aloe.Utils.Template.csproj -c Release

# Publish application
dotnet publish .\Aloe.Utils.Template\Aloe.Utils.Template.csproj -c Release -o .\publish\AloeUtilsTemplate

# Create NuGet package
dotnet pack .\Aloe.Utils.Template\Aloe.Utils.Template.csproj -c Release -o .\publish
```

### Junction Management
```cmd
# Create junction links to all aloe-utils projects (requires admin privileges)
create_junctions.cmd

# Remove junction links
remove_junctions.cmd
```

### Utility Scripts (from individual project src directories)
```bash
# Clean build artifacts and temporary files
.\_clean_workdirs.cmd

# Build, publish, and package
.\_publish.cmd
```

### Working with Individual Projects
```bash
# Navigate to a specific project via junction link
cd aloe-utils-commandline\src

# Build a specific project
dotnet build .\Aloe.Utils.CommandLine\Aloe.Utils.CommandLine.csproj -c Release

# Run tests for a specific project
dotnet test .\Aloe.Utils.CommandLine.Tests\Aloe.Utils.CommandLine.Tests.csproj
```

## Code Standards

### Implementation Standards
- Use dependency injection pattern with appropriate interfaces
- Implement proper async/await patterns for I/O operations
- Include comprehensive error handling with structured logging
- Follow MVVM pattern where applicable
- Use `ILogger<T>` for all logging operations

### Naming Conventions
- Classes: PascalCase
- Methods: PascalCase  
- Variables: camelCase
- Constants: UPPER_CASE
- Interfaces: I prefix (IPascalCase)

### Code Style
- Indentation: 4 spaces
- Line length: 120 characters maximum
- Use XML documentation comments for public APIs
- Include proper exception handling with specific exception types

## Knowledge Management

The project uses structured documentation in `.cursor/rules/` directory for maintaining consistency:
- Reference `./doc` directory for accumulated knowledge
- Document new implementation patterns and decisions
- Maintain implementation consistency across utilities

## Repository Structure

- **Root Directory**: Contains junction links to all aloe-utils projects for unified access
- **Junction Links**: Each `aloe-utils-*` directory is a Windows junction pointing to `../aloe-utils-*`
- **Individual Projects**: Each utility maintains its own repository structure with `src/`, tests, and documentation
- **Consistent Structure**: All projects follow the pattern:
  - `src/ProjectName/` - Main library code
  - `src/ProjectName.Tests/` - Unit tests  
  - `src/ProjectName.Samples/` - Usage examples
  - Standard build scripts (`_clean_workdirs.cmd`, `_publish.cmd`)

## Slash Commands

Custom slash commands are available in `.claude/commands/`:
- `/basic-template` - Simple command template
- `/advanced-template` - Complex command with tool permissions
- See `.claude/commands/README.md` for detailed usage