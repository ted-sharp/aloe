---
description: Advanced slash command template with tool permissions
argument-hint: <action> [target]
allowed-tools: Bash(git status:*), Bash(git diff:*), Read(*), Write(*), Edit(*)
model: claude-3-5-sonnet-20241022
---

# Advanced Slash Command Template with Tools

This template demonstrates how to create slash commands with specific tool permissions and model selection.

## Command Usage
Use this command with: `/advanced-template <action> [target]`

Arguments provided: $ARGUMENTS

## Available Tools
This command has access to:
- Git commands (status, diff)
- File operations (read, write, edit)
- Custom model selection

## Implementation Instructions

### Step 1: Parse Arguments
First, analyze the provided arguments: $ARGUMENTS

### Step 2: Execute Action
Based on the action parameter, perform the appropriate operation:
- `analyze`: Analyze the codebase or specific files
- `build`: Execute build commands
- `test`: Run tests and report results
- `docs`: Generate or update documentation

### Step 3: File Operations
When working with files:
1. Use Read tool to examine existing files
2. Use Write tool for new files
3. Use Edit tool for modifications

### Step 4: Git Integration
For version control operations:
1. Check git status first
2. Review changes with git diff
3. Provide summary of repository state

## Example Usage Scenarios

### Analyze Code
```
/advanced-template analyze src/
```

### Build Project
```
/advanced-template build release
```

### Generate Documentation  
```
/advanced-template docs api
```

## Error Handling
Always include proper error handling and user feedback for:
- Invalid arguments
- Missing files
- Tool execution failures
- Git repository issues