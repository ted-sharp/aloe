namespace Aloe.Utils.Wafu.Date.Tests;

using Xunit;

/// <summary>
/// DateHelperクラスのテスト
/// </summary>
public class WafuDateTests
{
    [Theory]
    [InlineData("2024年3月15日", 2024, 3, 15)]
    [InlineData("2024/03/15", 2024, 3, 15)]
    [InlineData("2024-03-15", 2024, 3, 15)]
    [InlineData("2024.03.15", 2024, 3, 15)]
    [InlineData("2024年3月", 2024, 3, 1)]
    [InlineData("2024/03", 2024, 3, 1)]
    [InlineData("2024-03", 2024, 3, 1)]
    [InlineData("2024.03", 2024, 3, 1)]
    [InlineData("20240315", 2024, 3, 15)]
    [InlineData("202403", 2024, 3, 1)]
    public void TryParseEx_ValidDateStrings_ReturnsTrue(string input, int expectedYear, int expectedMonth, int expectedDay)
    {
        // Arrange & Act
        var result = DateHelper.TryParseEx(input, out var date);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedYear, date.Year);
        Assert.Equal(expectedMonth, date.Month);
        Assert.Equal(expectedDay, date.Day);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("2024/13/01")]
    [InlineData("2024/00/01")]
    public void TryParseEx_InvalidDateStrings_ReturnsFalse(string input)
    {
        // Arrange & Act
        var result = DateHelper.TryParseEx(input, out var date);

        // Assert
        Assert.False(result);
        Assert.Equal(default, date);
    }

    [Fact]
    public void GetToday_ReturnsCurrentDate()
    {
        // Arrange & Act
        var today = DateHelper.GetToday();

        // Assert
        Assert.Equal(DateOnly.FromDateTime(DateTime.Today), today);
    }

    [Fact]
    public void GetFirstDate_ReturnsFirstDayOfMonth()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var firstDate = DateHelper.GetFirstDate();

        // Assert
        Assert.Equal(1, firstDate.Day);
        Assert.Equal(today.Year, firstDate.Year);
        Assert.Equal(today.Month, firstDate.Month);
    }

    [Theory]
    [InlineData(2024, 3)]
    [InlineData(2024, 12)]
    public void GetFirstDate_WithYearAndMonth_ReturnsFirstDayOfSpecifiedMonth(int year, int month)
    {
        // Arrange & Act
        var firstDate = DateHelper.GetFirstDate(year, month);

        // Assert
        Assert.Equal(1, firstDate.Day);
        Assert.Equal(year, firstDate.Year);
        Assert.Equal(month, firstDate.Month);
    }

    [Fact]
    public void GetEndDate_ReturnsLastDayOfMonth()
    {
        // Arrange
        var date = new DateOnly(2024, 3, 15);

        // Act
        var endDate = DateHelper.GetEndDate(date);

        // Assert
        Assert.Equal(31, endDate.Day);
        Assert.Equal(date.Year, endDate.Year);
        Assert.Equal(date.Month, endDate.Month);
    }
}
