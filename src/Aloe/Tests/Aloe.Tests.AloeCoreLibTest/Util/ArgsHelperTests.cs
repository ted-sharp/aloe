using Aloe.Common.AloeCoreLib.Util;
using System;
using Xunit;

namespace Aloe.Tests.AloeCoreLibTest.Util;

public class ArgsHelperTests
{
    [Fact]
    public void 引数がnullのとき_メソッドを実行すると_ArgumentNullExceptionをスローする()
    {
        string[]? args = null;
        string[] flagArgs = ["--IsFlag"];
        string[] shortArgs = ["-u", "-p"];

        Assert.Throws<ArgumentNullException>(() => ArgsHelper.PreprocessArgs(args!, flagArgs, shortArgs));
        Assert.Throws<ArgumentNullException>(() => ArgsHelper.PreprocessArgs([], null!, shortArgs));
        Assert.Throws<ArgumentNullException>(() => ArgsHelper.PreprocessArgs([], flagArgs, null!));
    }

    [Fact]
    public void フラグの後に値がないとき_メソッドを実行すると_trueが補完される()
    {
        string[] args = ["--IsFlag", "-x"];
        string[] flagArgs = ["--IsFlag"];
        string[] shortArgs = ["-x"];

        string[] result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);
        string[] expected = ["--IsFlag", "true", "-x"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void フラグの後に値があるとき_メソッドを実行すると_値はそのまま維持される()
    {
        string[] args = ["--IsFlag", "false"];
        string[] flagArgs = ["--IsFlag"];

        string[] result = ArgsHelper.PreprocessArgs(args, flagArgs, []);
        string[] expected = ["--IsFlag", "false"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ショートオプションと値が連結しているとき_メソッドを実行すると_オプションと値に分割される()
    {
        string[] args = ["-uadmin"];
        string[] shortArgs = ["-u"];

        string[] result = ArgsHelper.PreprocessArgs(args, [], shortArgs);
        string[] expected = ["-u", "admin"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ショートオプションだけのとき_メソッドを実行すると_そのまま追加される()
    {
        string[] args = ["-p"];
        string[] shortArgs = ["-p"];

        string[] result = ArgsHelper.PreprocessArgs(args, [], shortArgs);
        string[] expected = ["-p"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void 未定義のショートオプションが連結されているとき_メソッドを実行すると_そのまま追加される()
    {
        string[] args = ["-zvalue"];
        string[] shortArgs = ["-x", "-y"];

        string[] result = ArgsHelper.PreprocessArgs(args, [], shortArgs);
        string[] expected = ["-zvalue"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void 通常の引数だけのとき_メソッドを実行すると_そのまま追加される()
    {
        string[] args = ["input.txt"];

        string[] result = ArgsHelper.PreprocessArgs(args, [], []);
        string[] expected = ["input.txt"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void 複数のパターンが混在しているとき_メソッドを実行すると_すべて正しく処理される()
    {
        string[] args = ["--IsFlag", "-uadmin", "-p", "1234", "input.txt"];
        string[] flagArgs = ["--IsFlag"];
        string[] shortArgs = ["-u", "-p"];

        string[] result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);
        string[] expected = ["--IsFlag", "true", "-u", "admin", "-p", "1234", "input.txt"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ショートオプションの後に別のショートオプションが続くとき_メソッドを実行すると_値と認識されない()
    {
        string[] args = ["-u", "-p"];
        string[] shortArgs = ["-u", "-p"];

        string[] result = ArgsHelper.PreprocessArgs(args, [], shortArgs);
        string[] expected = ["-u", "-p"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void フラグが配列の最後にあるとき_メソッドを実行すると_trueが補完される()
    {
        string[] args = ["--IsFlag"];
        string[] flagArgs = ["--IsFlag"];

        string[] result = ArgsHelper.PreprocessArgs(args, flagArgs, []);
        string[] expected = ["--IsFlag", "true"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void 定義されていないショートオプションのプレフィクスが一致するとき_メソッドを実行すると_そのまま追加される()
    {
        string[] args = ["-foobar"];
        string[] shortArgs = ["-f"];

        string[] result = ArgsHelper.PreprocessArgs(args, [], shortArgs);
        string[] expected = ["-f", "oobar"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void フラグの後にfalseが明示されているとき_メソッドを実行すると_falseがそのまま使われる()
    {
        string[] args = ["--standalone", "false"];
        string[] flagArgs = ["--standalone"];

        string[] result = ArgsHelper.PreprocessArgs(args, flagArgs, []);
        string[] expected = ["--standalone", "false"];

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ショートオプションの直後にフラグが続くとき_それぞれ別のオプションとして扱われる()
    {
        string[] args = ["-uadmin", "--firstchance"];
        string[] flagArgs = ["--firstchance"];
        string[] shortArgs = ["-u"];

        string[] result = ArgsHelper.PreprocessArgs(args, flagArgs, shortArgs);
        string[] expected = ["-u", "admin", "--firstchance", "true"];

        Assert.Equal(expected, result);
    }
}
