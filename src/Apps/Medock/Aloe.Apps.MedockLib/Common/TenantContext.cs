namespace Aloe.Apps.MedockLib.Common;

/// <summary>
/// テナントコンテキストの値オブジェクト
/// ユーザーに関連するテナント、施設、ユーザーID をまとめて扱う
/// </summary>
public record TenantContext(
    /// <summary>テナントID（null の場合はシステム管理者など）</summary>
    Guid? TenantId,

    /// <summary>施設ID（null の場合はテナント全体を対象）</summary>
    Guid? FacilityId,

    /// <summary>ユーザーID（null の場合はコンテキスト未初期化）</summary>
    Guid? UserId);
