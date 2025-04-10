
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Microsoft.Extensions.Configuration;

namespace Aloe.Medock.Reservation.AloeMedockResvServer.Settings;

/// <summary>
/// コマンドライン引数、設定ファイル、シークレット、環境変数から値を読み込みます。
/// </summary>
public class AloeServerSettings
{
    #region static

    /// <summary>
    /// フラグ用のコマンドライン引数の一覧です。
    /// </summary>
    public static readonly List<string> FlagArgs = [
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
        { "--seed", "AloeServerSettings:IsSeed" },
        { "--sql", "AloeServerSettings:IsSqlLogging" },
        { "--conn", "AloeServerSettings:ConnectionStringName" },
        { "--firstchance", "AloeServerSettings:IsFirstChanceExceptionLogging" },
    };

    public static IConfigurationRoot CreateConfiguration(string[] args, string fileName = "appsettings.json")
    {
        var processedArgs = ArgsHelper.PreprocessArgs(
            args,
            AloeServerSettings.FlagArgs,
            AloeServerSettings.ShortArgs);

        var config = new ConfigurationBuilder()
            // サービスとして登録する場合でも読めるように exe を基準とする
            .SetBasePath(AppContext.BaseDirectory)
            // 設定ファイル
            .AddJsonFile(fileName, optional: true, reloadOnChange: true)
            // シークレット(開発環境用)
            .AddUserSecrets<AloeServerSettings>(optional: true)
            // 環境変数
            .AddEnvironmentVariables()
            // コマンドライン引数
            .AddCommandLine(processedArgs, AloeServerSettings.Aliases)
            .Build();

        return config;
    }

    #endregion static

    /// <summary>
    /// サンプルデータの挿入を試行します。
    /// 空の場合のみ挿入できます。
    /// </summary>
    public bool IsSeed { get; set; }

    /// <summary>
    /// SQLのログを出力できるようにします。
    /// </summary>
    public bool IsSqlLogging { get; set; }

    /// <summary>
    /// 使用する接続文字列のキー名を指定します。
    /// </summary>
    public string ConnectionStringName { get; set; } = "DefaultConnection";

    /// <summary>
    /// 例外が発生したときにTraceレベルでログ出力します。
    /// 内部で握りつぶしている場合や、例外を制御に使っている場合でも捕捉します。
    /// </summary>
    public bool IsFirstChanceExceptionLogging { get; set; }
}
