using Aloe.Common.AloeCoreLib.Util;
using Microsoft.Extensions.Configuration;

// ReSharper disable ArrangeStaticMemberQualifier


namespace Aloe.Medock.Reservation.AloeMedockResvApp.Configuration;

/// <summary>
/// コマンドライン引数、設定ファイル、シークレット、環境変数から値を読み込みます。
/// </summary>
public static class AloeClientConfig
{
    /// <summary>
    /// フラグ用のコマンドライン引数の一覧です。
    /// </summary>
    public static readonly List<string> FlagArgs =
    [
        "--standalone",
        "--sql",
        "--firstchance",
        "--resident",
    ];

    /// <summary>
    /// 短い名前のコマンドライン引数の一覧です。
    /// </summary>
    public static readonly List<string> ShortArgs =
    [
        "-u",
        "-p",
    ];

    /// <summary>
    /// コマンドライン引数を IConfiguration の設定とマッピングするためのエイリアスです。
    /// </summary>
    public static readonly Dictionary<string, string> Aliases = new()
    {
        { "--standalone", "AloeClientArgs:IsStandalone" },
        { "--sql", "AloeClientArgs:IsStandaloneSqlLogging" },
        { "--conn", "AloeClientArgs:ConnectionStringName" },
        { "--firstchance", "AloeClientArgs:IsFirstChanceExceptionLogging" },
        { "--resident", "AloeClientArgs:IsResident" },

        { "--screen", "AloeClientArgs:ScreenCode" },

        { "-u", "AloeClientArgs:User" },
        { "--usr", "AloeClientArgs:User" },
        { "--user", "AloeClientArgs:User" },

        { "-p", "AloeClientArgs:Password" },
        { "--pwd", "AloeClientArgs:Password" },
        { "--pass", "AloeClientArgs:Password" },
        { "--password", "AloeClientArgs:Password" },

        { "--pt", "AloeClientArgs:KarteNumber" },
        { "--patient", "AloeClientArgs:KarteNumber" },
        { "--karte", "AloeClientArgs:KarteNumber" },
    };

    /// <summary>
    /// 設定ファイルとして読み込むJSONファイルの一覧です。
    /// </summary>
    public static readonly List<string> Jsons =
    [
        "appsettings.json",
        UserOptions.FileName,
    ];

    /// <summary>
    /// 設定ファイルを読み込みます。
    /// コマンドライン引数は前処理され、特定のプロパティにマッピングされます。
    /// </summary>
    public static IConfigurationRoot CreateConfigurationRoot(string[] args)
    {
        var processedArgs = ArgsHelper.PreprocessArgs(
            args,
            FlagArgs,
            ShortArgs);

        // 読み込む順番に留意
        var builder = new ConfigurationBuilder()
            // サービスとして登録する場合でも読めるように exe を基準とする
            .SetBasePath(AppContext.BaseDirectory)
            // 設定ファイル
            .AddJsonFiles(Jsons)
            // シークレット(開発環境用)
            .AddUserSecrets<App>(optional: true)
            // 環境変数
            .AddEnvironmentVariables()
            // コマンドライン引数
            .AddCommandLine(processedArgs, Aliases);

        var config = builder.Build();

        return config;
    }
}
