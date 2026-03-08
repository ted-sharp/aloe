# Aloe.Utils.Text

[![NuGet Version](https://img.shields.io/nuget/v/Aloe.Utils.Text.svg)](https://www.nuget.org/packages/Aloe.Utils.Text)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Aloe.Utils.Text.svg)](https://www.nuget.org/packages/Aloe.Utils.Text)
[![License](https://img.shields.io/github/license/ted-sharp/aloe-utils-text.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

`Aloe.Utils.Text` は、Stackalloc と ArrayPool を組み合わせた高速かつ低GCの StringBuilder 構造体を提供するライブラリです。

## 主な機能

* スタックアロケーションによる初期バッファの確保
* ArrayPool を利用した効率的なメモリ管理
* 低GCな文字列操作
* 高速な文字列構築

## 対応環境

* .NET 9 以降

## インストール

NuGet パッケージマネージャーからインストール：

```cmd
Install-Package Aloe.Utils.Text
```

または、.NET CLI で：

```cmd
dotnet add package Aloe.Utils.Text
```

## 使用例

```csharp
using Aloe.Utils.Text;

// 基本的な使用方法
using (var sb = new ValueStringBuilder(stackalloc char[256]))
{
    sb.Append("Hello");
    sb.Append(" ");
    sb.Append("World");
    var result = sb.ToString(); // "Hello World"を返します
}

// Spanを使用した追加
using (var sb = new ValueStringBuilder(stackalloc char[256]))
{
    ReadOnlySpan<char> span = "Test".AsSpan();
    sb.Append(span);
    var result = sb.ToString(); // "Test"を返します
}

// 文字の追加
using (var sb = new ValueStringBuilder(stackalloc char[256]))
{
    sb.Append('a');
    sb.Append('b');
    sb.Append('c');
    var result = sb.ToString(); // "abc"を返します
}
```

## 注意事項

* 必ず `Dispose()` でリソース（ArrayPoolから借りたバッファ）を返却してください
* `ToString()` はリソース解放を行いません。Disposeを呼ぶまで再利用可能です
* 構造体なので、`ref` キーワードを使用して参照渡しすることを推奨します

## License

MIT License

## Contributing

Please report bugs and feature requests through GitHub Issues. Pull requests are welcome.
