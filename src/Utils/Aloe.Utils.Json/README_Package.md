# Aloe.Utils.Json

A lightweight utility for JSON serialization and formatting in .NET applications.

## Main Features

* Simple JSON serialization and deserialization with extension methods
* JSON string formatting with proper indentation
* Type-safe conversion between objects and JSON

## Supported Environments

* .NET 9 and later

## Usage

```csharp
using Aloe.Utils.Json;

// Serialize an object to JSON
var person = new Person { Name = "John", Age = 30 };
string json = person.ToJson();

// Deserialize JSON to an object
var deserializedPerson = json.FromJson<Person>();

// Format a JSON string
string formattedJson = json.FormatJson();
```

## License

MIT License
