# Aloe.Utils.Text

`Aloe.Utils.Text` is a library that provides a high-performance, low-GC StringBuilder struct that combines Stackalloc and ArrayPool.

## Key Features

* Initial buffer allocation using stack allocation
* Efficient memory management using ArrayPool
* Low-GC string operations
* Fast string building

## Supported Environments

* .NET 9 and later

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
