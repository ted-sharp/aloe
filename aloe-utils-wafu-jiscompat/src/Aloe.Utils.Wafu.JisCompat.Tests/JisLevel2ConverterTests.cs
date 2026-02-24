namespace Aloe.Utils.Wafu.JisCompat.Tests;

using Xunit;

/// <summary>
/// JisLevel2Converterのテストクラス
/// </summary>
public class JisLevel2ConverterTests
{
    /// <summary>
    /// 静的コンストラクタ
    /// </summary>
    static JisLevel2ConverterTests()
    {
        // テスト実行前にマッピングテーブルを初期化
        _ = JisLevel2Converter.ConvertToLevel2("髙");
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("普通の文字ABC123", "普通の文字ABC123")]
    [InlineData("髙﨑神社", "高崎神社")]
    [InlineData("塚德增寬", "塚徳増寛")]
    [InlineData("殺纊", "殺絋")]
    [InlineData("曻雝", "昇雍")]
    // 氏名でよく使用される文字
    [InlineData("塚", "塚")]
    [InlineData("德", "徳")]
    [InlineData("增", "増")]
    [InlineData("寬", "寛")]
    [InlineData("德川", "徳川")]
    [InlineData("增田", "増田")]
    [InlineData("寬子", "寛子")]
    public void ConvertToLevel2_変換が正しく行われる(string input, string expected)
    {
        // Arrange
        // Act
        var actual = JisLevel2Converter.ConvertToLevel2(input);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    // サロゲートペアを含む文字は変換に失敗する
    [InlineData("𠮟", "叱")]
    [InlineData("𡈽", "土")]
    [InlineData("𡌛", "野")]
    [InlineData("𡸳", "嶢")]
    [InlineData("𢛳", "徳")]
    [InlineData("𣏐", "杓")]
    [InlineData("𣘺", "橋")]
    [InlineData("𤰖", "畝")]
    [InlineData("𥔎", "碕")]
    [InlineData("𦙾", "脛")]
    [InlineData("𦚰", "脇")]
    [InlineData("𦜝", "臍")]
    [InlineData("𦣝", "頤")]
    [InlineData("𦣪", "塩")]
    [InlineData("𦹥", "蔭")]
    [InlineData("𧜎", "襷")]
    [InlineData("𨂻", "蹈")]
    [InlineData("𨥫", "鉚")]
    [InlineData("𨦇", "鋏")]
    [InlineData("𨨞", "斧")]
    [InlineData("𨪙", "鏘")]
    [InlineData("𨫝", "鑵")]
    [InlineData("𨯯", "鑓")]
    [InlineData("𨻫", "隴")]
    [InlineData("𨼲", "蔭")]
    [InlineData("𨿸", "鶏")]
    [InlineData("𩜙", "饒")]
    [InlineData("𩷛", "鯒")]
    [InlineData("𩿎", "鴉")]
    [InlineData("𪗱", "齟")]
    [InlineData("𪘂", "齧")]
    [InlineData("𪘚", "齬")]
    public void ConvertToLevel2_サロゲートペアは変換に失敗する(string input, string expected)
    {
        // Arrange
        // Act
        var actual = JisLevel2Converter.ConvertToLevel2(input);

        // Assert
        Assert.NotEqual(expected, actual); // 変換に失敗することを確認
        Assert.Equal(input, actual); // 元の文字列がそのまま返されることを確認
    }

}
