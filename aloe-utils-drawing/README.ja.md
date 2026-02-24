# Aloe.Utils.Drawing

[![English](https://img.shields.io/badge/Language-English-blue)](./README.md)
[![日本語](https://img.shields.io/badge/言語-日本語-blue)](./README.ja.md)

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Drawing.svg)](https://www.nuget.org/packages/Aloe.Utils.Drawing)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Drawing.svg)](https://www.nuget.org/packages/Aloe.Utils.Drawing)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-drawing.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Drawing` は、Windowsの描画関連のWin32 APIを.NETアプリケーションから直接呼び出すためのライブラリです。
グラフィックス処理や描画操作をプログラムから行う必要がある場合に使用できます。

## 主な機能

* Windowsの描画APIの直接呼び出し
* グラフィックス処理のサポート

## 対応環境

* .NET 9 以降
* Windows OS

## インストール

NuGet パッケージマネージャーからインストール：

```cmd
Install-Package Aloe.Utils.Drawing
```

または、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Drawing
```

## 使用例

```csharp
using System.Drawing;
using Aloe.Utils.Drawing;

// 画像ファイルの読み込み(メモリに読み込むのでファイルロックなし)
using var image = ImageLoader.Load("path/to/image.png");

// 画像からアイコンへの変換
using var icon = image.ToIcon();
```

## ライセンス

MIT License

## 貢献

バグ報告や機能リクエストはGitHub Issuesを通じてお願いします。プルリクエストも歓迎します。 