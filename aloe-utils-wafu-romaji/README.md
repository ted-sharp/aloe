# Aloe.Utils.Wafu.Romaji

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Wafu.Romaji.svg)](https://www.nuget.org/packages/Aloe.Utils.Wafu.Romaji)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Wafu.Romaji.svg)](https://www.nuget.org/packages/Aloe.Utils.Wafu.Romaji)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-wafu-romaji.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Wafu.Romaji` は、日本語文字列をローマ字に変換するための軽量で使いやすいライブラリです。
主に日本語のテキスト処理や国際化対応が必要なアプリケーションで使用できます。

## 主な機能

* ひらがな・カタカナ文字列をヘボン式ローマ字に変換
* 基本的な文字（あいうえお等）の変換
* 濁音・半濁音（がぎぐげご等）の変換
* 拗音（きゃきゅきょ等）の変換
* 促音（っ）の適切な処理
* 長音記号（ー）の適切な処理
* 小文字（ぁぃぅぇぉ等）の変換
* パフォーマンスに最適化された実装（ValueStringBuilder使用）

## 対応環境

* .NET 9 以降

## インストール

NuGet パッケージマネージャーからインストール：

```cmd
Install-Package Aloe.Utils.Wafu.Romaji
```

または、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Wafu.Romaji
```

## 使用例

```csharp
using Aloe.Utils.Wafu.Romaji;

// 基本的な使用例
string result = Romanizer.Convert("こんにちは"); // "konnichiha"

// カタカナの変換
string katakana = Romanizer.Convert("コンニチハ"); // "konnichiha"

// 濁音の変換
string dakuon = Romanizer.Convert("がぎぐげご"); // "gagigugego"

// 拗音の変換
string youon = Romanizer.Convert("きゃきゅきょ"); // "kyakyukyo"

// 促音の変換
string sokuon = Romanizer.Convert("っか"); // "kka"

// 長音の変換
string chouon = Romanizer.Convert("あー"); // "aa"
```

## 機能詳細

### 変換ルール

* 基本的なひらがな・カタカナの変換
* 濁音・半濁音の適切な変換
* 拗音（二文字）の正しい変換
* 促音（っ）の適切な処理
* 長音記号（ー）の適切な処理
* 小文字の適切な変換

### パフォーマンス

* ValueStringBuilder を使用したメモリ効率の良い実装
* スタックアロケーションを活用した高速な文字列処理
* スレッドセーフな設計

## License

MIT License

## Contributing

Please report bugs and feature requests through GitHub Issues. Pull requests are welcome.
