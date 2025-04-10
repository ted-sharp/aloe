
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Microsoft.Extensions.Configuration;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Settings;

/// <summary>
/// コマンドライン引数、設定ファイル、シークレット、環境変数から値を読み込みます。
/// </summary>
public class AloeClientSettings
{
    #region static

    /// <summary>
    /// フラグ用のコマンドライン引数の一覧です。
    /// </summary>
    public static readonly List<string> FlagArgs = [
        "--standalone",
        "--sql",
        "--firstchance",
    ];

    /// <summary>
    /// 短い名前のコマンドライン引数の一覧です。
    /// </summary>
    public static readonly List<string> ShortArgs = [
        "-u",
        "-p",
    ];

    /// <summary>
    /// コマンドライン引数を IConfiguration の設定とマッピングするためのエイリアスです。
    /// </summary>
    public static readonly Dictionary<string, string> Aliases = new()
    {
        { "--standalone", "AloeClientSettings:IsStandalone" },
        { "--sql", "AloeClientSettings:IsStandaloneSqlLogging" },
        { "--conn", "AloeClientSettings:ConnectionStringName" },
        { "--firstchance", "AloeClientSettings:IsFirstChanceExceptionLogging" },

        { "--screen", "AloeClientSettings:ScreenCode" },

        { "-u", "AloeClientSettings:User" },
        { "--usr", "AloeClientSettings:User" },
        { "--user", "AloeClientSettings:User" },

        { "-p", "AloeClientSettings:Password" },
        { "--pwd", "AloeClientSettings:Password" },
        { "--pass", "AloeClientSettings:Password" },
        { "--password", "AloeClientSettings:Password" },

        { "--pt", "AloeClientSettings:KarteNumber" },
        { "--patient", "AloeClientSettings:KarteNumber" },
        { "--karte", "AloeClientSettings:KarteNumber" },
    };

    public static IConfigurationRoot CreateConfiguration(string[] args, string fileName = "appsettings.json")
    {
        var processedArgs = ArgsHelper.PreprocessArgs(
            args,
            AloeClientSettings.FlagArgs,
            AloeClientSettings.ShortArgs);

        var config = new ConfigurationBuilder()
            // サービスとして登録する場合でも読めるように exe を基準とする
            .SetBasePath(AppContext.BaseDirectory)
            // 設定ファイル
            .AddJsonFile(fileName, optional: true, reloadOnChange: true)
            // シークレット(開発環境用)
            .AddUserSecrets<App>(optional: true)
            // 環境変数
            .AddEnvironmentVariables()
            // コマンドライン引数
            .AddCommandLine(processedArgs, AloeClientSettings.Aliases)
            .Build();

        return config;
    }

    #endregion static

    /// <summary>
    /// スタンドアローンモードを有効にします。
    /// 通常の Client/Server モードではなく、単独で実行できるようになります。
    /// 直接データベースへ接続するため接続文字列の設定が必要です。
    /// </summary>
    public bool IsStandalone { get; set; }

    /// <summary>
    /// スタンドアローンモードのとき、SQLのログを出力できるようにします。
    /// </summary>
    public bool IsStandaloneSqlLogging { get; set; }

    /// <summary>
    /// 使用する接続文字列のキー名を指定します。
    /// </summary>
    public string ConnectionStringName { get; set; } = "DefaultConnection";

    /// <summary>
    /// 例外が発生したときにTraceレベルでログ出力します。
    /// 内部で握りつぶしている場合や、例外を制御に使っている場合でも捕捉します。
    /// </summary>
    public bool IsFirstChanceExceptionLogging { get; set; }

    /// <summary>
    /// 起動後に開く画面を指定できます。
    /// ただし、指定すると常駐しません。
    /// </summary>
    public ScreenCode ScreenCode { get; set; } = ScreenCode.None;

    /// <summary>
    /// 起動後に画面を開く場合に指定できるログインユーザーです。
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// 起動後に画面を開く場合に指定できるパスワードです。
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 起動後に画面を開く場合に指定できる患者のカルテ番号です。
    /// </summary>
    public string? KarteNumber { get; set; }
}
