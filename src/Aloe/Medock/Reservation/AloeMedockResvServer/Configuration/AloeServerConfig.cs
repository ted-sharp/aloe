using Aloe.Common.AloeCoreLib.Util;

// ReSharper disable ArrangeStaticMemberQualifier

namespace Aloe.Medock.Reservation.AloeMedockResvServer.Configuration;

/// <summary>
/// コマンドライン引数、設定ファイル、シークレット、環境変数から値を読み込みます。
/// </summary>
public static class AloeServerConfig
{
    /// <summary>
    /// フラグ用のコマンドライン引数の一覧です。
    /// </summary>
    public static readonly List<string> FlagArgs =
    [
        "--seed",
        "--sql",
        "--conn",
        "--firstchance",
    ];

    /// <summary>
    /// 短い名前のコマンドライン引数の一覧です。
    /// </summary>
    public static readonly List<string> ShortArgs = [];

    /// <summary>
    /// コマンドライン引数を IConfiguration の設定とマッピングするためのエイリアスです。
    /// </summary>
    public static readonly Dictionary<string, string> Aliases = new()
    {
        { "--seed", "AloeServerArgs:IsSeed" },
        { "--sql", "AloeServerArgs:IsSqlLogging" },
        { "--conn", "AloeServerArgs:ConnectionStringName" },
        { "--firstchance", "AloeServerArgs:IsFirstChanceExceptionLogging" },
    };

    /// <summary>
    /// 設定ファイルとして読み込むJSONファイルの一覧です。
    /// </summary>
    public static readonly List<string> Jsons =
    [
        "appsettings.json",
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
            .AddUserSecrets<AloeServerArgs>(optional: true)
            // 環境変数
            .AddEnvironmentVariables()
            // コマンドライン引数
            .AddCommandLine(processedArgs, Aliases);

        var config = builder.Build();

        return config;
    }
}
