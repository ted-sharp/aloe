# Aloe.Utils.Drawing.Wpf

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Drawing.Wpf.svg)](https://www.nuget.org/packages/Aloe.Utils.Drawing.Wpf)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Drawing.Wpf.svg)](https://www.nuget.org/packages/Aloe.Utils.Drawing.Wpf)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-drawing-wpf.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Drawing.Wpf` is a utility library that makes it easy to convert between icons and image sources in WPF applications.
It particularly provides functionality to safely convert icons to ImageSource in a thread-safe manner.

## Key Features

* Conversion from Icon to ImageSource
* Thread-safe implementation (automatic Freeze functionality)
* Windows OS-specific optimizations

## Supported Environments

* .NET 9 and later
* Windows OS

## Installation

Using NuGet Package Manager:

```cmd
Install-Package Aloe.Utils.Drawing.Wpf
```

Using .NET CLI:

```cmd
dotnet add package Aloe.Utils.Drawing.Wpf
```

## Usage Example

```csharp
using System.Drawing;
using Aloe.Utils.Drawing.Wpf;

// Convert from Icon to ImageSource
using var icon = new Icon("path/to/icon.ico");
 // Automatically frozen
var imageSource = icon.ToImageSource();
```

## License

MIT License

## Contributing

Please report bugs and feature requests through GitHub Issues. Pull requests are also welcome.

