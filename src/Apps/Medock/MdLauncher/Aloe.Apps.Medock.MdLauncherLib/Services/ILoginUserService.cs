// <copyright file="ILoginUserService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.Medock.MdLauncherLib.Services;

/// <summary>
/// ユーザーログイン検証サービスのインターフェース。
/// </summary>
public interface ILoginUserService
{
    /// <summary>
    /// ユーザーコードとパスワードで認証を行う。
    /// </summary>
    Task<(bool Success, Guid UserId, string ErrorMessage)> ValidateAsync(string userCode, string password);
}
