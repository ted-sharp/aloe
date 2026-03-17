// <copyright file="LoginUserService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdLauncherLib.Services;

/// <summary>
/// ユーザーログイン検証サービスの実装。
/// </summary>
public class LoginUserService : ILoginUserService
{
    private readonly MdLauncherDbContext _db;

    /// <summary>
    /// <see cref="LoginUserService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public LoginUserService(MdLauncherDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<(bool Success, Guid UserId, string ErrorMessage)> ValidateAsync(string userCode, string password)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.UserCode == userCode);

        if (user is null)
        {
            return (false, Guid.Empty, "ユーザーコードまたはパスワードが正しくありません。");
        }

        if (string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.PasswordSalt))
        {
            return (false, Guid.Empty, "パスワードが設定されていません。");
        }

        var valid = PasswordHasher.Default.VerifyPassword(password, user.PasswordHash, user.PasswordSalt);
        if (!valid)
        {
            return (false, Guid.Empty, "ユーザーコードまたはパスワードが正しくありません。");
        }

        return (true, user.UserId, string.Empty);
    }
}
