namespace Aloe.Utils.Wafu.Date.Tests;

using Xunit;

/// <summary>
/// 会計年度の月名や和風月名の解析に関するテスト。
/// </summary>
public class WafuDateFiscalMonthTests
{
    private static int ComputeExpectedFiscalYear(int targetMonth)
    {
        var currentMonth = DateTime.Today.Month;
        var fiscalYear = DateTime.Today.Year;

        if ((4 <= currentMonth && targetMonth <= 3) ||
            (currentMonth <= 3 && targetMonth <= 3 && targetMonth < currentMonth))
        {
            fiscalYear++;
        }

        return fiscalYear;
    }

    [Theory]
    [InlineData("4月", 4)]
    [InlineData("睦月", 1)]
    [InlineData("師走", 12)]
    [InlineData("Feb", 2)]
    [InlineData("january", 1)]
    [InlineData("１１月", 11)] // 全角も可
    public void TryParseEx_MonthNameOnly_ParsesToFiscalMonthFirstDay(string input, int expectedMonth)
    {
        // Act
        var ok = DateHelper.TryParseEx(input, out var date);

        // Assert
        Assert.True(ok);
        Assert.Equal(1, date.Day);
        Assert.Equal(expectedMonth, date.Month);
        Assert.Equal(ComputeExpectedFiscalYear(expectedMonth), date.Year);
    }
}


