# Aloe.Utils.Hosting.Npgsql

Npgsql / EF Core DbContext の DI 登録を一元化する拡張メソッドを提供します。

## 機能

- `IServiceCollection.AddNpgsqlDbContext<TContext>()` — EF Core DbContext 登録
- `IServiceCollection.AddNpgsqlDataSource()` — 生 NpgsqlDataSource 登録

## 使い方

```csharp
services.AddNpgsqlDbContext<MyDbContext>(
    configuration,
    "DefaultConnection");
```

Development 環境では `EnableDetailedErrors()` / `EnableSensitiveDataLogging()` が自動で有効になります。
