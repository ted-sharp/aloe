# Aloe.Utils.Win32.ScCommand

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Win32.ScCommand.svg)](https://www.nuget.org/packages/Aloe.Utils.Win32.ScCommand)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Win32.ScCommand.svg)](https://www.nuget.org/packages/Aloe.Utils.Win32.ScCommand)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-win32-sccommand.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Win32.ScCommand` は、WindowsのSCコマンドの基盤となるWin32 APIを.NETアプリケーションから直接呼び出すためのライブラリです。
サービス管理や操作をプログラムから行う必要がある場合に使用できます。

## 主な機能

* Windowsサービスの作成と削除
* サービス管理操作のサポート
* Win32 APIの安全なラッピング
* エラーハンドリングとログ出力
* Microsoft.Extensions.Loggingとの統合

## 対応環境

* .NET 9 以降
* Windows OS
* 管理者権限（サービス操作には必須）

## 注意事項

このライブラリを使用するには、アプリケーションを管理者権限で実行する必要があります。
これは、Windowsのサービス管理APIがシステムレベルの操作を必要とするためです。
管理者権限がない場合、サービス操作は失敗し、アクセス拒否エラーが発生します。

## インストール

NuGet パッケージマネージャーからインストール：

```cmd
Install-Package Aloe.Utils.Win32.ScCommand
```

または、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Win32.ScCommand
```

## 使用例

```csharp
using Microsoft.Extensions.Logging;
using Aloe.Utils.Win32.ScCommand;

// ロガーファクトリーの作成
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

// ロガーの取得
var logger = loggerFactory.CreateLogger<Program>();

// サービスの作成
bool created = Sc.CreateService(
    serviceName: "MyService",
    serviceFullPath: @"C:\Path\To\Service.exe",
    description: "サービスの説明",
    startType: "auto",
    account: "LocalSystem",
    dependencies: null,
    logger: logger);

// サービスの削除
bool deleted = Sc.DeleteService("MyService", logger);
```

## ライセンス

MIT License

## 貢献

バグ報告や機能リクエストはGitHub Issuesを通じてお願いします。プルリクエストも歓迎します。 