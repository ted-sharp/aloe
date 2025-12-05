using System.Security.Cryptography;
using System.Text;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// パスワードのハッシュ化と検証を行うサービス
/// </summary>
/// <remarks>
/// PBKDF2（Password-Based Key Derivation Function 2）を使用してパスワードをハッシュ化します。
/// src_oldのPasswordHasherを参考に新規実装。
/// </remarks>
public class PasswordHasher
{
    private static readonly Lazy<PasswordHasher> s_default = new(() => new PasswordHasher
    {
        SaltSize = 16,
        HashSize = 32,
        Iterations = 10000,
    });

    /// <summary>
    /// デフォルトの設定を持つPasswordHasherインスタンスを取得します。
    /// </summary>
    public static PasswordHasher Default => s_default.Value;

    /// <summary>
    /// ソルトのサイズ（バイト）
    /// </summary>
    /// <remarks>
    /// 16バイト程度が標準的です。
    /// </remarks>
    public required int SaltSize { get; init; }

    /// <summary>
    /// ハッシュのサイズ（バイト）
    /// </summary>
    /// <remarks>
    /// 32バイトでSHA256の出力サイズです。
    /// </remarks>
    public required int HashSize { get; init; }

    /// <summary>
    /// ストレッチングの回数
    /// </summary>
    /// <remarks>
    /// 回数を多くすることで、ブルートフォース攻撃の耐性を高めますが、
    /// サーバー負荷も増加するため、10,000回程度から始めると良いです。
    /// </remarks>
    public required int Iterations { get; init; }

    /// <summary>
    /// 新しいパスワードのハッシュとソルトを生成します。
    /// </summary>
    /// <param name="password">ハッシュ化するパスワード</param>
    /// <returns>Base64エンコードされたハッシュとソルトのタプル</returns>
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
    /// <param name="password">検証するパスワード</param>
    /// <param name="hash">比較対象のハッシュ（Base64エンコード）</param>
    /// <param name="salt">ソルト（Base64エンコード）</param>
    /// <returns>パスワードが一致する場合はtrue</returns>
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

        // ハッシュが一致するかを比較（タイミング攻撃対策として固定時間比較を使用）
        return CryptographicOperations.FixedTimeEquals(hashBytes, Convert.FromBase64String(hash));
    }
}


