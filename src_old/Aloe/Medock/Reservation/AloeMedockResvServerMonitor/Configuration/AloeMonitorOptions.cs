using Aloe.Common.AloeCoreLib.Util;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Configuration;

/// <summary>
/// コマンドライン引数、設定ファイル、シークレット、環境変数から値を読み込みます。
/// </summary>
public class AloeMonitorOptions
{
    /// <summary>
    /// 監視するサービス名です。
    /// </summary>
    public string WindowsServiceName { get; set; } = "AbcAloeMedockResvServer";

    /// <summary>
    /// 監視するサービスの実行ファイルです。
    /// </summary>
    public string WindowsServicePath { get; set; } = "AloeMedockResvServer.exe";

    /// <summary>
    /// サービスの説明（オプション）。
    /// </summary>
    public string? WindowsServiceDescription { get; set; } = "Aloe Medock Reservation Server";

    /// <summary>
    /// サービスの起動種別（auto/demand/disabled）。
    /// </summary>
    public string WindowsServiceStartType { get; set; } = "auto";

    /// <summary>
    /// サービスの実行アカウント（例: LocalSystem, NT AUTHORITY\LocalService）。
    /// </summary>
    public string WindowsServiceAccount { get; set; } = "LocalSystem";

    /// <summary>
    /// サービスが依存する他のサービス（カンマ区切り）。
    /// </summary>
    public string? WindowsServiceDependencies { get; set; } = "postgresql-x64-17";

    /// <summary>
    /// サービスの回復のエラーカウントリセット(秒)。
    /// 1日=86400秒
    /// </summary>
    public int WindowsServiceFailureResets { get; set; } = 86400;

    /// <summary>
    /// サービスの回復のエラー時のアクション。
    /// </summary>
    public string? WindowsServiceFailureActions { get; set; } = "restart/60000/restart/60000/restart/60000";

    /// <summary>
    /// サービス監視の間隔(ミリ秒)です。
    /// </summary>
    public int MonitoringInterval { get; set; } = 1000;

    /// <summary>
    /// 監視するサービスの実行ファイルのフルパスを取得します。
    /// </summary>
    public string GetWindowsServiceFullPath()
    {
        var path = this.WindowsServicePath;
        var fullPath = PathHelper.FromBase(path);
        return fullPath;
    }
}
