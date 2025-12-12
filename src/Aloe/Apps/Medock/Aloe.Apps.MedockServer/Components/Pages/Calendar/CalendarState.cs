using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockServer.Components.Calendar;
using Aloe.Apps.MedockServer.Components.FAB;

namespace Aloe.Apps.MedockServer.Components.Pages;

/// <summary>
/// カレンダーページの状態管理クラス
/// </summary>
public class CalendarState
{
    // ビュー状態
    public DateOnly CurrentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public CalendarViewType CurrentView { get; set; } = CalendarViewType.Month;
    public int WeekDays { get; set; } = 7;
    public bool ShowSlots { get; set; } = true;
    public bool ShowFilterPanel { get; set; }
    public bool ShowSimpleView { get; set; }
    public bool ShowEquipmentGraph { get; set; }

    // ユーザー情報
    public string UserInitial { get; set; } = "U";
    public string UserDisplayName { get; set; } = "";
    public string UserEmail { get; set; } = "";
    public string TenantName { get; set; } = "";
    public string FacilityName { get; set; } = "";
    public string UserRole { get; set; } = "";
    public Guid? CurrentFacilityId { get; set; }
    public bool HasMultipleFacilities { get; set; }
    public List<FacilityInfo>? AvailableFacilities { get; set; }

    // モーダル状態
    public bool IsModalOpen { get; set; }
    public DateOnly? ModalDate { get; set; }
    public TimeOnly? ModalTime { get; set; }
    public Guid? SelectedAppointmentId { get; set; }

    // 選択状態
    public DateOnly? SelectedDate { get; set; }
    public (DateOnly Start, DateOnly End)? SelectedDateRange { get; set; }

    // カレンダーデータ
    public Dictionary<string, CalendarDayStats> DayStats { get; set; } = new();
    public Dictionary<string, CalendarDayStats> OriginalDayStats { get; set; } = new();
    public List<CalendarAppointment> Appointments { get; set; } = new();
    public Dictionary<string, string> Holidays { get; set; } = new();

    // 営業時間
    public BusinessHoursDto? BusinessHours { get; set; }
    public int StartHour { get; set; } = 8;
    public int EndHour { get; set; } = 18;

    // フィルター
    public List<SearchFilterPanel.FilterItem> AvailableEquipments { get; set; } = new();
    public SearchFilterPanel.SearchFilter? CurrentFilter { get; set; }

    /// <summary>
    /// アクティブなフィルター数（ナビバーのバッジ表示用）
    /// </summary>
    public int ActiveFilterCount =>
        (this.CurrentFilter?.SelectedDays.Any() == true ? 1 : 0) +
        (this.CurrentFilter?.TimeSlots.Any() == true ? 1 : 0) +
        (this.CurrentFilter?.RequiredCapacity > 1 ? 1 : 0) +
        (this.CurrentFilter?.EquipIds.Any() == true ? 1 : 0);

    /// <summary>
    /// 現在の期間のタイトルを取得
    /// </summary>
    public string GetCurrentPeriodTitle()
    {
        return this.CurrentView switch
        {
            CalendarViewType.Year => $"{this.CurrentDate.Year}年",
            CalendarViewType.Month => $"{this.CurrentDate.Year}年{this.CurrentDate.Month}月",
            CalendarViewType.Week => this.WeekDays == 1
                ? $"{this.CurrentDate.Year}年{this.CurrentDate.Month}月{this.CurrentDate.Day}日"
                : $"{this.CurrentDate.Year}年{this.CurrentDate.Month}月{this.CurrentDate.Day}日〜",
            _ => ""
        };
    }

    /// <summary>
    /// 前の期間へ移動
    /// </summary>
    public void PreviousPeriod()
    {
        this.CurrentDate = this.CurrentView switch
        {
            CalendarViewType.Year => this.CurrentDate.AddYears(-1),
            CalendarViewType.Month => this.CurrentDate.AddMonths(-1),
            CalendarViewType.Week => this.CurrentDate.AddDays(-this.WeekDays),
            _ => this.CurrentDate
        };
    }

    /// <summary>
    /// 次の期間へ移動
    /// </summary>
    public void NextPeriod()
    {
        this.CurrentDate = this.CurrentView switch
        {
            CalendarViewType.Year => this.CurrentDate.AddYears(1),
            CalendarViewType.Month => this.CurrentDate.AddMonths(1),
            CalendarViewType.Week => this.CurrentDate.AddDays(this.WeekDays),
            _ => this.CurrentDate
        };
    }

    /// <summary>
    /// 今日へ移動
    /// </summary>
    public void GoToToday()
    {
        this.CurrentDate = DateOnly.FromDateTime(DateTime.Today);
    }

    /// <summary>
    /// ビュー切り替え
    /// </summary>
    public void SetView(CalendarViewType view)
    {
        this.CurrentView = view;
    }

    /// <summary>
    /// 文字列からビューを設定
    /// </summary>
    public void SetViewFromString(string view)
    {
        this.CurrentView = view.ToLower() switch
        {
            "year" => CalendarViewType.Year,
            "month" => CalendarViewType.Month,
            "week" => CalendarViewType.Week,
            _ => CalendarViewType.Month
        };
    }

    /// <summary>
    /// モーダルを開く
    /// </summary>
    public void OpenModal(DateOnly date, TimeOnly time, Guid? appointmentId = null)
    {
        this.SelectedAppointmentId = appointmentId;
        this.ModalDate = date;
        this.ModalTime = time;
        this.IsModalOpen = true;
    }

    /// <summary>
    /// モーダルを閉じる
    /// </summary>
    public void CloseModal()
    {
        this.IsModalOpen = false;
        this.SelectedAppointmentId = null;
        this.ModalDate = null;
        this.ModalTime = null;
    }
}
