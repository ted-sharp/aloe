namespace Aloe.Utils.Text.Tests;

using Xunit;
using Aloe.Utils.Text;

/// <summary>
/// ValueStringBuilder クラスのテスト
/// </summary>
public class ValueStringBuilderTest
{
    [Fact(DisplayName = "文字の追加が正常に動作する")]
    public void Append_Char_WorksCorrectly()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        sb.Append('a');
        sb.Append('b');
        sb.Append('c');
        Assert.Equal("abc", sb.ToString());
    }

    [Fact(DisplayName = "文字列の追加が正常に動作する")]
    public void Append_String_WorksCorrectly()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        sb.Append("Hello");
        sb.Append(" ");
        sb.Append("World");
        Assert.Equal("Hello World", sb.ToString());
    }

    [Fact(DisplayName = "Spanからの文字列追加が正常に動作する")]
    public void Append_Span_WorksCorrectly()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        ReadOnlySpan<char> span = "Test".AsSpan();
        sb.Append(span);
        Assert.Equal("Test", sb.ToString());
    }

    [Fact(DisplayName = "クリアが正常に動作する")]
    public void Clear_WorksCorrectly()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        sb.Append("Test");
        sb.Clear();
        Assert.Equal("", sb.ToString());
    }

    [Fact(DisplayName = "長さが正しく取得できる")]
    public void Length_ReturnsCorrectValue()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        sb.Append("Test");
        Assert.Equal(4, sb.Length);
    }

    [Fact(DisplayName = "容量が正しく取得できる")]
    public void Capacity_ReturnsCorrectValue()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        Assert.Equal(256, sb.Capacity);
    }

    [Fact(DisplayName = "インデクサーが正しく動作する")]
    public void Indexer_WorksCorrectly()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        sb.Append("Test");
        Assert.Equal('T', sb[0]);
        Assert.Equal('e', sb[1]);
        Assert.Equal('s', sb[2]);
        Assert.Equal('t', sb[3]);
    }

    [Fact(DisplayName = "範囲外アクセスで例外が発生する")]
    public void Indexer_ThrowsOnOutOfRange()
    {
        using var sb = new ValueStringBuilder(stackalloc char[256]);
        sb.Append("Test");
        try
        {
            var _ = sb[4];
            Assert.Fail("ArgumentOutOfRangeExceptionが発生するはず");
        }
        catch (ArgumentOutOfRangeException)
        {
            // 期待通りの例外
        }
    }

    [Fact(DisplayName = "Dispose後の操作で例外が発生する")]
    public void Dispose_ThrowsOnSubsequentOperations()
    {
        var sb = new ValueStringBuilder(stackalloc char[256]);
        sb.Append("Test");
        sb.Dispose();

        try
        {
            sb.Append("Test");
            Assert.Fail("ObjectDisposedExceptionが発生するはず");
        }
        catch (ObjectDisposedException)
        {
            // 期待通りの例外
        }

        try
        {
            sb.ToString();
            Assert.Fail("ObjectDisposedExceptionが発生するはず");
        }
        catch (ObjectDisposedException)
        {
            // 期待通りの例外
        }

        try
        {
            sb.Clear();
            Assert.Fail("ObjectDisposedExceptionが発生するはず");
        }
        catch (ObjectDisposedException)
        {
            // 期待通りの例外
        }
    }

    [Fact(DisplayName = "バッファが正しく拡張される")]
    public void Grow_WorksCorrectly()
    {
        using var sb = new ValueStringBuilder(stackalloc char[4]);
        sb.Append("Test");
        sb.Append("Longer");
        Assert.Equal("TestLonger", sb.ToString());
    }
}
