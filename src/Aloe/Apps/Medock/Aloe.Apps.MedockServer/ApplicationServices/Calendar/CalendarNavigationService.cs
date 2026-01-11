using Aloe.Apps.MedockServer.Components;
using Aloe.Apps.MedockServer.Components.Pages;

namespace Aloe.Apps.MedockServer.ApplicationServices.Calendar;

/// <summary>
/// カレンダーナビゲーション処理を統合するサービス
/// 前後の期間移動、データ更新、UI再レンダリングをまとめて処理
/// </summary>
public class CalendarNavigationService
{
    /// <summary>
    /// ナビゲーション方向の列挙型
    /// </summary>
    public enum NavigationDirection
    {
        /// <summary>前の期間</summary>
        PreviousPeriod,

        /// <summary>次の期間</summary>
        NextPeriod,

        /// <summary>前の大きな期間（1年など）</summary>
        PreviousBigPeriod,

        /// <summary>次の大きな期間（1年など）</summary>
        NextBigPeriod
    }

    /// <summary>
    /// ナビゲーション処理を実行
    /// 指定された方向に期間を移動し、データを更新してUIを再レンダリング
    /// </summary>
    /// <param name="direction">ナビゲーション方向</param>
    /// <param name="state">カレンダー状態</param>
    /// <param name="refreshCallback">データ更新処理のコールバック</param>
    /// <param name="stateChangedCallback">UI再レンダリング処理のコールバック</param>
    public async Task NavigateAsync(
        NavigationDirection direction,
        CalendarState state,
        Func<Task> refreshCallback,
        Action stateChangedCallback)
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        if (refreshCallback == null)
        {
            throw new ArgumentNullException(nameof(refreshCallback));
        }

        if (stateChangedCallback == null)
        {
            throw new ArgumentNullException(nameof(stateChangedCallback));
        }

        // 1. 期間を移動
        switch (direction)
        {
            case NavigationDirection.PreviousPeriod:
                state.PreviousPeriod();
                break;

            case NavigationDirection.NextPeriod:
                state.NextPeriod();
                break;

            case NavigationDirection.PreviousBigPeriod:
                state.PreviousBigPeriod();
                break;

            case NavigationDirection.NextBigPeriod:
                state.NextBigPeriod();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown navigation direction");
        }

        // 2. データを更新
        await refreshCallback();

        // 3. UIを再レンダリング
        stateChangedCallback();
    }
}
