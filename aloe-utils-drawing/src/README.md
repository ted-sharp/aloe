# Aloe.Utils.Drawing

`Aloe.Utils.Drawing` is a library that provides direct access to Windows drawing-related Win32 APIs from .NET applications.
It can be used when you need to perform graphics processing and drawing operations programmatically.

## Key Features

* Direct access to Windows drawing APIs
* Graphics processing support

## Supported Environments

* .NET 9 and later
* Windows OS

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
