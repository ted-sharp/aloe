using Aloe.Common.AloeCoreLib.Security;

namespace Aloe.Tests.AloeCoreLibTest.Security;

public class PasswordHasherTests
{
    [Fact]
    public void 同じパスワードのとき_もう一度パスワードを生成した場合_異なる値になる()
    {
        // Arrange(前提)
        // 同じパスワードのとき
        var password = "TestPassword123!";

        // Act(操作)
        // ハッシュとソルトを生成した場合
        var result1 = PasswordHasher.Default.HashPassword(password);
        var result2 = PasswordHasher.Default.HashPassword(password);

        // Assert(結果)
        Assert.NotNull(result1.Hash);
        Assert.NotNull(result1.Salt);
        Assert.NotNull(result2.Hash);
        Assert.NotNull(result2.Salt);

        // 異なる値になる
        Assert.NotEqual(result1.Hash, result2.Hash);
        Assert.NotEqual(result1.Salt, result2.Salt);
    }

    [Theory]
    [InlineData("password1")]
    [InlineData("P@ssw0rd!")]
    [InlineData("1234567890")]
    public void パスワードをハッシュ化したとき_パスワードを検証した場合_検証に成功する(string password)
    {
        // Arrange
        // パスワードをハッシュ化したとき
        var (hash, salt) = PasswordHasher.Default.HashPassword(password);

        // Act
        // パスワードを検証した場合
        var result = PasswordHasher.Default.VerifyPassword(password, hash, salt);

        // Assert
        // 検証に成功する
        Assert.True(result);
    }

    [Fact]
    public void パスワードを検証するとき_間違ったパスワードの場合_検証に失敗する()
    {
        // Arrange
        // パスワードを検証するとき
        var originalPassword = "CorrectPassword";
        var (hash, salt) = PasswordHasher.Default.HashPassword(originalPassword);

        // Act
        // 間違ったパスワードの場合
        var wrongPassword = "WrongPassword";
        var result = PasswordHasher.Default.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        // 検証に失敗する
        Assert.False(result);
    }

    [Fact]
    public void パスワードを検証するとき_間違ったソルトの場合_検証に失敗する()
    {
        // Arrange
        // パスワードを検証するとき
        var originalPassword = "CorrectPassword";
        var (hash, salt) = PasswordHasher.Default.HashPassword(originalPassword);

        // Act
        var corruptedSaltBytes = Convert.FromBase64String(salt);
        // BASE64形式が必要なので元のソルトを書き換える
        corruptedSaltBytes[0] ^= 0xFF;

        // 間違ったソルトの場合
        var wrongSalt = Convert.ToBase64String(corruptedSaltBytes);
        var result = PasswordHasher.Default.VerifyPassword(originalPassword, hash, wrongSalt);

        // Assert
        // 検証に失敗する
        Assert.False(result);
    }
}
