# Aloe.Utils.Drawing.Wpf

`Aloe.Utils.Drawing.Wpf` is a utility library that makes it easy to convert between icons and image sources in WPF applications.
It particularly provides functionality to safely convert icons to ImageSource in a thread-safe manner.

## Key Features

* Conversion from Icon to ImageSource
* Thread-safe implementation (automatic Freeze functionality)
* Windows OS-specific optimizations

## Supported Environments

* .NET 9 and later
* Windows OS

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
