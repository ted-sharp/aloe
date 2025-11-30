using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Configuration;

/// <summary>
/// コマンドライン引数、設定ファイル、シークレット、環境変数から値を読み込みます。
/// </summary>
public static class AloeMonitorConfig
{

    public static IConfigurationRoot CreateConfiguration(string[] args, string fileName = "appsettings.json")
    {
        var config = new ConfigurationBuilder()
            // サービスとして登録する場合でも読めるように exe を基準とする
            .SetBasePath(AppContext.BaseDirectory)
            // 設定ファイル
            .AddJsonFile(fileName, optional: true, reloadOnChange: true)
            // シークレット(開発環境用)
            .AddUserSecrets<AloeMonitorOptions>(optional: true)
            // 環境変数
            .AddEnvironmentVariables()
            // コマンドライン引数
            .AddCommandLine(args)
            .Build();

        return config;
    }
}
