namespace Aloe.Medock.Reservation.AloeMedockResvServer.Configuration;

/// <summary>
/// コマンドライン引数、設定ファイル、シークレット、環境変数から値を読み込みます。
/// </summary>
public class AloeServerArgs
{
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
