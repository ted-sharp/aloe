# Aloe.Utils.Configuration.Json

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Configuration.Json.svg)](https://www.nuget.org/packages/Aloe.Utils.Configuration.Json)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Configuration.Json.svg)](https://www.nuget.org/packages/Aloe.Utils.Configuration.Json)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-configuration-json.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Configuration.Json` は、JSON形式の設定ファイルを柔軟に処理するための軽量ユーティリティです。  

## 主な機能

* 複数のJSON設定を読み込むための拡張メソッド
* 条件付き読み込みのための拡張メソッド

## 対応環境

* .NET 9 以降
* Microsoft.Extensions.Configuration.Json とともに使用

## Install

NuGet パッケージマネージャーからインストールします：

```cmd
Install-Package Aloe.Utils.Configuration.Json
```

あるいは、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Configuration.Json
```

## Usage

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Aloe.Utils.Configuration.Json;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    // パターンを指定できる。
    .AddAllJsonFiles("*.json")
    // 複数ファイルを指定できる。
    .AddJsonFiles(["appsettings.PostgreSql.json", "appsettings.Serilog.json" ])
    // メソッドチェーン内で条件を指定できる。
    .AddJsonFileIf(condition: true, "appsettings.Debug.json");
```

## Note

### `reloadOnChange: true` を設定する場合は1回だけビルドする

`ConfigurationBuilder` では、`reloadOnChange: true` にすると、ファイル監視が有効になるため GC で解放されなくなります。
そのため、設定ファイルはアプリケーションの開始時に一度だけ読み込みます。

## ライセンス

MIT License

## 貢献

バグ報告や機能要望は、GitHub Issues でお願いします。プルリクエストも歓迎します。
