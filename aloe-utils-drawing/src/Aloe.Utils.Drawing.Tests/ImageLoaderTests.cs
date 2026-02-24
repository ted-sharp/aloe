// <copyright file="ImageLoaderTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Drawing;

namespace Aloe.Utils.Drawing.Tests;

/// <summary>
/// ImageLoaderクラスのテスト
/// </summary>
public class ImageLoaderTests : IDisposable
{
    private readonly string _testImagePath;
    private readonly byte[] _validImageData = new byte[]
    {
        // 1x1 pixel PNG image
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
        0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
        0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, 0x54, 0x78, 0x9C, 0x63, 0xF8, 0x0F, 0x00, 0x00,
        0x01, 0x00, 0x01, 0x14, 0x6D, 0xD3, 0x8D, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
        0x42, 0x60, 0x82
    };

    public ImageLoaderTests()
    {
        _testImagePath = Path.GetTempFileName() + ".png";
        File.WriteAllBytes(_testImagePath, _validImageData);
    }

    public void Dispose()
    {
        if (File.Exists(_testImagePath))
        {
            File.Delete(_testImagePath);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Load_ValidImageFile_ReturnsImage()
    {
        // Act
        using var image = ImageLoader.Load(_testImagePath);

        // Assert
        Assert.NotNull(image);
        Assert.Equal(1, image.Width);
        Assert.Equal(1, image.Height);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Load_NullPath_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => ImageLoader.Load(null!));
        Assert.Equal("filePath", exception.ParamName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Load_EmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ImageLoader.Load(string.Empty));
        Assert.Contains("The value cannot be an empty string", exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Load_WhitespacePath_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ImageLoader.Load("   "));
        Assert.Contains("The value cannot be an empty string", exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Load_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "non_existent_file.png");

        // Act & Assert
        var exception = Assert.Throws<FileNotFoundException>(() => ImageLoader.Load(nonExistentPath));
        Assert.Contains("Image file not found", exception.Message);
        Assert.Contains(nonExistentPath, exception.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Load_InvalidImageFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidImagePath = Path.GetTempFileName() + ".png";
        File.WriteAllText(invalidImagePath, "This is not an image file");

        try
        {
            // Act & Assert - GDI+ throws ArgumentException for invalid image data
            var exception = Assert.Throws<InvalidOperationException>(() => ImageLoader.Load(invalidImagePath));
            Assert.Contains("Failed to load image from", exception.Message);
            Assert.Contains(invalidImagePath, exception.Message);
            Assert.IsType<ArgumentException>(exception.InnerException);
        }
        finally
        {
            File.Delete(invalidImagePath);
        }
    }
}