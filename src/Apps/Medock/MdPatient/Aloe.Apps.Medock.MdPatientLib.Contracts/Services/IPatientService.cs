// <copyright file="IPatientService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using MagicOnion;
using MessagePack;

namespace Aloe.Apps.Medock.MdPatientLib.Contracts.Services;

/// <summary>患者検索リクエスト。</summary>
[MessagePackObject]
public class SearchPatientsRequest
{
    /// <summary>検索キーワード（名前・コード等）。</summary>
    [Key(0)]
    public string Keyword { get; set; } = string.Empty;

    /// <summary>施設 ID（任意）。</summary>
    [Key(1)]
    public Guid? FacilityId { get; set; }

    /// <summary>ページ番号（1 始まり）。</summary>
    [Key(2)]
    public int Page { get; set; } = 1;

    /// <summary>ページサイズ。</summary>
    [Key(3)]
    public int PageSize { get; set; } = 50;
}

/// <summary>患者検索レスポンス。</summary>
[MessagePackObject]
public class SearchPatientsResponse
{
    /// <summary>検索結果一覧。</summary>
    [Key(0)]
    public List<PatientSummary> Items { get; set; } = [];

    /// <summary>検索結果の総件数。</summary>
    [Key(1)]
    public int TotalCount { get; set; }
}

/// <summary>患者サマリー（一覧表示用）。</summary>
[MessagePackObject]
public class PatientSummary
{
    /// <summary>患者 ID。</summary>
    [Key(0)]
    public Guid PtId { get; set; }

    /// <summary>患者コード。</summary>
    [Key(1)]
    public string PtCode { get; set; } = string.Empty;

    /// <summary>患者名。</summary>
    [Key(2)]
    public string PtName { get; set; } = string.Empty;

    /// <summary>患者名カタカナ。</summary>
    [Key(3)]
    public string PtNameKatakana { get; set; } = string.Empty;

    /// <summary>生年月日。</summary>
    [Key(4)]
    public string BirthDate { get; set; } = string.Empty;

    /// <summary>性別コード (0: None, 1: Man, 2: Woman, 9: Unknown)。</summary>
    [Key(5)]
    public int SexCode { get; set; }
}

/// <summary>患者詳細レスポンス。</summary>
[MessagePackObject]
public class PatientDetailResponse
{
    /// <summary>患者 ID。</summary>
    [Key(0)]
    public Guid PtId { get; set; }

    /// <summary>正規患者 ID（名寄せ用）。</summary>
    [Key(1)]
    public Guid CanonicalPtId { get; set; }

    /// <summary>施設 ID。</summary>
    [Key(2)]
    public Guid FacilityId { get; set; }

    /// <summary>主団体 ID。</summary>
    [Key(3)]
    public Guid PrimaryOrgId { get; set; }

    /// <summary>患者コード。</summary>
    [Key(4)]
    public string PtCode { get; set; } = string.Empty;

    /// <summary>カルテコード。</summary>
    [Key(5)]
    public string? KarteCode { get; set; }

    /// <summary>患者名。</summary>
    [Key(6)]
    public string PtName { get; set; } = string.Empty;

    /// <summary>患者名カタカナ。</summary>
    [Key(7)]
    public string PtNameKatakana { get; set; } = string.Empty;

    /// <summary>旧姓名。</summary>
    [Key(8)]
    public string PtMaidenName { get; set; } = string.Empty;

    /// <summary>別名・芸名。</summary>
    [Key(9)]
    public string PtAliasName { get; set; } = string.Empty;

    /// <summary>生年月日。</summary>
    [Key(10)]
    public string BirthDate { get; set; } = string.Empty;

    /// <summary>性別コード。</summary>
    [Key(11)]
    public int SexCode { get; set; }

    /// <summary>メモ。</summary>
    [Key(12)]
    public string PtMemo { get; set; } = string.Empty;

    /// <summary>削除フラグ。</summary>
    [Key(13)]
    public bool IsDeleted { get; set; }
}

/// <summary>患者取得リクエスト。</summary>
[MessagePackObject]
public class GetPatientRequest
{
    /// <summary>患者 ID。</summary>
    [Key(0)]
    public Guid PtId { get; set; }
}

/// <summary>患者作成リクエスト。</summary>
[MessagePackObject]
public class CreatePatientRequest
{
    /// <summary>施設 ID。</summary>
    [Key(0)]
    public Guid FacilityId { get; set; }

    /// <summary>主団体 ID。</summary>
    [Key(1)]
    public Guid PrimaryOrgId { get; set; }

    /// <summary>患者コード。</summary>
    [Key(2)]
    public string PtCode { get; set; } = string.Empty;

    /// <summary>カルテコード。</summary>
    [Key(3)]
    public string? KarteCode { get; set; }

    /// <summary>患者名。</summary>
    [Key(4)]
    public string PtName { get; set; } = string.Empty;

    /// <summary>患者名カタカナ。</summary>
    [Key(5)]
    public string PtNameKatakana { get; set; } = string.Empty;

    /// <summary>旧姓名。</summary>
    [Key(6)]
    public string PtMaidenName { get; set; } = string.Empty;

    /// <summary>別名・芸名。</summary>
    [Key(7)]
    public string PtAliasName { get; set; } = string.Empty;

    /// <summary>生年月日。</summary>
    [Key(8)]
    public string BirthDate { get; set; } = string.Empty;

    /// <summary>性別コード。</summary>
    [Key(9)]
    public int SexCode { get; set; }

    /// <summary>メモ。</summary>
    [Key(10)]
    public string PtMemo { get; set; } = string.Empty;
}

/// <summary>患者更新リクエスト。</summary>
[MessagePackObject]
public class UpdatePatientRequest
{
    /// <summary>患者 ID。</summary>
    [Key(0)]
    public Guid PtId { get; set; }

    /// <summary>患者コード。</summary>
    [Key(1)]
    public string PtCode { get; set; } = string.Empty;

    /// <summary>カルテコード。</summary>
    [Key(2)]
    public string? KarteCode { get; set; }

    /// <summary>患者名。</summary>
    [Key(3)]
    public string PtName { get; set; } = string.Empty;

    /// <summary>患者名カタカナ。</summary>
    [Key(4)]
    public string PtNameKatakana { get; set; } = string.Empty;

    /// <summary>旧姓名。</summary>
    [Key(5)]
    public string PtMaidenName { get; set; } = string.Empty;

    /// <summary>別名・芸名。</summary>
    [Key(6)]
    public string PtAliasName { get; set; } = string.Empty;

    /// <summary>生年月日。</summary>
    [Key(7)]
    public string BirthDate { get; set; } = string.Empty;

    /// <summary>性別コード。</summary>
    [Key(8)]
    public int SexCode { get; set; }

    /// <summary>メモ。</summary>
    [Key(9)]
    public string PtMemo { get; set; } = string.Empty;
}

/// <summary>患者削除リクエスト。</summary>
[MessagePackObject]
public class DeletePatientRequest
{
    /// <summary>患者 ID。</summary>
    [Key(0)]
    public Guid PtId { get; set; }
}

/// <summary>患者削除レスポンス。</summary>
[MessagePackObject]
public class DeletePatientResponse
{
    /// <summary>削除が成功したかどうか。</summary>
    [Key(0)]
    public bool Success { get; set; }
}

/// <summary>
/// 患者サービスの gRPC インターフェース。
/// </summary>
public interface IPatientService : IService<IPatientService>
{
    /// <summary>患者を検索する。</summary>
    UnaryResult<SearchPatientsResponse> SearchAsync(SearchPatientsRequest request);

    /// <summary>患者を ID で取得する。</summary>
    UnaryResult<PatientDetailResponse> GetByIdAsync(GetPatientRequest request);

    /// <summary>患者を新規作成する。</summary>
    UnaryResult<PatientDetailResponse> CreateAsync(CreatePatientRequest request);

    /// <summary>患者を更新する。</summary>
    UnaryResult<PatientDetailResponse> UpdateAsync(UpdatePatientRequest request);

    /// <summary>患者を削除する。</summary>
    UnaryResult<DeletePatientResponse> DeleteAsync(DeletePatientRequest request);
}
