using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Aloe.Utils.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;

// 1. .NET 9以降の最小ホスト ビルダーを作成
var builder = Host.CreateApplicationBuilder(args);

// 2. ConfigurationManager に対してベースパスと JSON ファイルを設定
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddAllJsonFiles("*.json");
    //.AddJsonFiles(
    //[
    //    "appsettings.json",
    //    "appsettings.PostgreSQL.json",
    //    "appsettings.Serilog.json"
    //]);

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
