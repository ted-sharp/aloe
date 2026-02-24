# Slash Commands for Aloe.Utils

This directory contains custom slash commands for Claude Code to help with development tasks in the Aloe.Utils project.

## Available Templates

### basic-template.md
A simple template for creating basic slash commands without special tool permissions.

**Usage**: `/basic-template [arguments]`

### advanced-template.md  
A comprehensive template demonstrating tool permissions, model selection, and complex workflows.

**Usage**: `/advanced-template <action> [target]`

## Creating Custom Slash Commands

### File Structure
```
.claude/commands/
├── README.md (this file)
├── basic-template.md
├── advanced-template.md
└── your-command.md
```

### Command Format
```markdown
---
description: Brief description of the command
argument-hint: Expected arguments format
allowed-tools: Tool permissions (optional)
model: Specific model to use (optional)
---

Command instructions and prompt content
```

### Frontmatter Options

- **description**: Brief explanation of what the command does
- **argument-hint**: Shows users the expected argument format
- **allowed-tools**: Specific tool permissions (e.g., `Bash(git:*)`, `Read(*)`, `Write(*)`)
- **model**: Choose specific Claude model (e.g., `claude-3-5-sonnet-20241022`)

### Special Variables

- **$ARGUMENTS**: Access to command-line arguments passed to the slash command
- **@filename**: Reference specific files in the command
- **!command**: Execute bash commands directly

## Examples for Aloe.Utils Project

### Project-Specific Commands You Could Create

```markdown
# build.md - Build and test utilities
---
description: Build, test and package Aloe.Utils libraries
allowed-tools: Bash(dotnet:*), Bash(src/*), Read(*)
---

# clean.md - Clean build artifacts
---  
description: Clean build artifacts using project clean script
allowed-tools: Bash(src/_clean_workdirs.cmd)
---

# publish.md - Publish utilities
---
description: Build and publish Aloe.Utils with packaging
allowed-tools: Bash(src/_publish.cmd), Read(*)
---

# docs.md - Generate documentation
---
description: Update project documentation and README files
allowed-tools: Read(*), Write(*), Edit(*)
---
```

## Best Practices

1. **Clear Descriptions**: Write concise, helpful descriptions for each command
2. **Argument Hints**: Provide clear guidance on expected arguments
3. **Tool Permissions**: Only grant necessary tool permissions for security
4. **Error Handling**: Include instructions for handling common error scenarios
5. **Documentation**: Document complex commands with usage examples

## Invoking Commands

After creating a command file, invoke it with:
```
/command-name arguments
```

The filename (without `.md`) becomes the slash command name.