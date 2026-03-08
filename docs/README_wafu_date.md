# Aloe.Utils.Wafu.Date

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Wafu.Date.svg)](https://www.nuget.org/packages/Aloe.Utils.Wafu.Date)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Wafu.Date.svg)](https://www.nuget.org/packages/Aloe.Utils.Wafu.Date)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-wafu-date.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Wafu.Date` は、和風な日付処理をサポートするための軽量ユーティリティです。

## 主な機能

* 和風日付文字列のパース
* 漢数字を含む日付文字列のサポート
* 会計年度の月の解析
* 日付の拡張メソッド
* 時間間隔の日本語表示

## 対応環境

* .NET 9 以降
* AOT互換
* トリミング対応

## Install

NuGet パッケージマネージャーからインストールします：

```cmd
Install-Package Aloe.Utils.Wafu.Date
```

あるいは、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Wafu.Date
```

## Usage

```csharp
using Aloe.Utils.Wafu.Date;

// 和風日付文字列のパース
if (DateHelper.TryParseEx("令和5年4月1日", out var date))
{
    Console.WriteLine(date); // 2023-04-01
}

// 会計年度の月の解析
if (DateHelper.TryParseEx("4月", out var date3))
{
    Console.WriteLine(date3); // 2024-04-01
}

// 時間間隔の日本語表示
var span = new TimeSpan(1, 2, 3, 4);
Console.WriteLine(span.ToJaString()); // 1日 2時間 3分 4秒
Console.WriteLine(span.ToApproximateJaString()); // 約1日

// 文字列→日付（拡張メソッド）
var d1 = "202403".ToDateOrToday(); // 2024-03-01（今日が異なるなら今日）
var d2 = "invalid".ToDateOr(new DateOnly(2024, 1, 2)); // フォールバックを返す

// DateOnly ↔ DateTime 変換
DateOnly only = new DateOnly(2024, 3, 15);
DateTime dt = only.ToDateTime(); // 2024-03-15 00:00:00
DateOnly? only2 = DateTime.Now.ToDateOnly();
```

## ライセンス

MIT License

## 貢献

バグ報告や機能要望は、GitHub Issues でお願いします。プルリクエストも歓迎します。

