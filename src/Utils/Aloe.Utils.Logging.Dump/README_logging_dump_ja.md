# Aloe.Utils.Logging.Dump

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Logging.Dump.svg)](https://www.nuget.org/packages/Aloe.Utils.Logging.Dump)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Logging.Dump.svg)](https://www.nuget.org/packages/Aloe.Utils.Logging.Dump)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-logging-dump.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Logging.Dump` は、バイト配列を16進ダンプ形式でログ出力するための軽量で使いやすいライブラリです。
主にデバッグやバイナリデータの解析が必要なアプリケーションで使用できます。

## 主な機能

* バイト配列を16進ダンプ形式でログ出力
* 16バイトごとの行区切り表示
* パフォーマンスに最適化された実装（ValueStringBuilder使用）
* Microsoft.Extensions.Loggingとの統合

## 対応環境

* .NET 9 以降

## インストール

NuGet パッケージマネージャーからインストール：

```cmd
Install-Package Aloe.Utils.Logging.Dump
```

または、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Logging.Dump
```

## 使用例

```csharp
using Microsoft.Extensions.Logging;
using Aloe.Utils.Logging.Dump;

// ロガーファクトリーの作成
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddConsole();
});

// ロガーの取得
var logger = loggerFactory.CreateLogger<Program>();

// サンプルデータの作成
byte[] sampleData =
[
    0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
    0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F,
    0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17,
    0x18, 0x19, 0x1A, 0x1B, 0x1C, 0x1D, 0x1E, 0x1F,
    0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x2C, 0x20, 0x57,
    0x6F, 0x72, 0x6C, 0x64, 0x21, 0x00, 0x00, 0x00,
];

// ダンプ出力の実行
logger.DumpDebug(sampleData);
```

出力例:
```
00000000 | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | 0123456789ABCDEF
-------- | -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- | ----------------
00000000 | 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F | ................
00000010 | 10 11 12 13 14 15 16 17 18 19 1A 1B 1C 1D 1E 1F | ................
00000020 | 48 65 6C 6C 6F 2C 20 57 6F 72 6C 64 21 00 00 00 | Hello, World!...
```

## ライセンス

MIT License

## 貢献

バグ報告や機能リクエストはGitHub Issuesを通じてお願いします。プルリクエストも歓迎します。 