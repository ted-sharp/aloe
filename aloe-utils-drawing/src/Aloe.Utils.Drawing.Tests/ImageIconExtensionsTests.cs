// <copyright file="ImageIconExtensionsTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Drawing;

namespace Aloe.Utils.Drawing.Tests;

/// <summary>
/// ImageIconExtensionsクラスのテスト
/// </summary>
public class ImageIconExtensionsTests : IDisposable
{
    private readonly Bitmap _testBitmap;

    public ImageIconExtensionsTests()
    {
        // Create a simple 16x16 test bitmap
        _testBitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(_testBitmap);
        graphics.Clear(Color.Red);
    }

    public void Dispose()
    {
        _testBitmap?.Dispose();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToIcon_ValidBitmap_ReturnsIcon()
    {
        // Act
        using var icon = _testBitmap.ToIcon();

        // Assert
        Assert.NotNull(icon);
        Assert.Equal(16, icon.Width);
        Assert.Equal(16, icon.Height);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToIcon_NullImage_ThrowsArgumentNullException()
    {
        // Arrange
        Image? nullImage = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => nullImage!.ToIcon());
        Assert.Equal("image", exception.ParamName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToIcon_DisposedImage_ThrowsInvalidOperationException()
    {
        // Arrange
        var disposedBitmap = new Bitmap(16, 16);
        disposedBitmap.Dispose();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => disposedBitmap.ToIcon());
        Assert.Contains("Failed to convert image to icon", exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToIcon_MultipleCallsOnSameImage_ReturnsDistinctIcons()
    {
        // Act
        using var icon1 = _testBitmap.ToIcon();
        using var icon2 = _testBitmap.ToIcon();

        // Assert
        Assert.NotNull(icon1);
        Assert.NotNull(icon2);
        Assert.NotSame(icon1, icon2); // Different instances
        Assert.Equal(icon1.Width, icon2.Width);
        Assert.Equal(icon1.Height, icon2.Height);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ToIcon_DifferentImageFormats_ReturnsValidIcons()
    {
        // Arrange
        var testCases = new[]
        {
            new { Width = 16, Height = 16 },
            new { Width = 32, Height = 32 },
            new { Width = 48, Height = 48 }
        };

        foreach (var testCase in testCases)
        {
            using var bitmap = new Bitmap(testCase.Width, testCase.Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Blue);

            // Act
            using var icon = bitmap.ToIcon();

            // Assert
            Assert.NotNull(icon);
            Assert.Equal(testCase.Width, icon.Width);
            Assert.Equal(testCase.Height, icon.Height);
        }
    }
}