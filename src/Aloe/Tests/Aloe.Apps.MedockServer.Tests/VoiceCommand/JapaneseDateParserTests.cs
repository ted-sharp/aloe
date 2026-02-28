using Aloe.Apps.MedockLib.Services.VoiceCommand;
using FluentAssertions;

namespace Aloe.Apps.MedockServer.Tests.VoiceCommand;

public class JapaneseDateParserTests
{
    // テスト基準日: 2026年2月28日（土曜日）
    private static readonly DateOnly Today = new(2026, 2, 28);

    // ── 相対日付 ──

    [Fact]
    public void ParseDate_今日_ReturnsTodayDate()
    {
        JapaneseDateParser.ParseDate("今日の10時", Today).Should().Be(Today);
    }

    [Fact]
    public void ParseDate_明日_ReturnsTomorrow()
    {
        JapaneseDateParser.ParseDate("明日の予約", Today).Should().Be(Today.AddDays(1));
    }

    [Fact]
    public void ParseDate_明後日_ReturnsDayAfterTomorrow()
    {
        JapaneseDateParser.ParseDate("明後日に予約", Today).Should().Be(Today.AddDays(2));
    }

    // ── 絶対日付 ──

    [Fact]
    public void ParseDate_月日指定_ReturnsCorrectDate()
    {
        JapaneseDateParser.ParseDate("3月5日に予約", Today)
            .Should().Be(new DateOnly(2026, 3, 5));
    }

    [Fact]
    public void ParseDate_年月日指定_ReturnsExactDate()
    {
        JapaneseDateParser.ParseDate("2026年3月5日の予約", Today)
            .Should().Be(new DateOnly(2026, 3, 5));
    }

    [Fact]
    public void ParseDate_過去の月日_推定来年()
    {
        // 2月28日時点で「1月10日」→ 来年
        JapaneseDateParser.ParseDate("1月10日の予約", Today)
            .Should().Be(new DateOnly(2027, 1, 10));
    }

    // ── 曜日指定 ──

    [Fact]
    public void ParseDate_来週の月曜日_Returns来週月曜()
    {
        // 今日=2026/2/28(土) → 来週月曜=3/2
        var result = JapaneseDateParser.ParseDate("来週の月曜日", Today);
        result.Should().NotBeNull();
        result!.Value.DayOfWeek.Should().Be(DayOfWeek.Monday);
        result.Value.Should().Be(new DateOnly(2026, 3, 2));
    }

    [Fact]
    public void ParseDate_今週金曜_Returns今週金曜()
    {
        // 今日=2026/2/28(土) → 今週金曜=来週の金曜(3/6)
        var result = JapaneseDateParser.ParseDate("今週金曜", Today);
        result.Should().NotBeNull();
        result!.Value.DayOfWeek.Should().Be(DayOfWeek.Friday);
    }

    [Fact]
    public void ParseDate_再来週水曜日_Returns再来週水曜()
    {
        var result = JapaneseDateParser.ParseDate("再来週水曜日", Today);
        result.Should().NotBeNull();
        result!.Value.DayOfWeek.Should().Be(DayOfWeek.Wednesday);
        // 今日(土)→来週月曜(3/2)+7+2=3/11
        result.Value.Should().Be(new DateOnly(2026, 3, 11));
    }

    // ── 時刻 ──

    [Theory]
    [InlineData("10時", 600)]
    [InlineData("10時半", 630)]
    [InlineData("10時30分", 630)]
    [InlineData("9時", 540)]
    [InlineData("2時", 840)]      // 1〜7時は午後と推定
    [InlineData("3時半", 930)]    // 1〜7時は午後と推定
    [InlineData("8時", 480)]      // 8時はそのまま（業務時間内）
    public void ParseTime_時刻表現_Returns分数(string text, int expectedMin)
    {
        JapaneseDateParser.ParseTime(text).Should().Be(expectedMin);
    }

    [Theory]
    [InlineData("午前10時", 600)]
    [InlineData("午前10時半", 630)]
    [InlineData("午前9時30分", 570)]
    public void ParseTime_午前表現_Returns分数(string text, int expectedMin)
    {
        JapaneseDateParser.ParseTime(text).Should().Be(expectedMin);
    }

    [Theory]
    [InlineData("午後3時", 900)]
    [InlineData("午後3時半", 930)]
    [InlineData("午後3時30分", 930)]
    [InlineData("午後1時", 780)]
    public void ParseTime_午後表現_Returns分数(string text, int expectedMin)
    {
        JapaneseDateParser.ParseTime(text).Should().Be(expectedMin);
    }

    [Fact]
    public void ParseTime_時刻なし_ReturnsNull()
    {
        JapaneseDateParser.ParseTime("明日の予約を確認して").Should().BeNull();
    }

    // ── 相対日付（追加） ──

    [Fact]
    public void ParseDate_一昨日_Returns2日前()
    {
        JapaneseDateParser.ParseDate("一昨日の予約", Today).Should().Be(Today.AddDays(-2));
    }

    [Fact]
    public void ParseDate_昨日_Returns1日前()
    {
        JapaneseDateParser.ParseDate("昨日の予約", Today).Should().Be(Today.AddDays(-1));
    }

    [Fact]
    public void ParseDate_三日後_Returns3日後()
    {
        JapaneseDateParser.ParseDate("三日後に予約", Today).Should().Be(Today.AddDays(3));
    }

    // ── 来月・先月 ──

    [Fact]
    public void ParseDate_来月の5日_Returns来月5日()
    {
        // 2026/2/28 → 来月 = 3月
        JapaneseDateParser.ParseDate("来月の5日に予約", Today)
            .Should().Be(new DateOnly(2026, 3, 5));
    }

    [Fact]
    public void ParseDate_来月1日_Returns来月1日()
    {
        JapaneseDateParser.ParseDate("来月1日の予約", Today)
            .Should().Be(new DateOnly(2026, 3, 1));
    }

    [Fact]
    public void ParseDate_先月10日_Returns先月10日()
    {
        JapaneseDateParser.ParseDate("先月10日の予約", Today)
            .Should().Be(new DateOnly(2026, 1, 10));
    }

    [Fact]
    public void ParseDate_来月のみ_Returns来月1日()
    {
        JapaneseDateParser.ParseDate("来月の予約を確認", Today)
            .Should().Be(new DateOnly(2026, 3, 1));
    }

    [Fact]
    public void ParseDate_再来月_Returns再来月1日()
    {
        JapaneseDateParser.ParseDate("再来月の予約", Today)
            .Should().Be(new DateOnly(2026, 4, 1));
    }

    // ── 月末 ──

    [Fact]
    public void ParseDate_月末_Returns今月末()
    {
        // 2026/2/28 → 今月末 = 2/28
        JapaneseDateParser.ParseDate("月末に予約を入れて", Today)
            .Should().Be(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public void ParseDate_今月末_Returns今月末()
    {
        JapaneseDateParser.ParseDate("今月末に予約を入れて", Today)
            .Should().Be(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public void ParseDate_来月末_Returns来月末()
    {
        // 2026/2/28 → 来月末 = 3/31
        JapaneseDateParser.ParseDate("来月末に予約を入れて", Today)
            .Should().Be(new DateOnly(2026, 3, 31));
    }

    [Fact]
    public void ParseDate_先月末_Returns先月末()
    {
        // 2026/2/28 → 先月末 = 1/31
        JapaneseDateParser.ParseDate("先月末に予約を入れて", Today)
            .Should().Be(new DateOnly(2026, 1, 31));
    }

    [Fact]
    public void ParseDate_再来月末_Returns再来月末()
    {
        // 2026/2/28 → 再来月末 = 4/30
        JapaneseDateParser.ParseDate("再来月末に予約を入れて", Today)
            .Should().Be(new DateOnly(2026, 4, 30));
    }

    [Fact]
    public void ParseDate_来月末_うるう年2月末()
    {
        // 2028/1/15 → 来月末 = 2028/2/29（うるう年）
        var jan2028 = new DateOnly(2028, 1, 15);
        JapaneseDateParser.ParseDate("来月末に予約を入れて", jan2028)
            .Should().Be(new DateOnly(2028, 2, 29));
    }

    // ── 日のみ指定（月なし） ──

    [Theory]
    [InlineData("5日に予約を入れて")]    // 2/28時点で2/5は過去 → 3/5
    [InlineData("5日の予約")]
    public void ParseDate_日のみ_過去の日_Returns翌月(string text)
    {
        // 今日=2026/2/28、5日は過去 → 翌月3月5日
        JapaneseDateParser.ParseDate(text, Today)
            .Should().Be(new DateOnly(2026, 3, 5));
    }

    [Fact]
    public void ParseDate_日のみ_今日_Returns今月()
    {
        // 今日=2026/2/28、28日は今日 → 今月2月28日
        JapaneseDateParser.ParseDate("28日に予約を入れて", Today)
            .Should().Be(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public void ParseDate_日のみ_未来日_Returns今月()
    {
        // 今日=2026/2/10、15日はまだ先 → 今月2月15日
        var feb10 = new DateOnly(2026, 2, 10);
        JapaneseDateParser.ParseDate("15日に予約を入れて", feb10)
            .Should().Be(new DateOnly(2026, 2, 15));
    }

    [Fact]
    public void ParseDate_日のみ_翌月に繰り越し_MaxDayClamped()
    {
        // 今日=2026/3/15、31日は3月31日（まだ先）→ 3月31日
        var mar15 = new DateOnly(2026, 3, 15);
        JapaneseDateParser.ParseDate("31日に予約", mar15)
            .Should().Be(new DateOnly(2026, 3, 31));
    }

    [Fact]
    public void ParseDate_日のみ_5日後_はDayOnlyにマッチしない()
    {
        // 「5日後」は相対日付ではないが、DayOnlyRegexの除外対象
        // → 日付パース失敗でnullになる
        JapaneseDateParser.ParseDate("5日後に予約", Today).Should().BeNull();
    }

    [Fact]
    public void ParseDate_月日付き_DayOnlyより優先()
    {
        // 「3月5日」はShortDateRegexが優先。DayOnlyRegexは使われない
        JapaneseDateParser.ParseDate("3月5日に予約", Today)
            .Should().Be(new DateOnly(2026, 3, 5));
    }

    // ── 日付なし ──

    [Fact]
    public void ParseDate_日付なし_ReturnsNull()
    {
        JapaneseDateParser.ParseDate("予約を確認して", Today).Should().BeNull();
    }
}
