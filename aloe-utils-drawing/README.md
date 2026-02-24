# Aloe.Utils.Drawing

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Drawing.svg)](https://www.nuget.org/packages/Aloe.Utils.Drawing)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Drawing.svg)](https://www.nuget.org/packages/Aloe.Utils.Drawing)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-drawing.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Drawing` is a library that provides direct access to Windows drawing-related Win32 APIs from .NET applications.
It can be used when you need to perform graphics processing and drawing operations programmatically.

## Key Features

* Direct access to Windows drawing APIs
* Graphics processing support

## Supported Environments

* .NET 9 and later
* Windows OS

## Installation

Using NuGet Package Manager:

```cmd
Install-Package Aloe.Utils.Drawing
```

Using .NET CLI:

```cmd
dotnet add package Aloe.Utils.Drawing
```

## Usage Example

```csharp
using System.Drawing;
using Aloe.Utils.Drawing;

// Load image file (loads into memory, so no file lock)
using var image = ImageLoader.Load("path/to/image.png");

// Convert image to icon
using var icon = image.ToIcon();
```

## License

MIT License

## Contributing

Please report bugs and feature requests through GitHub Issues. Pull requests are also welcome.
