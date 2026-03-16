# Aloe.Utils.Hosting.Serilog

Serilog の標準設定を一括で追加する拡張メソッドを提供します。

## 機能

- `IHostBuilder.AddSerilogDefaults()` — Worker / CLI / Blazor 共通の Serilog 設定
- `SerilogExtensions.ConfigureSerilogFromSettings()` — Host 不使用の CLI 用

## 使い方

```csharp
var builder = Host.CreateApplicationBuilder(args);
builder.Host.AddSerilogDefaults();
```

appsettings.json に `Serilog:WriteTo` セクションがない場合、Console + File（rolling daily, 30日保持）がデフォルトで追加されます。
