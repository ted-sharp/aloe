// <copyright file="PasswordHasher.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;

namespace Aloe.Apps.Medock.MdLauncherLib.Services;

/// <summary>
/// パスワードのハッシュ化と検証を行うサービス。
/// </summary>
/// <remarks>
/// PBKDF2-SHA256 を使用。Salt 16bytes, Hash 32bytes, 10000 iterations。
/// </remarks>
public class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    private static readonly Lazy<PasswordHasher> s_default = new(() => new PasswordHasher());

    /// <summary>
    /// デフォルトインスタンスを取得する。
    /// </summary>
    public static PasswordHasher Default => s_default.Value;

    /// <summary>
    /// パスワードをハッシュ化し、Base64 エンコードされたハッシュとソルトを返す。
    /// </summary>
    public (string Hash, string Salt) HashPassword(string password)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    /// <summary>
    /// パスワードを既存のハッシュとソルトで検証する。
    /// </summary>
    public bool VerifyPassword(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(hashBytes, Convert.FromBase64String(hash));
    }
}
