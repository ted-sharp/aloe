using Aloe.Common.AloeCoreLib.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Tests.AloeCoreLibTest.Util;

public class DateHelperTests
{
    public static IEnumerable<object[]> ValidDateStringTestData =>
    [
        ["20240401", 2024, 4, 1],
        ["2024/04/01", 2024, 4, 1],
        ["2024-04-01", 2024, 4, 1],
        ["2024.04.01", 2024, 4, 1],
        ["2024年04月01日", 2024, 4, 1],
        ["2024/4/1", 2024, 4, 1],
        ["2024-4-1", 2024, 4, 1],
        ["2024.4.1", 2024, 4, 1],
        ["2024年4月1日", 2024, 4, 1],
        ["0001/01/01", 1, 1, 1],
        ["9999/12/31", 9999, 12, 31],

        ["R1/05/01", 2019, 5, 1],
        ["R01/05/01", 2019, 5, 1],
        ["H1/01/08", 1989, 1, 8],
        ["H31/04/30", 2019, 4, 30],
        ["S1/12/25", 1926, 12, 25],
        ["S64/01/07", 1989, 1, 7],
        ["T1/07/30", 1912, 7, 30],
        ["T15/12/24", 1926, 12, 24],
        ["M1/09/08", 1868, 9, 8],
        ["M45/07/29", 1912, 7, 29],

        ["令和元年5月1日", 2019, 5, 1],
        ["令和6年4月1日", 2024, 4, 1],
        ["平成元年1月8日", 1989, 1, 8],
        ["平成31年4月30日", 2019, 4, 30],
        ["昭和元年12月25日", 1926, 12, 25],
        ["昭和64年1月7日", 1989, 1, 7],
        ["大正元年7月30日", 1912, 7, 30],
        ["大正15年12月24日", 1926, 12, 24],
        ["明治元年9月8日", 1868, 9, 8],
        ["明治45年7月29日", 1912, 7, 29],

        ["M45-07-29", 1912, 7, 29],
        ["M45.07.29", 1912, 7, 29],
        ["Ｍ４５／７／２９", 1912, 7, 29],
        ["Ｍ４５．７．２９", 1912, 7, 29],
        ["Ｍ４５－７－２９", 1912, 7, 29],
        ["明治４５年７月２９日", 1912, 7, 29],
        ["明治〇一年〇九月〇八日", 1868, 9, 8],
        ["明治四五年七月二九日", 1912, 7, 29],
        ["明治四十五年七月二十九日", 1912, 7, 29],
        ["明治十年一月一日", 1877, 1, 1],
        ["明治拾一年二月二日", 1878, 2, 2],
        ["明治弐拾年三月三日", 1887, 3, 3],
        ["明治廿一年四月四日", 1888, 4, 4],
        ["明治参拾年五月五日", 1897, 5, 5],
        ["明治丗一年六月六日", 1898, 6, 6],
        ["明治卅一年七月七日", 1898, 7, 7],
        ["明治卌一年八月八日", 1908, 8, 8],

        ["2024/04", 2024, 4, 1],
        ["2024-04", 2024, 4, 1],
        ["2024.04", 2024, 4, 1],
        ["令和6年4月", 2024, 4, 1],
    ];

    [Theory]
    [MemberData(nameof(DateHelperTests.ValidDateStringTestData))]
    public void 有効な日付文字列のとき_パースした場合_成功する(string input, int year, int month, int day)
    {
        // Act
        var result = DateHelper.TryParseEx(input, out var actualDate);

        // Assert
        Assert.True(result);

        var expectedDate = new DateOnly(year, month, day);
        Assert.Equal(expectedDate, actualDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("　")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("any")]
    [InlineData("13")]
    [InlineData("13月")]
    [InlineData("2023/02/29")] // 存在しない日付
    [InlineData("2024/13/01")] // 存在しない日付
    [InlineData("令和6年4月31日")] // 存在しない日付
    [InlineData("慶応4年1月24日")] // 明治以前の元号
    public void 無効な日付文字列のとき_パースした場合_失敗する(string? input)
    {
        // Act
        var result = DateHelper.TryParseEx(input!, out var actualDate);

        // Assert
        Assert.False(result);
        Assert.Equal(DateOnly.MinValue, actualDate);
    }

    public static IEnumerable<object[]> MonthNameTestData =>
    [
        // 1月
        ["睦月", 1], ["むつき", 1], ["ムツキ", 1],
        ["jan", 1], ["january", 1],
        ["1", 1], ["01", 1], ["一月", 1], ["1月", 1],

        // 2月
        ["如月", 2], ["きさらぎ", 2], ["キサラギ", 2],
        ["feb", 2], ["february", 2],
        ["2", 2], ["02", 2], ["二月", 2], ["2月", 2],

        // 3月
        ["弥生", 3], ["やよい", 3], ["ヤヨイ", 3],
        ["mar", 3], ["march", 3],
        ["3", 3], ["03", 3], ["三月", 3], ["3月", 3],

        // 4月
        ["卯月", 4], ["うづき", 4], ["ウヅキ", 4],
        ["apr", 4], ["april", 4],
        ["4", 4], ["04", 4], ["四月", 4], ["4月", 4],

        // 5月
        ["皐月", 5], ["さつき", 5], ["サツキ", 5],
        ["may", 5],
        ["5", 5], ["05", 5], ["五月", 5], ["5月", 5],

        // 6月
        ["水無月", 6], ["みなづき", 6], ["ミナヅキ", 6],
        ["jun", 6], ["june", 6],
        ["6", 6], ["06", 6], ["六月", 6], ["6月", 6],

        // 7月
        ["文月", 7], ["ふみづき", 7], ["フミヅキ", 7],
        ["jul", 7], ["july", 7],
        ["7", 7], ["07", 7], ["七月", 7], ["7月", 7],

        // 8月
        ["葉月", 8], ["はづき", 8], ["ハヅキ", 8],
        ["aug", 8], ["august", 8],
        ["8", 8], ["08", 8], ["八月", 8], ["8月", 8],

        // 9月
        ["長月", 9], ["ながつき", 9], ["ナガツキ", 9],
        ["sep", 9], ["sept", 9], ["september", 9],
        ["9", 9], ["09", 9], ["九月", 9], ["9月", 9],

        // 10月
        ["神無月", 10], ["かんなづき", 10], ["カンナヅキ", 10],
        ["oct", 10], ["octo", 10], ["october", 10],
        ["10", 10], ["十月", 10], ["10月", 10],

        // 11月
        ["霜月", 11], ["しもつき", 11], ["シモツキ", 11],
        ["nov", 11], ["novem", 11], ["november", 11],
        ["11", 11], ["十一月", 11], ["11月", 11],

        // 12月
        ["師走", 12], ["しわす", 12], ["シワス", 12],
        ["dec", 12], ["decem", 12], ["december", 12],
        ["12", 12], ["十二月", 12], ["12月", 12],
    ];

    [Theory]
    [MemberData(nameof(DateHelperTests.MonthNameTestData))]
    public void 月だけ指定したとき_パースした場合_年度でパースできる(string input, int month)
    {
        // Act
        var result = DateHelper.TryParseEx(input, out var actualDate);

        // Assert
        Assert.True(result);

        var year = DateHelperTests.ToFiscalYear(month);
        var expectedDate = new DateOnly(year, month, 1);
        Assert.Equal(expectedDate, actualDate);
    }

    /// <summary>
    /// fiscal month 対応のため、期待される年度を推測する（現在月と現在年から）
    /// </summary>
    private static int ToFiscalYear(int month)
    {
        var now = DateTime.Today;
        var currentMonth = now.Month;
        var year = now.Year;

        if (4 <= currentMonth && month <= 3)
        {
            year++;
        }

        if (currentMonth <= 3 && month <= 3 && month < currentMonth)
        {
            year++;
        }

        return year;
    }

}
