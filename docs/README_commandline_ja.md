# Aloe.Utils.CommandLine

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.CommandLine.svg)](https://www.nuget.org/packages/Aloe.Utils.CommandLine)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.CommandLine.svg)](https://www.nuget.org/packages/Aloe.Utils.CommandLine)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-commandline.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)

`Aloe.Utils.CommandLine` は、コマンドライン引数を柔軟に前処理する軽量ユーティリティです。  
`--flag` のようなブールオプションの補完や、`-uadmin` のような短縮オプションと値の連結にも対応しています。  
.NET の `IConfigurationBuilder.AddCommandLine(...)` と組み合わせて使用することで、アプリケーションの設定構成が簡潔かつ堅牢になります。

## 主な機能

* --flag → --flag true に補完
* -uadmin → -u admin に分割
* プラグイン不要・ゼロ依存の軽量ユーティリティ
* DI / IConfiguration / appsettings.json などとの統合に最適

## 対応環境

* .NET 10 以降
* Microsoft.Extensions.Configuration と併用可能

## Install

NuGet パッケージマネージャーからインストールします：

```cmd
Install-Package Aloe.Utils.CommandLine
```

あるいは、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.CommandLine
```

## Usage

### 1. 基本的な使用方法

```csharp
using Aloe.Utils.CommandLine;

var processedArgs = ArgsHelper.PreprocessArgs(
    args,
    flagArgs: ["--debug", "--standalone"],
    shortArgs: ["-u", "-p"]
);
```

入力例：

```cmd
myapp.exe -uadmin --debug
```

出力結果：

```csharp
new[] { "-u", "admin", "--debug", "true" }
```

### 2. 実践的な使用例

以下は、実際のアプリケーションでの使用例です。コマンドライン引数、設定ファイル、シークレット、環境変数を統合的に管理する方法を示しています：

```csharp
public static class AppConfig
{
    // フラグ用のコマンドライン引数
    public static readonly List<string> FlagArgs =
    [
        "--standalone",
        "--debug",
        "--verbose",
    ];

    // 短い名前のコマンドライン引数
    public static readonly List<string> ShortArgs =
    [
        "-u",
        "-p",
        "-c",
    ];

    // コマンドライン引数と設定プロパティのマッピング
    public static readonly Dictionary<string, string> Aliases = new()
    {
        { "--standalone", "AppSettings:IsStandalone" },
        { "--debug", "AppSettings:IsDebug" },
        { "-u", "AppSettings:Username" },
        { "-p", "AppSettings:Password" },
    };

    public static IConfigurationRoot CreateConfigurationRoot(string[] args)
    {
        // コマンドライン引数の前処理
        var processedArgs = ArgsHelper.PreprocessArgs(
            args,
            FlagArgs,
            ShortArgs);

        // 設定の構築
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            // 設定ファイル（優先順位: 低）
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            // 開発環境用のシークレット（優先順位: 中）
            .AddUserSecrets<Program>(optional: true)
            // 環境変数（優先順位: 高）
            .AddEnvironmentVariables()
            // コマンドライン引数（優先順位: 最高）
            .AddCommandLine(processedArgs, Aliases);

        return builder.Build();
    }
}
```

この例では以下の機能を実装しています：

* フラグオプションと短縮オプションの定義
* コマンドライン引数と設定プロパティのマッピング
* 複数の設定ソースの統合（JSON、シークレット、環境変数、コマンドライン）
* 開発環境と本番環境の設定の分離

### 3. 設定の読み取り

```csharp
// 設定の読み取り例
var config = AppConfig.CreateConfigurationRoot(args);

// 設定値の取得
var isStandalone = config.GetValue<bool>("AppSettings:IsStandalone");
var username = config.GetValue<string>("AppSettings:Username");
```

## ライセンス

MIT License

## 貢献

バグ報告や機能要望は、GitHub Issues でお願いします。プルリクエストも歓迎します。

