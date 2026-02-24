# Aloe.Utils.Text

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Text.svg)](https://www.nuget.org/packages/Aloe.Utils.Text)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Text.svg)](https://www.nuget.org/packages/Aloe.Utils.Text)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-text.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Text` is a library that provides a high-performance, low-GC StringBuilder struct that combines Stackalloc and ArrayPool.

## Key Features

* Initial buffer allocation using stack allocation
* Efficient memory management using ArrayPool
* Low-GC string operations
* Fast string building

## Supported Environments

* .NET 9 and later

## Installation

Install via NuGet Package Manager:

```cmd
Install-Package Aloe.Utils.Text
```

Or using .NET CLI:

```cmd
dotnet add package Aloe.Utils.Text
```

## Usage Examples

```csharp
using Aloe.Utils.Text;

// Basic usage
using (var sb = new ValueStringBuilder(stackalloc char[256]))
{
    sb.Append("Hello");
    sb.Append(" ");
    sb.Append("World");
    var result = sb.ToString(); // Returns "Hello World"
}

// Appending using Span
using (var sb = new ValueStringBuilder(stackalloc char[256]))
{
    ReadOnlySpan<char> span = "Test".AsSpan();
    sb.Append(span);
    var result = sb.ToString(); // Returns "Test"
}

// Appending characters
using (var sb = new ValueStringBuilder(stackalloc char[256]))
{
    sb.Append('a');
    sb.Append('b');
    sb.Append('c');
    var result = sb.ToString(); // Returns "abc"
}
```

## Important Notes

* Always call `Dispose()` to return resources (buffers borrowed from ArrayPool)
* `ToString()` does not release resources. The builder can be reused until Dispose is called
* As this is a struct, it's recommended to pass it by reference using the `ref` keyword

## License

MIT License

## Contributing

Please report bugs and feature requests through GitHub Issues. Pull requests are welcome. 