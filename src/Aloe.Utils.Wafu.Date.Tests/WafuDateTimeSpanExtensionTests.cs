namespace Aloe.Utils.Wafu.Date.Tests;

using Xunit;

/// <summary>
/// TimeSpan拡張のテスト。
/// </summary>
public class WafuDateTimeSpanExtensionTests
{
    [Theory]
    [InlineData(1, 2, 3, 4, "1日 2時間 3分 4秒")]
    [InlineData(0, 2, 0, 4, "2時間 4秒")]
    [InlineData(0, 0, 3, 0, "3分")]
    [InlineData(0, 0, 0, 5, "5秒")]
    public void ToJaString_ComposesUnits(int d, int h, int m, int s, string expected)
    {
        var span = new TimeSpan(d, h, m, s);
        Assert.Equal(expected, span.ToJaString());
    }

    [Theory]
    [InlineData(1, 12, 0, 0, "約2日")]
    [InlineData(0, 1, 30, 0, "約2時間")]
    [InlineData(0, 0, 1, 30, "約2分")]
    [InlineData(0, 0, 0, 45, "45秒")]
    public void ToApproximateJaString_RoundsProperly(int d, int h, int m, int s, string expected)
    {
        var span = new TimeSpan(d, h, m, s);
        Assert.Equal(expected, span.ToApproximateJaString());
    }
}


