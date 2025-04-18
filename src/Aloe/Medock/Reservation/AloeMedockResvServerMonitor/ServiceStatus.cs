using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Aloe.Medock.Reservation.AloeMedockResvServerMonitor.Assets;

namespace Aloe.Medock.Reservation.AloeMedockResvServerMonitor;

public class ServiceStatus
{
    // スレッドセーフにするために lock するか、volatile 修飾子なども検討してください。
    public ServiceControllerStatus? State { get; private set; } = null;

    public string StateText { get; set; } = "-- -";

    public Icon? Icon { get; private set; } = null;

    public Image? Image { get; private set; } = null;

    public bool CanStartStop { get; private set; }

    public bool CanRegisterUnregister { get; private set; }

    public string RegisterMenuText { get; private set; } = "---";

    public Image? RegisterMenuImage { get; private set; } = null;

    public string StartStopMenuText { get; private set; } = "---";

    public Image? StartStopMenuImage { get; private set; } = null;

    public void SetState(ServiceControllerStatus? state)
    {
        this.State = state;
        this.StateText = state.ToStateText();
        this.Icon = state.ToIcon();
        this.Image = state.ToImage();
        this.CanStartStop = state.CanStartStop();
        // 未登録か止まっているときのみ有効化
        this.CanRegisterUnregister = state is null or ServiceControllerStatus.Stopped;
        this.RegisterMenuText = state.ToRegistrationMenuText();
        this.RegisterMenuImage = state.ToRegistrationMenuImage();
        this.StartStopMenuText = state.ToStartStopMenuText();
        this.StartStopMenuImage = state.ToStartStopMenuImage();
    }

    public void SetNotFound()
    {
        this.State = null;
        this.StateText = "---";
        this.Icon = Icons.Circle.Value;
        this.Image = Images.Circle.Value;
        this.CanStartStop = false;
        this.CanRegisterUnregister = false;
        this.RegisterMenuText = "---";
        this.RegisterMenuImage = null;
        this.StartStopMenuText = "---";
        this.StartStopMenuImage = null;
    }
}

public static class ServiceControllerStatusExtensions
{
    /// <summary>
    /// ServiceControllerStatus を表示用の状態テキストに変換します。
    /// </summary>
    public static string ToStateText(this ServiceControllerStatus? status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => "実行中",
            ServiceControllerStatus.Stopped => "停止",
            ServiceControllerStatus.StartPending => "起動中",
            ServiceControllerStatus.StopPending => "停止中",
            ServiceControllerStatus.ContinuePending => "再開中",
            ServiceControllerStatus.PausePending => "一時停止中",
            ServiceControllerStatus.Paused => "中断",
            _ => "---",
        };
    }

    /// <summary>
    /// 現在のステータスで開始/終了の操作が可能かどうかを返します。
    /// </summary>
    public static bool CanStartStop(this ServiceControllerStatus? status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => true,
            ServiceControllerStatus.Stopped => true,
            _ => false,
        };
    }

    /// <summary>
    /// サービス登録メニュー用のテキストを取得します。
    /// </summary>
    public static string ToRegistrationMenuText(this ServiceControllerStatus? status)
    {
        return status is null ? "サービス登録" : "サービス解除";
    }

    /// <summary>
    /// サービス登録メニュー用のイメージを取得します。
    /// </summary>
    public static Image? ToRegistrationMenuImage(this ServiceControllerStatus? status)
    {
        return status is null ? Images.Add.Value : Images.Remove.Value;
    }

    /// <summary>
    /// サービス開始／停止メニュー用のテキストを取得します。
    /// </summary>
    public static string ToStartStopMenuText(this ServiceControllerStatus? status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => "サービス停止",
            ServiceControllerStatus.Stopped => "サービス開始",
            ServiceControllerStatus.StartPending => "起動中",
            ServiceControllerStatus.StopPending => "停止中",
            ServiceControllerStatus.ContinuePending => "再開中",
            ServiceControllerStatus.PausePending => "一時停止中",
            ServiceControllerStatus.Paused => "中断",
            _ => "---",
        };
    }

    /// <summary>
    /// サービス開始／停止メニュー用のイメージを取得します。
    /// </summary>
    public static Image? ToStartStopMenuImage(this ServiceControllerStatus? status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => Images.Stop.Value,
            ServiceControllerStatus.Stopped => Images.Play.Value,
            _ => null,
        };
    }

    /// <summary>
    /// ステータスに応じたアイコンを取得します。
    /// </summary>
    public static Icon ToIcon(this ServiceControllerStatus? status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => Icons.PlayCircle.Value,
            ServiceControllerStatus.Stopped => Icons.StopCircle.Value,
            ServiceControllerStatus.StartPending => Icons.Hourglass.Value,
            ServiceControllerStatus.StopPending => Icons.Hourglass.Value,
            ServiceControllerStatus.ContinuePending => Icons.Hourglass.Value,
            ServiceControllerStatus.PausePending => Icons.Hourglass.Value,
            ServiceControllerStatus.Paused => Icons.PauseCircle.Value,
            _ => Icons.Cancel.Value,
        };
    }

    /// <summary>
    /// ステータスに応じたアイコンを取得します。
    /// </summary>
    public static Image ToImage(this ServiceControllerStatus? status)
    {
        return status switch
        {
            ServiceControllerStatus.Running => Images.PlayCircle.Value,
            ServiceControllerStatus.Stopped => Images.StopCircle.Value,
            ServiceControllerStatus.StartPending => Images.Hourglass.Value,
            ServiceControllerStatus.StopPending => Images.Hourglass.Value,
            ServiceControllerStatus.ContinuePending => Images.Hourglass.Value,
            ServiceControllerStatus.PausePending => Images.Hourglass.Value,
            ServiceControllerStatus.Paused => Images.PauseCircle.Value,
            _ => Images.Cancel.Value,
        };
    }
}
