# Aloe.Utils.Drawing.Wpf

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Drawing.Wpf.svg)](https://www.nuget.org/packages/Aloe.Utils.Drawing.Wpf)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Drawing.Wpf.svg)](https://www.nuget.org/packages/Aloe.Utils.Drawing.Wpf)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-drawing-wpf.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Drawing.Wpf` は、WPFアプリケーションでアイコンとイメージソースの相互変換を簡単に行うためのユーティリティライブラリです。
特に、アイコンからImageSourceへの変換をスレッドセーフに行う機能を提供します。

## 主な機能

* Icon から ImageSource への変換
* スレッドセーフな実装（Freeze機能の自動適用）
* Windows OS専用の最適化

## 対応環境

* .NET 9 以降
* Windows OS

## インストール

NuGet パッケージマネージャーからインストール：

```cmd
Install-Package Aloe.Utils.Drawing.Wpf
```

または、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Drawing.Wpf
```

## 使用例

```csharp
using System.Drawing;
using Aloe.Utils.Drawing.Wpf;

// Icon から ImageSource への変換
using var icon = new Icon("path/to/icon.ico");
 // 自動的にFreezeされます
var imageSource = icon.ToImageSource();
```

## ライセンス

MIT License

## 貢献

バグ報告や機能リクエストはGitHub Issuesを通じてお願いします。プルリクエストも歓迎します。 