# ナレッジベース

## コード修正のルール

### 1. ファイル構成
- ファイルヘッダーは著作権表示を含める(stylecop.jsonを参照のこと)
- プロジェクトでglobal usingが定義されているのでusingディレクティブは必要最小限に
- namespaceはファイルスコープ形式を使用
- usingディレクティブはnamespaceの前に配置する（スコープを考慮）

### 2. メソッドの配置
- publicメソッドを先に配置
- privateメソッドを後に配置

### 3. プロジェクト設定
- .NET 9のライブラリを参照
- パッケージの説明文とタグは適切に設定
- READMEの参照は`README_Package.md`を使用

### 4. 警告対応
- StyleCopの警告は順序に従って修正
- AOT/Trim警告はソースジェネレーションの導入を検討
- 不要な参照は削除

### 5. 型名の使い分け
- ローカル変数・パラメータ・フィールド・プロパティの型宣言にはC#の組み込み型エイリアス（例：string, int, bool）を使う
- 静的メンバーや定数参照時はCLR型名（例：String.Empty, Int32.MaxValue）を使う
- 【良い例】
    - string s = String.Empty;
    - int max = Int32.MaxValue;
    - bool flag = false;
- 【悪い例】
    - String s = String.Empty;
    - Int32 max = 0;
    - Boolean flag = Boolean.FalseString;

### 6. テストプロジェクトのルール
- テストプロジェクトにはStyleCopや静的解析アナライザーの適用は不要

### 7. 複雑な文字列リテラルの記述
- C# 11（.NET 7）以降で利用可能なraw string literals（ヒアドキュメント記法、"""..."""）は、テストコード・プロダクションコード問わず、JSON・SQL・正規表現など複雑な文字列を扱う際に積極的に使用すること。特に.NET 9など新しめのバージョンでは標準的に対応しているため、可読性・保守性向上のために活用する。

## 修正履歴

### 2024-03-XX
- **JsonExtensions.cs**
  - 不要なメソッドを削除
  - StyleCopの順序警告を解消
  - ファイルヘッダーを修正
  - usingディレクティブを整理
  - namespaceをファイルスコープ形式に変更

- **Aloe.Utils.Json.csproj**
  - READMEの参照を修正
  - .NET 9のライブラリを参照
  - パッケージの説明文とタグを更新

## 今後の課題
- System.Text.JsonのAOT/Trim警告の対応
- サンプルプロジェクトとテストプロジェクトの不要な参照の削除
- その他のStyleCop警告の確認と修正 
