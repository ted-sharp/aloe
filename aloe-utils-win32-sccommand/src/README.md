# Aloe.Utils.Win32.ScCommand

`Aloe.Utils.Win32.ScCommand` is a library that provides direct access to the Windows SC command's underlying Win32 API from .NET applications.
It can be used when you need to manage and operate services programmatically.

## Key Features

* Windows service creation and deletion
* Service management operations support
* Safe wrapping of Win32 API
* Error handling and logging
* Integration with Microsoft.Extensions.Logging

## Supported Environments

* .NET 9 and later
* Windows OS
* Administrator privileges (required for service operations)

## Important Notes

This library requires the application to be run with administrator privileges.
This is because Windows service management APIs require system-level operations.
Without administrator privileges, service operations will fail with access denied errors.

## Usage Example

```csharp
using Microsoft.Extensions.Logging;
using Aloe.Utils.Win32.ScCommand;

// Create logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

// Get logger
var logger = loggerFactory.CreateLogger<Program>();

// Create a service
bool created = Sc.CreateService(
    serviceName: "MyService",
    serviceFullPath: @"C:\Path\To\Service.exe",
    description: "Service description",
    startType: "auto",
    account: "LocalSystem",
    dependencies: null,
    logger: logger);

// Delete a service
bool deleted = Sc.DeleteService("MyService", logger);
```

## License

MIT License
