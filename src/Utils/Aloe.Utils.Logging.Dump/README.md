# Aloe.Utils.Logging.Dump

A lightweight and easy-to-use library for logging byte arrays in hexadecimal dump format.
Primarily used in applications that require debugging and binary data analysis.

## Main Features

* Log byte arrays in hexadecimal dump format
* Line breaks every 16 bytes
* Performance-optimized implementation (using ValueStringBuilder)
* Integration with Microsoft.Extensions.Logging

## Supported Environments

* .NET 9 and later

## Usage Example

```csharp
using Microsoft.Extensions.Logging;
using Aloe.Utils.Logging.Dump;

// Create logger factory
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

// Get logger
var logger = loggerFactory.CreateLogger<Program>();

// Create sample data
byte[] sampleData =
[
    0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
    0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
    0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
    0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
    0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x2C, 0x20, 0x57,
    0x6F, 0x72, 0x6C, 0x64, 0x21, 0x00, 0x00, 0x00,
];

// Execute dump output
logger.DumpDebug(sampleData);
```

Example output:
```
00000000 | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | 0123456789ABCDEF
-------- | -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ----------------
00000000 | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | ................
00000010 | 10 11 12 13 14 15 16 17 18 19 1A 1B 1C 1D 1E 1F | ................
00000020 | 48 65 6C 6C 6F 2C 20 57 6F 72 6C 64 21 00 00 00 | Hello, World!...
```

## License

MIT License
