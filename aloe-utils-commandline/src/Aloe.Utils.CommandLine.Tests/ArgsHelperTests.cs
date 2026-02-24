namespace Aloe.Utils.CommandLine.Tests;

public class ArgsHelperTests
{
    [Fact]
    public void PreprocessArgs_WithFlagArgs_ShouldAddTrueValue()
    {
        // Arrange
        string[] args = ["--IsFlag"];
        string[] flagArgs = ["--IsFlag"];
        string[] shortArgs = [];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["--IsFlag", "true"], result);
    }

    [Fact]
    public void PreprocessArgs_FlagFollowedByExplicitValue_ShouldNotAppendTrue()
    {
        // Arrange
        string[] args = ["--IsFlag", "false"]; // 明示的な値が続く
        string[] flagArgs = ["--IsFlag"];
        string[] shortArgs = [];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["--IsFlag", "false"], result);
    }

    [Fact]
    public void PreprocessArgs_EqualsSeparator_ShouldPassThrough()
    {
        // Arrange
        string[] args = ["--IsFlag=false"]; // '=' はそのまま素通し
        string[] flagArgs = ["--IsFlag"];
        string[] shortArgs = [];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["--IsFlag=false"], result);
    }

    [Fact]
    public void PreprocessArgs_ShortOptionLongestMatch_ShouldPreferLonger()
    {
        // Arrange
        string[] args = ["-abvalue"]; // "-ab" と "-a" がある場合は "-ab" を優先
        string[] flagArgs = [];
        string[] shortArgs = ["-a", "-ab"];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["-ab", "value"], result);
    }

    [Fact]
    public void PreprocessArgs_WithShortArgs_ShouldSplitCombinedValue()
    {
        // Arrange
        string[] args = ["-uadmin", "-ppassword"];
        string[] flagArgs = [];
        string[] shortArgs = ["-u", "-p"];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["-u", "admin", "-p", "password"], result);
    }

    [Fact]
    public void PreprocessArgs_WithMixedArgs_ShouldProcessCorrectly()
    {
        // Arrange
        string[] args = ["--IsFlag", "-uadmin", "--OtherFlag", "value"];
        string[] flagArgs = ["--IsFlag", "--OtherFlag"];
        string[] shortArgs = ["-u"];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["--IsFlag", "true", "-u", "admin", "--OtherFlag", "value"], result);
    }

    [Fact]
    public void PreprocessArgs_WithNullArgs_ShouldThrowArgumentNullException()
    {
        // Arrange
        string[] args = null!;
        string[] flagArgs = [];
        string[] shortArgs = [];

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs));
    }

    [Fact]
    public void PreprocessArgs_WithNullFlagArgs_ShouldThrowArgumentNullException()
    {
        // Arrange
        string[] args = [];
        string[] flagArgs = null!;
        string[] shortArgs = [];

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs));
    }

    [Fact]
    public void PreprocessArgs_WithNullShortArgs_ShouldThrowArgumentNullException()
    {
        // Arrange
        string[] args = [];
        string[] flagArgs = [];
        string[] shortArgs = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs));
    }

    [Fact]
    public void PreprocessArgs_WithEmptyArgs_ShouldReturnEmpty()
    {
        // Arrange
        string[] args = [];
        string[] flagArgs = [];
        string[] shortArgs = [];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void PreprocessArgs_WithEmptyStringArg_ShouldPreserve()
    {
        // Arrange
        string[] args = [""];
        string[] flagArgs = [];
        string[] shortArgs = [];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal([""], result);
    }

    [Fact]
    public void PreprocessArgs_WithDuplicateFlags_ShouldProcessBoth()
    {
        // Arrange
        string[] args = ["--debug", "--debug"];
        string[] flagArgs = ["--debug"];
        string[] shortArgs = [];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["--debug", "true", "--debug", "true"], result);
    }

    [Fact]
    public void PreprocessArgs_FlagVsShortOption_ShouldPrioritizeFlag()
    {
        // Arrange
        string[] args = ["-u"];
        string[] flagArgs = ["-u"]; // 同じ文字列がフラグとショートオプション両方にある場合
        string[] shortArgs = ["-u"];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["-u", "true"], result); // フラグが優先される
    }

    [Fact]
    public void PreprocessArgs_WithSingleHyphen_ShouldPreserve()
    {
        // Arrange
        string[] args = ["-"];
        string[] flagArgs = [];
        string[] shortArgs = [];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["-"], result);
    }

    [Fact]
    public void PreprocessArgs_WithDoubleHyphen_ShouldPreserve()
    {
        // Arrange
        string[] args = ["--"];
        string[] flagArgs = [];
        string[] shortArgs = [];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        Assert.Equal(["--"], result);
    }

    [Fact]
    public void PreprocessArgs_WithDoubleHyphenAndOtherArgs_ShouldPreserveAll()
    {
        // Arrange
        string[] args = ["--IsFlag", "--", "-uvalue"];
        string[] flagArgs = ["--IsFlag"];
        string[] shortArgs = ["-u"];

        // Act
        var result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);

        // Assert
        // "--" の前は処理、"--" 以降はそのまま保持
        Assert.Equal(["--IsFlag", "true", "--", "-uvalue"], result);
    }

    [Fact]
    public void PreprocessArgs_WithNullElementInArgs_ShouldThrowArgumentException()
    {
        // Arrange
        string[] args = ["--IsFlag", null!, "value"];
        string[] flagArgs = ["--IsFlag"];
        string[] shortArgs = [];

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs));
        Assert.Contains("null", exception.Message);
    }
}
