namespace Aloe.Utils.Wafu.Date.Tests;

using Aloe.Utils.Wafu.Date;
using Xunit;

/// <summary>
/// StringExtensions のテスト。
/// </summary>
public class WafuDateStringExtensionTests
{
    [Fact]
    public void ToDateOrToday_ReturnsToday_WhenInvalid()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var date = "".ToDateOrToday();

        // Assert
        Assert.Equal(today, date);
    }

    [Fact]
    public void ToDateOr_ReturnsFallback_WhenInvalid()
    {
        // Arrange
        var fallback = new DateOnly(2024, 1, 2);

        // Act
        var date = "invalid".ToDateOr(fallback);

        // Assert
        Assert.Equal(fallback, date);
    }
}


