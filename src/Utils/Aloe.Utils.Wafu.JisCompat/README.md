# Aloe.Utils.Wafu.JisCompat

`Aloe.Utils.Wafu.JisCompat` は、JIS縮退用文字列置換ライブラリです。第3水準、第4水準漢字を第2水準漢字に縮退する機能を提供します。

## 主な機能

* 第3水準、第4水準漢字を第2水準漢字に縮退
* Shift_JIS の範囲に収まる文字列への変換
* シンプルで使いやすい API

## 対応環境

* .NET 9 以降

## インストール

NuGet パッケージマネージャーからインストールします：

```cmd
Install-Package Aloe.Utils.Wafu.JisCompat
```

あるいは、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Wafu.JisCompat
```

## 使用例

```csharp
using Aloe.Utils.Wafu.JisCompat;

// 文字列をJIS Level 2に縮退
string input = "髙島屋"; // 第3水準漢字を含む文字列
string result = JisLevel2Converter.ConvertToLevel2(input);
// result = "高島屋" // 第2水準漢字に変換
```

## ライセンス

MIT License
