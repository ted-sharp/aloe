# Aloe.Apps.Medock.MdPatient

患者情報の検索・閲覧・編集を行うデスクトップアプリケーション群。WPF クライアント（Finder / Viewer / Editor）と gRPC API サーバーで構成され、Named Pipe による IPC でアプリ間を連携する。

## 必要な環境

- .NET 10.0
- PostgreSQL

## ソリューション構成

| プロジェクト | 説明 |
|-------------|------|
| **Aloe.Apps.Medock.MdPatientApi** | REST / gRPC API サーバー（MagicOnion） |
| **Aloe.Apps.Medock.MdPatientLib** | コアライブラリ（EF Core DbContext・ビジネスロジック） |
| **Aloe.Apps.Medock.MdPatientLib.Contracts** | gRPC サービス定義（MagicOnion インターフェース・DTO） |
| **Aloe.Apps.Medock.MdPatientLib.Tests** | xUnit テスト |
| **Aloe.Apps.Medock.MdPatientFinder** | WPF — 患者検索・一覧表示、Viewer / Editor の起動 |
| **Aloe.Apps.Medock.MdPatientViewer** | WPF — 患者情報の閲覧（読み取り専用） |
| **Aloe.Apps.Medock.MdPatientEditor** | WPF — 患者情報の新規作成・編集 |

## ビルド・実行

```powershell
# ビルド
dotnet build src/Apps/Medock/MdPatient/Aloe.Apps.Medock.MdPatientApi/

# API サーバー起動
dotnet run --project src/Apps/Medock/MdPatient/Aloe.Apps.Medock.MdPatientApi/

# WPF アプリ起動
dotnet run --project src/Apps/Medock/MdPatient/Aloe.Apps.Medock.MdPatientFinder/

# テスト実行
dotnet test src/Apps/Medock/MdPatient/Aloe.Apps.Medock.MdPatientLib.Tests/
```

---

## gRPC の使い方（MagicOnion）

`IPatientService` インターフェース（`Aloe.Apps.Medock.MdPatientLib.Contracts`）経由で患者情報の CRUD を提供します。

| メソッド | 説明 |
|---------|------|
| `SearchAsync` | キーワード検索（カナ名・患者コード）、ページネーション対応 |
| `GetByIdAsync` | 患者 ID で詳細取得 |
| `CreateAsync` | 患者を新規作成 |
| `UpdateAsync` | 患者情報を更新 |
| `DeleteAsync` | 患者を論理削除 |

### ポート構成

| プロトコル | URL | 用途 |
|-----------|-----|------|
| HTTP/1 | `http://localhost:5090` | REST API |
| HTTP/2 | `http://localhost:5091` | gRPC |

---

## 設定（appsettings.json）

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=mdpatient;Username=postgres;Password=postgres"
  }
}
```

---

## アーキテクチャ

### レイヤー構成

```
WPF クライアント (Finder / Viewer / Editor)
  ↓ gRPC (MagicOnion)
API サーバー (MdPatientApi)
  ↓
ビジネスロジック (MdPatientLib)
  ↓ EF Core
PostgreSQL
```

### アプリ間連携

Finder から Viewer / Editor を起動する際、Named Pipe で IPC を行います。

```
Finder
  ├─→ Named Pipe "MdPatient_Viewer" → Viewer（既に起動中なら患者IDを送信）
  └─→ Named Pipe "MdPatient_Editor" → Editor（既に起動中なら患者IDを送信）
       ※ 接続失敗時は exe を --patient-id 引数付きで新規起動
```

Named Pipe のプロトコルは JSON 形式です:

```json
{ "action": "open", "patientId": "550e8400-e29b-41d4-a716-446655440000" }
```

### 主要クラス

| クラス | 役割 |
|-------|------|
| `PatientService` | 患者 CRUD のビジネスロジック（検索・論理削除対応） |
| `MdPatientDbContext` | EF Core DbContext（QueryFilter による論理削除フィルタ） |
| `Patient` | 患者エンティティ（患者コード・氏名・カナ・生年月日・性別等） |
| `GrpcPatientService` | MagicOnion gRPC サービス実装 |
| `ViewerLauncher` | Finder から Viewer / Editor を Named Pipe または exe 起動で呼び出す |
| `PipeServer` | Named Pipe サーバー（Viewer / Editor 側で患者 ID の受信を待機） |

### WPF アプリの MVVM 構成

各 WPF アプリは CommunityToolkit.Mvvm を使用した MVVM パターンです。

| アプリ | ViewModel | 主な機能 |
|-------|-----------|---------|
| Finder | `MainViewModel` | キーワード検索、DataGrid 一覧表示、Viewer / Editor 起動 |
| Viewer | `MainViewModel` | 患者詳細の読み取り専用表示 |
| Editor | `MainViewModel` | 患者の新規作成・編集・保存 |

---

## データモデル

### Patient エンティティ

| フィールド | 型 | 説明 |
|-----------|-----|------|
| `PtId` | Guid | 主キー |
| `CanonicalPtId` | Guid | 名寄せ用統合 ID |
| `FacilityId` | Guid | 施設 ID |
| `PrimaryOrgId` | Guid | 主組織 ID |
| `PtCode` | string | 患者コード |
| `KarteCode` | string? | カルテコード |
| `PtName` | string | 患者氏名 |
| `PtNameKatakana` | string | 患者氏名（カタカナ） |
| `BirthDate` | DateOnly | 生年月日 |
| `SexCode` | int | 性別（0: なし, 1: 男, 2: 女, 9: 不明） |
| `PtMemo` | string? | メモ |
| `IsDeleted` | bool | 論理削除フラグ |

---

## テスト

```powershell
dotnet test src/Apps/Medock/MdPatient/Aloe.Apps.Medock.MdPatientLib.Tests/
```

| テストクラス | テスト内容 |
|-------------|-----------|
| `PatientServiceTests` | 新規作成、ID 検索、キーワード検索・ページネーション、更新、論理削除 |
