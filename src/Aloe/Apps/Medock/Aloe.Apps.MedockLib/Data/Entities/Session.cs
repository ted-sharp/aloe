namespace Aloe.Apps.MedockLib.Data.Entities;

/// <summary>
/// セッションエンティティ
/// </summary>
public class Session
{
    /// <summary>セッションID (PK)</summary>
    public Guid SessionId { get; set; }

    /// <summary>ユーザーID (FK)</summary>
    public Guid UserId { get; set; }

    /// <summary>ユーザー表示名</summary>
    public string UserDisplayName { get; set; } = String.Empty;

    /// <summary>クライアントアプリ名（バージョン含む）</summary>
    public string ClientAppName { get; set; } = String.Empty;

    /// <summary>クライアントエンドポイント（IP, Port含む）</summary>
    public string ClientEndpoint { get; set; } = String.Empty;

    /// <summary>ログイン日時</summary>
    public DateTimeOffset LoginAt { get; set; }

    /// <summary>ログアウト日時</summary>
    public DateTimeOffset? LogoutAt { get; set; }

    // Navigation Properties
    public virtual User User { get; set; } = null!;
}


