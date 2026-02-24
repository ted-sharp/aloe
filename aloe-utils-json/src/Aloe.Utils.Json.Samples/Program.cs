using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Aloe.Utils.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

// ========================================
// JsonExtensions の使い方デモ
// ========================================
Console.WriteLine("=== Aloe.Utils.Json Demo ===");
Console.WriteLine();

// 1. ToJson() - オブジェクトをJSONに変換
var person = new Person
{
    Name = "Alice",
    Age = 30,
    Email = "alice@example.com"
};

string json = person.ToJson();
Console.WriteLine("1. ToJson() - オブジェクト → JSON:");
Console.WriteLine(json);
Console.WriteLine();

// 2. ToObj() - JSONをオブジェクトに変換
var jsonString = """
    {
        "Name": "Bob",
        "Age": 25,
        "Email": "bob@example.com"
    }
    """;

var deserializedPerson = jsonString.ToObj<Person>();
Console.WriteLine("2. ToObj() - JSON → オブジェクト:");
Console.WriteLine($"Name: {deserializedPerson?.Name}, Age: {deserializedPerson?.Age}, Email: {deserializedPerson?.Email}");
Console.WriteLine();

// 3. FormatJson() - JSON文字列を整形
var compactJson = """{"Name":"Charlie","Age":35,"Email":"charlie@example.com"}""";
var formattedJson = compactJson.FormatJson();
Console.WriteLine("3. FormatJson() - JSON整形:");
Console.WriteLine(formattedJson);
Console.WriteLine();

// 4. ToJson(JsonTypeInfo) - AOT/トリミング対応版
var aotPerson = new Person
{
    Name = "David",
    Age = 28,
    Email = "david@example.com"
};
var aotJson = aotPerson.ToJson(AppJsonContext.Default.Person);
Console.WriteLine("4. ToJson(JsonTypeInfo) - AOT/トリミング対応:");
Console.WriteLine(aotJson);
Console.WriteLine();

// 5. ToObj(JsonTypeInfo) - AOT/トリミング対応版
var aotJsonString = """
    {
        "Name": "Eve",
        "Age": 32,
        "Email": "eve@example.com"
    }
    """;
var aotDeserializedPerson = aotJsonString.ToObj(AppJsonContext.Default.Person);
Console.WriteLine("5. ToObj(JsonTypeInfo) - AOT/トリミング対応:");
Console.WriteLine($"Name: {aotDeserializedPerson?.Name}, Age: {aotDeserializedPerson?.Age}, Email: {aotDeserializedPerson?.Email}");
Console.WriteLine();

// 6. TryToObj() - 例外を発生させずに変換を試みる
var validJson = """{"Name":"Frank","Age":40,"Email":"frank@example.com"}""";
var invalidJson = """{"invalid json}""";

Console.WriteLine("6. TryToObj() - 安全な変換:");
if (validJson.TryToObj<Person>(out var validPerson))
{
    Console.WriteLine($"✓ 成功: {validPerson.Name}, {validPerson.Age}");
}
if (!invalidJson.TryToObj<Person>(out var invalidPerson))
{
    Console.WriteLine("✓ 不正なJSONは安全に処理されました");
}
Console.WriteLine();

// 7. JsonSerializerOptions - カスタムオプションの使用
var customOptions = new System.Text.Json.JsonSerializerOptions
{
    WriteIndented = false,
    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
};

var customPerson = new Person { Name = "Grace", Age = 27, Email = "grace@example.com" };
var customJson = customPerson.ToJson(customOptions);
Console.WriteLine("7. ToJson(JsonSerializerOptions) - カスタムオプション:");
Console.WriteLine(customJson);
Console.WriteLine();

// 8. FormatJson(JsonSerializerOptions) - カスタムオプションで整形
var compactJson2 = """{"Name":"Henry","Age":45}""";
var formattedCompact = compactJson2.FormatJson(customOptions);
Console.WriteLine("8. FormatJson(JsonSerializerOptions) - コンパクト整形:");
Console.WriteLine(formattedCompact);
Console.WriteLine();

Console.WriteLine("=== Configuration Manager Demo ===");
Console.WriteLine();

// 1. .NET 9以降の最小ホスト ビルダーを作成
var builder = Host.CreateApplicationBuilder(args);

// 2. ConfigurationManager に対してベースパスと JSON ファイルを設定
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.PostgreSQL.json", optional: true)
    .AddJsonFile("appsettings.Serilog.json", optional: true);

// 3. ビルドして IHost を生成
using var host = builder.Build();

// 4. IConfiguration を取得
var config = host.Services.GetRequiredService<IConfiguration>();

// 5. ConnectionStrings:DefaultConnection を表示
var defaultConn = config.GetConnectionString("DefaultConnection");
Console.WriteLine("=== ConnectionStrings:DefaultConnection ===");
Console.WriteLine(defaultConn);
Console.WriteLine();

// 6. PostgreSQL セクションを表示
var pg = config.GetSection("PostgreSQL");
Console.WriteLine("=== PostgreSQL Configuration ===");
Console.WriteLine($"Host     : {pg["Host"]}");
Console.WriteLine($"Database : {pg["Database"]}");
Console.WriteLine($"User     : {pg["Username"]}");
Console.WriteLine($"Password : {pg["Password"]}");
Console.WriteLine();

// 7. Serilog セクションを表示
var serilog = config.GetSection("Serilog");
Console.WriteLine("=== Serilog Configuration ===");
Console.WriteLine($"MinimumLevel : {serilog["MinimumLevel"]}");
Console.WriteLine("WriteTo      : " +
    String.Join(", ",
        serilog
          .GetSection("WriteTo")
          .GetChildren()
          .Select(c => c["Name"])));

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
Console.WriteLine();

// ========================================
// 型定義（トップレベルステートメントの後に配置）
// ========================================

/// <summary>
/// サンプル用のPersonクラス
/// </summary>
public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// AOT/トリミング対応用のJsonSerializerContext
/// </summary>
[JsonSerializable(typeof(Person))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
