// <copyright file="ILoginService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using MagicOnion;
using MessagePack;

namespace Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;

/// <summary>ログインリクエスト。</summary>
[MessagePackObject]
public class LoginRequest
{
    /// <summary>ユーザーコード。</summary>
    [Key(0)]
    public string UserCode { get; set; } = string.Empty;

    /// <summary>パスワード。</summary>
    [Key(1)]
    public string Password { get; set; } = string.Empty;
}

/// <summary>ログインレスポンス。</summary>
[MessagePackObject]
public class LoginResponse
{
    /// <summary>認証成功フラグ。</summary>
    [Key(0)]
    public bool Success { get; set; }

    /// <summary>ユーザーコード。</summary>
    [Key(1)]
    public string UserCode { get; set; } = string.Empty;

    /// <summary>ユーザー ID。</summary>
    [Key(2)]
    public Guid UserId { get; set; }

    /// <summary>セッションキー。</summary>
    [Key(3)]
    public Guid SessionKey { get; set; }

    /// <summary>エラーメッセージ。</summary>
    [Key(4)]
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>発行日時。</summary>
    [Key(5)]
    public DateTimeOffset IssuedAt { get; set; }
}

/// <summary>
/// ログインサービスの gRPC インターフェース。
/// </summary>
public interface ILoginService : IService<ILoginService>
{
    /// <summary>ログイン認証を行う。</summary>
    UnaryResult<LoginResponse> LoginAsync(LoginRequest request);
}
