using System;
using System.Security.Cryptography;
using System.Text;

namespace AloeReservationGrid.Lib.CoreLib.Security;

public class PasswordHasher
{
    private static readonly Lazy<PasswordHasher> s_default = new Lazy<PasswordHasher>(() => new PasswordHasher
    {
        SaltSize = 16,
        HashSize = 32,
        Iterations = 10000,
    });

    public static PasswordHasher Default => PasswordHasher.s_default.Value;

    /// <summary>
    /// 推奨されるソルトサイズです。
    /// </summary>
    /// <remarks>
    /// 16バイト程度が標準的です。
    /// </remarks>
    public required int SaltSize { get; set; }

    /// <summary>
    /// ハッシュサイズです。
    /// </summary>
    /// <remarks>
    /// 32バイトでSHA256の出力サイズです。
    /// </remarks>
    public required int HashSize { get; set; }

    /// <summary>
    /// ストレッチングの回数です。
    /// </summary>
    /// <remarks>
    /// 回数を多くすることで、ブルートフォース攻撃の耐性を高めますが、サーバー負荷も増加するため、10,000回程度から始めると良いです。
    /// </remarks>
    public required int Iterations { get; set; }

    /// <summary>
    /// 新しいパスワードのハッシュとソルトを生成します。
    /// </summary>
    public (string Hash, string Salt) HashPassword(string password)
    {
        // ランダムなソルトを生成
        var saltBytes = new byte[this.SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        // パスワードをハッシュ化
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            this.Iterations,
            HashAlgorithmName.SHA256,
            this.HashSize);

        // Base64文字列に変換してハッシュとソルトを返す
        return (Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
    }

    /// <summary>
    /// 入力パスワードを既存のハッシュとソルトで検証します。
    /// </summary>
    public bool VerifyPassword(string password, string hash, string salt)
    {
        // ソルトと比較用ハッシュをバイト配列に変換
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            saltBytes,
            this.Iterations,
            HashAlgorithmName.SHA256,
            this.HashSize);

        // ハッシュが一致するかを比較
        return Convert.ToBase64String(hashBytes) == hash;
    }
}
