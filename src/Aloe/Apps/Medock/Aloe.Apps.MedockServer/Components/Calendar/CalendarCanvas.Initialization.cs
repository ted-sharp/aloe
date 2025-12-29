using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Text.Json;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class CalendarCanvas
{
    private async Task InitializeCalendarAsync()
    {
        // ローディング中は初期化を延期（OnParametersSetAsync で IsLoading = false になった時に実行される）
        if (this.IsLoading)
        {
            return;
        }

        // ES Moduleの読み込み完了を待つ
        var maxRetries = 50;
        var retryDelay = 100; // 100ms
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var isReady = await this.JSRuntime.InvokeAsync<bool>("eval", "typeof window.MedockCalendar !== 'undefined'");
                if (isReady)
                {
                    break;
                }
            }
            catch
            {
                // エラーは無視してリトライ
            }

            if (i < maxRetries - 1)
            {
                await Task.Delay(retryDelay);
            }
            else
            {
                // 最終的に読み込まれなかった場合はエラーをログに出力
                Console.WriteLine("Warning: MedockCalendar module not loaded after retries");
                return;
            }
        }

        var buildSw = Stopwatch.StartNew();

        // 【最適化版】EquipmentStatsOptimized がある場合はそれを使用
        CalendarDataDto calendarData;
        if (this.EquipmentStatsOptimized != null && this.EquipmentStatsOptimized.Count > 0)
        {
            calendarData = await this.CalendarDataService.BuildCalendarDataFromOptimizedAsync(
                this.Appointments ?? Enumerable.Empty<AppointmentDto>(),
                this.MainStats ?? new Dictionary<string, List<AppointmentStats>>(),
                this.MainStatsGrayedOut ?? new Dictionary<string, bool>(),
                this.Holidays ?? new Dictionary<string, string>(),
                this.FilterTimeSlots,
                this.EquipmentStatsOptimized);
        }
        else
        {
            calendarData = await this.CalendarDataService.BuildCalendarDataAsync(
                this.Appointments ?? Enumerable.Empty<AppointmentDto>(),
                this.MainStats ?? new Dictionary<string, List<AppointmentStats>>(),
                this.MainStatsGrayedOut ?? new Dictionary<string, bool>(),
                this.Holidays ?? new Dictionary<string, string>(),
                this.FilterTimeSlots,
                this.EquipmentStats);
        }

        buildSw.Stop();
        Console.WriteLine($"[Performance] BuildCalendarData: {buildSw.ElapsedMilliseconds}ms");
        Console.WriteLine($"  - Appointments: {calendarData.Appointments.Count}");
        Console.WriteLine($"  - MainStats dates: {calendarData.MainStats.Count}");
        Console.WriteLine($"  - EquipmentStats dates: {calendarData.EquipmentStats.Count}");
        var totalEquipmentResources = calendarData.EquipmentStats.Sum(kvp => kvp.Value.Resources.Count);
        Console.WriteLine($"  - Total Equipment Resources: {totalEquipmentResources}");

        var interopSw = Stopwatch.StartNew();
        var data = CalendarCanvasInterop.BuildDataObject(calendarData);
        var options = CalendarCanvasInterop.BuildOptions(
            this.WeekDays,
            this.ShowSlots,
            this.ShowSimpleView,
            this.StartHour,
            this.EndHour,
            this.BusinessHours);
        interopSw.Stop();
        Console.WriteLine($"[Performance] BuildDataObject/BuildOptions: {interopSw.ElapsedMilliseconds}ms");

        // JSON シリアライゼーションのサイズを計測
        var serializeSw = Stopwatch.StartNew();
        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data);
        serializeSw.Stop();
        var sizeKb = jsonBytes.Length / 1024.0;
        Console.WriteLine($"[Performance] JSON serialization: {serializeSw.ElapsedMilliseconds}ms (size: {sizeKb:F2} KB)");

        var jsInitSw = Stopwatch.StartNew();
        await this.JSRuntime.InvokeVoidAsync(
            "MedockCalendar.init",
            this.ContainerId,
            data,
            options,
            this._dotNetRef);
        jsInitSw.Stop();
        Console.WriteLine($"[Performance] JS init: {jsInitSw.ElapsedMilliseconds}ms");

        // Set initial view
        var jsViewSw = Stopwatch.StartNew();
        await this.JSRuntime.InvokeVoidAsync(
            "MedockCalendar.changeView",
            this.ViewType,
            this.CurrentDate.ToString("yyyy-MM-dd"));
        jsViewSw.Stop();
        Console.WriteLine($"[Performance] CalendarCanvas.InitializeCalendarAsync JS changeView: {jsViewSw.ElapsedMilliseconds}ms");

        // 初期化完了フラグを設定
        this._isInitialized = true;
        this._lastViewType = this.ViewType;
        this._lastDate = this.CurrentDate;
        this._lastWeekDays = this.WeekDays;
        this._lastShowSlots = this.ShowSlots;
        this._lastShowSimpleView = this.ShowSimpleView;
        this._lastMainStatsCount = this.MainStats?.Count ?? 0;
    }
}

