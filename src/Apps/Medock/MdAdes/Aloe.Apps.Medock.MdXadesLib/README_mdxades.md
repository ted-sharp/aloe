# Aloe.Apps.Medock.MdXades

ファイルハッシュに対する XAdES-BES / XAdES-T 電子署名の生成・検証を行うツール群。Detached 署名（外部ファイル参照）方式で、SHA256 / SHA384 / SHA512 に対応。RFC 3161 タイムスタンプの埋め込みにも対応。

## 必要な環境

- .NET 10.0
- RFC 3161 タイムスタンプサーバー（XAdES-T 使用時）

## ソリューション構成

| プロジェクト | 説明 |
|-------------|------|
| **Aloe.Apps.Medock.MdXadesApi** | REST / gRPC API サーバー（MagicOnion） |
| **Aloe.Apps.Medock.MdXadesCli** | CLI ツール（未実装） |
| **Aloe.Apps.Medock.MdXadesLib** | コアライブラリ（署名生成・検証・証明書管理） |
| **Aloe.Apps.Medock.MdXadesLib.Contracts** | gRPC サービス定義（MagicOnion インターフェース） |
| **Aloe.Apps.Medock.MdXadesLib.Tests** | xUnit テスト |

## ビルド・実行

```powershell
# ビルド
dotnet build src/Apps/Medock/MdAdes/Aloe.Apps.Medock.MdXadesApi/

# API サーバー起動
dotnet run --project src/Apps/Medock/MdAdes/Aloe.Apps.Medock.MdXadesApi/

# テスト実行
dotnet test src/Apps/Medock/MdAdes/Aloe.Apps.Medock.MdXadesLib.Tests/
```

### タイムスタンプサーバー（Docker）

XAdES-T 署名にはタイムスタンプサーバーが必要です。

```powershell
cd src/Apps/Medock/MdAdes/
docker-compose up
```

---

## REST API の使い方

`Aloe.Apps.Medock.MdXadesApi` を起動すると以下のエンドポイントが利用できます。

| メソッド | パス | 説明 |
|---------|------|------|
| `POST` | `/api/signatures` | ファイルハッシュに XAdES 署名を生成 |
| `GET` | `/api/signatures` | 署名一覧を取得 |
| `GET` | `/api/signatures/{id}/download` | 署名 XML をダウンロード |
| `POST` | `/api/signatures/verify` | 署名 XML を検証 |
| `GET` | `/api/signatures/certificate` | 署名証明書の情報を取得 |

### POST /api/signatures — 署名生成

```json
{
  "hashAlgorithm": "SHA256",
  "hashValue": "Base64エンコードされたハッシュ値",
  "fileName": "document.pdf",
  "mimeType": "application/pdf"
}
```

レスポンス:

```json
{
  "signatureId": "550e8400-e29b-41d4-a716-446655440000",
  "signedAt": "2024-03-15T14:30:45.1234567Z",
  "xmlBase64": "PGRzOlNpZ25hdHVyZSB4bWxucz..."
}
```

### POST /api/signatures/verify — 署名検証

```json
{
  "xmlBase64": "Base64エンコードされた署名XML"
}
```

レスポンス:

```json
{
  "isValid": true,
  "signedAt": "2024-03-15T14:30:45Z",
  "signerSubject": "CN=MdXades Dev",
  "hasTimestamp": true,
  "timestampedAt": "2024-03-15T14:30:46Z",
  "errorMessage": null
}
```

---

## gRPC の使い方（MagicOnion）

`IXadesService` インターフェース（`Aloe.Apps.Medock.MdXadesLib.Contracts`）経由で REST と同等の機能を gRPC で利用できます。

| メソッド | 説明 |
|---------|------|
| `SignAsync` | ファイルハッシュに XAdES 署名を生成 |
| `VerifyAsync` | 署名 XML を検証 |
| `GetSignatureAsync` | 署名を ID で取得 |
| `ListSignaturesAsync` | 署名一覧を取得 |
| `GetCertificateInfoAsync` | 署名証明書の情報を取得 |

### ポート構成

| プロトコル | URL | 用途 |
|-----------|-----|------|
| HTTP/1 | `http://localhost:5080` | REST API |
| HTTP/2 | `http://localhost:5081` | gRPC |
| HTTP | `http://localhost:8318` | RFC 3161 TSA（Docker） |

---

## 設定（appsettings.json）

```json
{
  "XAdES": {
    "UseDevCertificate": true,
    "TimestampServerUrl": "http://localhost:8318/",
    "EnableTimestamp": true,
    "OutputPath": "signatures",
    "MaxSignatureAgeMins": 1440
  }
}
```

| キー | 型 | デフォルト | 説明 |
|-----|----|-----------|------|
| `UseDevCertificate` | bool | `true` | 自己署名の開発用証明書を自動生成して使用 |
| `CertificatePath` | string? | `null` | PFX ファイルのパス（本番環境用） |
| `CertificatePassword` | string? | `null` | PFX ファイルのパスワード |
| `TimestampServerUrl` | string | `http://localhost:8318/` | RFC 3161 タイムスタンプサーバーの URL |
| `EnableTimestamp` | bool | `true` | タイムスタンプを埋め込む（XAdES-T） |
| `OutputPath` | string | `signatures` | 署名 XML ファイルの保存ディレクトリ |
| `MaxSignatureAgeMins` | int | `1440` | メモリ上の署名メタデータの保持期間（分） |

---

## アーキテクチャ

### レイヤー構成

```
API (Controller / gRPC)
  ↓
IXadesSigner (XadesSigningService)
  ├── ICertificateProvider  … 証明書の取得
  ├── ITimestampClient      … RFC 3161 タイムスタンプ取得
  ├── XadesXmlBuilder       … XAdES XML の構築・署名
  └── XadesXmlVerifier      … XAdES XML の検証
```

### 主要クラス

| クラス | 役割 |
|-------|------|
| `XadesSigningService` | 署名・検証のオーケストレーター |
| `XadesXmlBuilder` | XAdES-BES XML の構築、タイムスタンプの埋め込み |
| `XadesXmlVerifier` | 署名 XML の検証（証明書抽出・正準化・RSA 検証） |
| `DevCertificateProvider` | 起動時に自己署名証明書を自動生成（開発用） |
| `FileCertificateProvider` | PFX ファイルから証明書を読み込み（本番用） |
| `Rfc3161TimestampClient` | RFC 3161 TSA サーバーへの HTTP クライアント |
| `SignatureStore` | インメモリ署名メタデータストア（自動クリーンアップ付き） |

### ライブラリとして利用

DI 拡張メソッドで組み込めます:

```csharp
// 開発用証明書 + RFC 3161 タイムスタンプクライアント
services.AddMdXades();

// PFX ファイル証明書 + RFC 3161 タイムスタンプクライアント
services.AddMdXadesWithFileCert();
```

---

## テスト

```powershell
dotnet test src/Apps/Medock/MdAdes/Aloe.Apps.Medock.MdXadesLib.Tests/
```

| テストクラス | テスト内容 |
|-------------|-----------|
| `XadesSigningServiceTests` | 署名生成、署名→検証ラウンドトリップ、全アルゴリズム対応 |
| `XadesXmlBuilderTests` | XML 構造の検証、タイムスタンプ埋め込み |
| `XadesXmlVerifierTests` | 正常署名の検証、改ざん検知、不正 XML のエラー処理 |
