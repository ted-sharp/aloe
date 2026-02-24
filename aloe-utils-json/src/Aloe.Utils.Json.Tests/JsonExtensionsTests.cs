// <copyright file="JsonExtensionsTests.cs" company="Aloe">
// Copyright (c) Aloe. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aloe.Utils.Json.Tests;

/// <summary>
/// JsonExtensionsのテストクラスですわ。
/// </summary>
public partial class JsonExtensionsTests
{
    /// <summary>
    /// テスト用のサンプルクラスですわ。
    /// </summary>
    private class SampleClass
    {
        public string Name { get; set; } = String.Empty;
        public int Age { get; set; }
    }

    /// <summary>
    /// AOT/トリミング対応用のJsonSerializerContextですわ。
    /// </summary>
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(SampleClass))]
    [JsonSerializable(typeof(JsonElement))]
    private partial class TestJsonContext : JsonSerializerContext
    {
    }

    [Fact(DisplayName = "ToJson_正常系_シリアライズできること")]
    public void ToJson_正常系_シリアライズできること()
    {
        // Arrange
        var sample = new SampleClass { Name = "Test", Age = 20 };
        var expected = """
            {
              "Name": "Test",
              "Age": 20
            }
            """;

        // Act
        var actual = sample.ToJson();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "ToObj_正常系_デシリアライズできること")]
    public void ToObj_正常系_デシリアライズできること()
    {
        // Arrange
        var json = """
            {
                "Name": "Test",
                "Age": 20
            }
            """;
        var expected = new SampleClass { Name = "Test", Age = 20 };

        // Act
        var actual = json.ToObj<SampleClass>();

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Age, actual.Age);
    }

    [Fact(DisplayName = "ToObj_異常系_不正なJSONの場合は例外が発生すること")]
    public void ToObj_異常系_不正なJSONの場合は例外が発生すること()
    {
        // Arrange
        var invalidJson = "{invalid json}";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => invalidJson.ToObj<SampleClass>());
    }

    [Fact(DisplayName = "FormatJson_正常系_JSONが整形されること")]
    public void FormatJson_正常系_JSONが整形されること()
    {
        // Arrange
        var input = """
            {
                "Name": "Test",
                "Age": 20
            }
            """;
        var expected = """
            {
              "Name": "Test",
              "Age": 20
            }
            """;

        // Act
        var actual = input.FormatJson();

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "FormatJson_異常系_不正なJSONの場合は例外が発生すること")]
    public void FormatJson_異常系_不正なJSONの場合は例外が発生すること()
    {
        // Arrange
        var invalidJson = "{invalid json}";

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => invalidJson.FormatJson());
    }

    [Fact(DisplayName = "ToJson_JsonTypeInfo版_正常系_シリアライズできること")]
    public void ToJson_JsonTypeInfo版_正常系_シリアライズできること()
    {
        // Arrange
        var sample = new SampleClass { Name = "Test", Age = 20 };
        var expected = """
            {
              "Name": "Test",
              "Age": 20
            }
            """;

        // Act
        var actual = sample.ToJson(TestJsonContext.Default.SampleClass);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "ToObj_JsonTypeInfo版_正常系_デシリアライズできること")]
    public void ToObj_JsonTypeInfo版_正常系_デシリアライズできること()
    {
        // Arrange
        var json = """
            {
                "Name": "Test",
                "Age": 20
            }
            """;
        var expected = new SampleClass { Name = "Test", Age = 20 };

        // Act
        var actual = json.ToObj(TestJsonContext.Default.SampleClass);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Age, actual.Age);
    }

    [Fact(DisplayName = "FormatJson_JsonSerializerOptions版_正常系_カスタムオプションで整形できること")]
    public void FormatJson_JsonSerializerOptions版_正常系_カスタムオプションで整形できること()
    {
        // Arrange
        var input = """{"Name":"Test","Age":20}""";
        var expected = """{"Name":"Test","Age":20}""";
        var options = new JsonSerializerOptions { WriteIndented = false };

        // Act
        var actual = input.FormatJson(options);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "FormatJson_JsonSerializerOptions版_正常系_オプションがnullの場合はDefaultOptionsが使用されること")]
    public void FormatJson_JsonSerializerOptions版_正常系_オプションがnullの場合はDefaultOptionsが使用されること()
    {
        // Arrange
        var input = """{"Name":"Test","Age":20}""";
        var expected = """
            {
              "Name": "Test",
              "Age": 20
            }
            """;

        // Act
        var actual = input.FormatJson(options: null);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "TryToObj_正常系_デシリアライズできること")]
    public void TryToObj_正常系_デシリアライズできること()
    {
        // Arrange
        var json = """
            {
                "Name": "Test",
                "Age": 20
            }
            """;

        // Act
        var result = json.TryToObj<SampleClass>(out var actual);

        // Assert
        Assert.True(result);
        Assert.NotNull(actual);
        Assert.Equal("Test", actual.Name);
        Assert.Equal(20, actual.Age);
    }

    [Fact(DisplayName = "TryToObj_異常系_不正なJSONの場合はfalseを返すこと")]
    public void TryToObj_異常系_不正なJSONの場合はfalseを返すこと()
    {
        // Arrange
        var invalidJson = "{invalid json}";

        // Act
        var result = invalidJson.TryToObj<SampleClass>(out var actual);

        // Assert
        Assert.False(result);
        Assert.Null(actual);
    }

    [Fact(DisplayName = "TryToObj_異常系_空文字列の場合はfalseを返すこと")]
    public void TryToObj_異常系_空文字列の場合はfalseを返すこと()
    {
        // Arrange
        var emptyJson = string.Empty;

        // Act
        var result = emptyJson.TryToObj<SampleClass>(out var actual);

        // Assert
        Assert.False(result);
        Assert.Null(actual);
    }

    [Fact(DisplayName = "TryToObj_JsonTypeInfo版_正常系_デシリアライズできること")]
    public void TryToObj_JsonTypeInfo版_正常系_デシリアライズできること()
    {
        // Arrange
        var json = """
            {
                "Name": "Test",
                "Age": 20
            }
            """;

        // Act
        var result = json.TryToObj(TestJsonContext.Default.SampleClass, out var actual);

        // Assert
        Assert.True(result);
        Assert.NotNull(actual);
        Assert.Equal("Test", actual.Name);
        Assert.Equal(20, actual.Age);
    }

    [Fact(DisplayName = "TryToObj_JsonTypeInfo版_異常系_不正なJSONの場合はfalseを返すこと")]
    public void TryToObj_JsonTypeInfo版_異常系_不正なJSONの場合はfalseを返すこと()
    {
        // Arrange
        var invalidJson = "{invalid json}";

        // Act
        var result = invalidJson.TryToObj(TestJsonContext.Default.SampleClass, out var actual);

        // Assert
        Assert.False(result);
        Assert.Null(actual);
    }

    [Fact(DisplayName = "TryToObj_JsonTypeInfo版_異常系_jsonTypeInfoがnullの場合はfalseを返すこと")]
    public void TryToObj_JsonTypeInfo版_異常系_jsonTypeInfoがnullの場合はfalseを返すこと()
    {
        // Arrange
        var json = """
            {
                "Name": "Test",
                "Age": 20
            }
            """;

        // Act
        var result = json.TryToObj<SampleClass>(null!, out var actual);

        // Assert
        Assert.False(result);
        Assert.Null(actual);
    }

    [Fact(DisplayName = "ToJson_JsonSerializerOptions版_正常系_カスタムオプションでシリアライズできること")]
    public void ToJson_JsonSerializerOptions版_正常系_カスタムオプションでシリアライズできること()
    {
        // Arrange
        var sample = new SampleClass { Name = "Test", Age = 20 };
        var options = new JsonSerializerOptions { WriteIndented = false };
        var expected = """{"Name":"Test","Age":20}""";

        // Act
        var actual = sample.ToJson(options);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "ToJson_JsonSerializerOptions版_正常系_オプションがnullの場合はDefaultOptionsが使用されること")]
    public void ToJson_JsonSerializerOptions版_正常系_オプションがnullの場合はDefaultOptionsが使用されること()
    {
        // Arrange
        var sample = new SampleClass { Name = "Test", Age = 20 };
        var expected = """
            {
              "Name": "Test",
              "Age": 20
            }
            """;

        // Act
        var actual = sample.ToJson(options: null);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact(DisplayName = "ToObj_JsonSerializerOptions版_正常系_カスタムオプションでデシリアライズできること")]
    public void ToObj_JsonSerializerOptions版_正常系_カスタムオプションでデシリアライズできること()
    {
        // Arrange
        var json = """{"Name":"Test","Age":20}""";
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        // Act
        var actual = json.ToObj<SampleClass>(options);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("Test", actual.Name);
        Assert.Equal(20, actual.Age);
    }

    [Fact(DisplayName = "ToObj_JsonSerializerOptions版_正常系_オプションがnullの場合はDefaultOptionsが使用されること")]
    public void ToObj_JsonSerializerOptions版_正常系_オプションがnullの場合はDefaultOptionsが使用されること()
    {
        // Arrange
        var json = """
            {
                "Name": "Test",
                "Age": 20
            }
            """;

        // Act
        var actual = json.ToObj<SampleClass>(options: null);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal("Test", actual.Name);
        Assert.Equal(20, actual.Age);
    }
}
