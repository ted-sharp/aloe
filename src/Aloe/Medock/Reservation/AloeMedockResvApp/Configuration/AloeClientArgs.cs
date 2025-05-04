using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.Configuration;

/// <summary>
/// コマンドライン引数、設定ファイル、シークレット、環境変数から値を読み込みます。
/// </summary>
public class AloeClientArgs
{
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
    /// 起動後に画面を開く場合に指定できるログインユーザーです。
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// 起動後に画面を開く場合に指定できるパスワードです。
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// 起動後に開く画面を指定できます。
    /// </summary>
    public ScreenCode ScreenCode { get; set; } = ScreenCode.None;

    /// <summary>
    /// アプリを常駐します。
    /// </summary>
    public bool IsResident { get; set; }

    /// <summary>
    /// 起動後に画面を開く場合に指定できる患者のカルテ番号です。
    /// </summary>
    public string? KarteNumber { get; set; }
}
