# Aloe.Utils.Wafu.JisCompat

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Wafu.JisCompat.svg)](https://www.nuget.org/packages/Aloe.Utils.Wafu.JisCompat)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Wafu.JisCompat.svg)](https://www.nuget.org/packages/Aloe.Utils.Wafu.JisCompat)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-wafu-date.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Wafu.JisCompat` は、JIS縮退用文字列置換ライブラリです。第3水準、第4水準漢字を第2水準漢字に縮退する機能を提供します。

## 主な機能

* 第3水準、第4水準漢字を第2水準漢字に縮退
* Shift_JIS の範囲に収まる文字列への変換
* シンプルで使いやすい API

## 対応環境

* .NET 9 以降

## Install

NuGet パッケージマネージャーからインストールします：

```cmd
Install-Package Aloe.Utils.Wafu.JisCompat
```

あるいは、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Wafu.JisCompat
```

## Usage

```csharp
using Aloe.Utils.Wafu.JisCompat;

// 文字列をJIS Level 2に縮退
string input = "髙島屋"; // 第3水準漢字を含む文字列
string result = JisLevel2Converter.ConvertToLevel2(input);
// result = "高島屋" // 第2水準漢字に変換
```

## ライセンス

MIT License

## 貢献

バグ報告や機能要望は、GitHub Issues でお願いします。プルリクエストも歓迎します。

