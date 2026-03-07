using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.VoiceCommand;
using FluentAssertions;
using Moq;

namespace Aloe.Apps.MedockServer.Tests.VoiceCommand;

public class VoiceCommandParserTests
{
    private static readonly DateOnly Today = new(2026, 2, 28);
    private readonly VoiceCommandParser _parser;

    public VoiceCommandParserTests()
    {
        var mockDateTime = new Mock<IDateTimeProvider>();
        mockDateTime.Setup(x => x.TodayDateOnly).Returns(Today);
        _parser = new VoiceCommandParser(mockDateTime.Object);
    }

    // ── インテント検出 ──

    [Theory]
    [InlineData("明日の10時に田中さんの予約を入れて", VoiceCommandIntent.CreateAppointment)]
    [InlineData("3月5日に予約を追加して", VoiceCommandIntent.CreateAppointment)]
    [InlineData("予約を取ってください", VoiceCommandIntent.CreateAppointment)]
    public void Parse_予約作成系_ReturnsCreateAppointment(string transcript, VoiceCommandIntent expected)
    {
        _parser.Parse(transcript).Intent.Should().Be(expected);
    }

    [Theory]
    [InlineData("明日の予約をキャンセルして", VoiceCommandIntent.CancelAppointment)]
    [InlineData("田中さんの予約を取り消して", VoiceCommandIntent.CancelAppointment)]
    [InlineData("予約を削除して", VoiceCommandIntent.CancelAppointment)]
    public void Parse_キャンセル系_ReturnsCancelAppointment(string transcript, VoiceCommandIntent expected)
    {
        _parser.Parse(transcript).Intent.Should().Be(expected);
    }

    [Theory]
    [InlineData("明日の予約を確認して", VoiceCommandIntent.SearchAppointment)]
    [InlineData("来週月曜の予約を見せて", VoiceCommandIntent.SearchAppointment)]
    [InlineData("3月の空き状況を調べて", VoiceCommandIntent.SearchAppointment)]
    [InlineData("予約一覧を表示して", VoiceCommandIntent.SearchAppointment)]
    public void Parse_検索系_ReturnsSearchAppointment(string transcript, VoiceCommandIntent expected)
    {
        _parser.Parse(transcript).Intent.Should().Be(expected);
    }

    [Fact]
    public void Parse_不明テキスト_ReturnsUnknown()
    {
        _parser.Parse("こんにちは").Intent.Should().Be(VoiceCommandIntent.Unknown);
    }

    [Fact]
    public void Parse_空文字_ReturnsUnknown()
    {
        _parser.Parse("").Intent.Should().Be(VoiceCommandIntent.Unknown);
    }

    // ── 日付抽出 ──

    [Fact]
    public void Parse_明日の予約_Returns明日()
    {
        var result = _parser.Parse("明日の10時に予約を入れて");
        result.Date.Should().Be(Today.AddDays(1));
    }

    [Fact]
    public void Parse_絶対日付_ReturnsCorrectDate()
    {
        var result = _parser.Parse("3月5日の10時に予約");
        result.Date.Should().Be(new DateOnly(2026, 3, 5));
    }

    // ── 時刻抽出 ──

    [Fact]
    public void Parse_時刻指定_Returns分数()
    {
        var result = _parser.Parse("明日の10時に予約を入れて");
        result.StartMin.Should().Be(600);
    }

    [Fact]
    public void Parse_午後時刻_Returns分数()
    {
        var result = _parser.Parse("午後3時半に予約を入れて");
        result.StartMin.Should().Be(930);
    }

    // ── 患者名抽出 ──

    [Fact]
    public void Parse_さん付き患者名_ReturnsName()
    {
        var result = _parser.Parse("明日の10時に田中さんの予約を入れて");
        result.PatientName.Should().Be("田中");
    }

    [Fact]
    public void Parse_様付き患者名_ReturnsName()
    {
        var result = _parser.Parse("山田様の予約をキャンセル");
        result.PatientName.Should().Be("山田");
    }

    [Fact]
    public void Parse_の予約パターン_ReturnsName()
    {
        var result = _parser.Parse("鈴木の予約を確認して");
        result.PatientName.Should().Be("鈴木");
    }

    // ── 複合テスト ──

    [Fact]
    public void Parse_フルコマンド_ExtractsAllFields()
    {
        var result = _parser.Parse("明日の10時に田中さんの予約を入れて");

        result.Intent.Should().Be(VoiceCommandIntent.CreateAppointment);
        result.Date.Should().Be(Today.AddDays(1));
        result.StartMin.Should().Be(600);
        result.PatientName.Should().Be("田中");
        result.OriginalTranscript.Should().Be("明日の10時に田中さんの予約を入れて");
        result.Summary.Should().Contain("予約作成");
    }

    [Fact]
    public void Parse_Summaryが確認テキストを含む()
    {
        var result = _parser.Parse("3月5日の10時に田中さんの予約を入れて");

        result.Summary.Should().Contain("3月5日");
        result.Summary.Should().Contain("10:00");
        result.Summary.Should().Contain("田中さん");
        result.Summary.Should().Contain("予約作成");
    }

    [Fact]
    public void Parse_検索コマンド_日付のみ()
    {
        var result = _parser.Parse("来週の月曜日の予約を確認して");

        result.Intent.Should().Be(VoiceCommandIntent.SearchAppointment);
        result.Date.Should().NotBeNull();
        result.Date!.Value.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    // ── 漢数字対応 ──

    [Fact]
    public void Parse_漢数字の時刻_Returns分数()
    {
        var result = _parser.Parse("明日の十時に予約を入れて");
        result.StartMin.Should().Be(600);
    }

    [Fact]
    public void Parse_漢数字の日付と時刻_全て抽出()
    {
        var result = _parser.Parse("三月五日の十時半に田中さんの予約を入れて");
        result.Date.Should().Be(new DateOnly(2026, 3, 5));
        result.StartMin.Should().Be(630);
        result.PatientName.Should().Be("田中");
    }

    [Fact]
    public void Parse_句読点混じり_正しくパース()
    {
        var result = _parser.Parse("明日の、10時に、田中さんの予約を入れて。");
        result.Intent.Should().Be(VoiceCommandIntent.CreateAppointment);
        result.Date.Should().Be(Today.AddDays(1));
        result.StartMin.Should().Be(600);
        result.PatientName.Should().Be("田中");
    }

    [Fact]
    public void Parse_午後漢数字_Returns分数()
    {
        var result = _parser.Parse("午後三時に予約を入れて");
        result.StartMin.Should().Be(900);
    }

    // ── 口語表現 ──

    [Fact]
    public void Parse_おとつい_Returns一昨日()
    {
        var result = _parser.Parse("おとついの予約を確認して");
        result.Intent.Should().Be(VoiceCommandIntent.SearchAppointment);
        result.Date.Should().Be(Today.AddDays(-2));
    }

    [Fact]
    public void Parse_おととい_Returns一昨日()
    {
        var result = _parser.Parse("おとといの予約を確認して");
        result.Date.Should().Be(Today.AddDays(-2));
    }

    [Fact]
    public void Parse_きのう_Returns昨日()
    {
        var result = _parser.Parse("きのうの予約を確認して");
        result.Date.Should().Be(Today.AddDays(-1));
    }

    [Fact]
    public void Parse_しあさって_Returns3日後()
    {
        var result = _parser.Parse("しあさってに予約を入れて");
        result.Date.Should().Be(Today.AddDays(3));
    }

    [Fact]
    public void Parse_来月のついたち_Returns来月1日()
    {
        var result = _parser.Parse("来月のついたちに予約を入れて");
        result.Intent.Should().Be(VoiceCommandIntent.CreateAppointment);
        result.Date.Should().Be(new DateOnly(2026, 3, 1));
    }

    [Fact]
    public void Parse_来月のはつか_Returns来月20日()
    {
        var result = _parser.Parse("来月のはつかに予約を入れて");
        result.Date.Should().Be(new DateOnly(2026, 3, 20));
    }

    [Fact]
    public void Parse_午後いち_Returns13時()
    {
        var result = _parser.Parse("明日の午後いちに予約を入れて");
        result.StartMin.Should().Be(780); // 13:00
    }

    [Fact]
    public void Parse_朝いち_Returns9時()
    {
        var result = _parser.Parse("明日の朝いちに予約を入れて");
        result.StartMin.Should().Be(540); // 9:00
    }

    [Fact]
    public void Parse_お昼_Returns12時()
    {
        var result = _parser.Parse("明日のお昼に予約を入れて");
        result.StartMin.Should().Be(720); // 12:00
    }

    [Fact]
    public void Parse_きょうとあした_ひらがな()
    {
        var result = _parser.Parse("きょうの予約を確認して");
        result.Date.Should().Be(Today);

        var result2 = _parser.Parse("あしたの10時に予約を入れて");
        result2.Date.Should().Be(Today.AddDays(1));
    }

    // ── 時刻揺らぎ対応 ──

    [Theory]
    [InlineData("午後1予約", 780)]       // 午後1 → 午後1時 → 13:00
    [InlineData("午前9予約", 540)]       // 午前9 → 午前9時 → 9:00
    [InlineData("昨日午後1予約", 780)]   // 日付+午後1
    [InlineData("午後一予約", 780)]      // 午後一 → 漢数字変換 → 午後1 → 午後1時
    public void Parse_時省略パターン_Returns正しい時刻(string transcript, int expectedMin)
    {
        var result = _parser.Parse(transcript);
        result.StartMin.Should().Be(expectedMin);
    }

    [Fact]
    public void Parse_午後に_は助詞なので時刻なし()
    {
        // 「午後に予約を入れて」の「に」は助詞であり、「午後2時」ではない
        var result = _parser.Parse("午後に予約を入れて");
        result.StartMin.Should().BeNull();
    }

    [Fact]
    public void Parse_午後さん_Returns15時()
    {
        var result = _parser.Parse("午後さん予約を入れて");
        result.StartMin.Should().Be(900); // 15:00
    }

    [Fact]
    public void Parse_午前いち_Returns午前1時扱い()
    {
        // 午前いち → 午前1時 → 業務時間外なので午後推定（1〜7時ルール）
        // ただし午前が明示されているので1時のまま = 60分
        var result = _parser.Parse("午前いちに予約を入れて");
        result.StartMin.Should().Be(60); // 1:00
    }

    [Fact]
    public void Parse_午後よん_Returns16時()
    {
        var result = _parser.Parse("午後よんに予約を入れて");
        result.StartMin.Should().Be(960); // 16:00
    }

    [Fact]
    public void Parse_午後ご_Returns17時()
    {
        var result = _parser.Parse("午後ご予約を入れて");
        result.StartMin.Should().Be(1020); // 17:00
    }

    [Fact]
    public void Parse_午後し_Returns16時()
    {
        // 「し」→ 4
        var result = _parser.Parse("午後しに予約を入れて");
        result.StartMin.Should().Be(960); // 16:00
    }

    // ── 仮予約 ──

    [Theory]
    [InlineData("仮予約を入れて")]
    [InlineData("仮で来週月曜に入れて")]
    [InlineData("とりあえず明日の10時に入れて")]
    public void Parse_仮予約系_ReturnsTentativeAppointment(string transcript)
    {
        _parser.Parse(transcript).Intent.Should().Be(VoiceCommandIntent.TentativeAppointment);
    }

    // ── フィルター設定 ──

    [Fact]
    public void Parse_月曜だけ_SetFilter_月曜のみ()
    {
        var result = _parser.Parse("月曜だけ表示して");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
        result.FilterParams.Should().NotBeNull();
        result.FilterParams!.SelectedDays.Should().BeEquivalentTo([1]);
        result.FilterParams.ClearAll.Should().BeFalse();
    }

    [Fact]
    public void Parse_月火水_複数曜日フィルター()
    {
        var result = _parser.Parse("月曜と火曜と水曜だけ表示して");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
        result.FilterParams!.SelectedDays.Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void Parse_午前のみ_TimeSlotフィルター()
    {
        var result = _parser.Parse("午前のみ表示して");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
        result.FilterParams!.TimeSlots.Should().BeEquivalentTo(["09:00-12:00"]);
    }

    [Fact]
    public void Parse_午後だけ_TimeSlotフィルター()
    {
        var result = _parser.Parse("午後だけ表示して");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
        result.FilterParams!.TimeSlots.Should().BeEquivalentTo(["13:00-17:00"]);
    }

    [Fact]
    public void Parse_フィルタークリア_ClearAllTrue()
    {
        var result = _parser.Parse("フィルタークリア");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
        result.FilterParams!.ClearAll.Should().BeTrue();
    }

    [Fact]
    public void Parse_フィルターリセット_ClearAllTrue()
    {
        var result = _parser.Parse("フィルターリセットして");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
        result.FilterParams!.ClearAll.Should().BeTrue();
    }

    [Fact]
    public void Parse_全部表示_ClearAllTrue()
    {
        var result = _parser.Parse("全部表示して");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
        result.FilterParams!.ClearAll.Should().BeTrue();
    }

    [Fact]
    public void Parse_空き2件以上_RequiredCapacity()
    {
        var result = _parser.Parse("空きが2件以上の日を表示して");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
        result.FilterParams!.RequiredCapacity.Should().Be(2);
    }

    [Fact]
    public void Parse_絞り込み_SetFilter()
    {
        var result = _parser.Parse("絞り込みして");
        result.Intent.Should().Be(VoiceCommandIntent.SetFilter);
    }
}
