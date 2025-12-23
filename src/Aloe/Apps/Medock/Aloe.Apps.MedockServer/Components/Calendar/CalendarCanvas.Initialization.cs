using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.JSInterop;
using System.Diagnostics;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class CalendarCanvas
{
    private async Task InitializeCalendarAsync()
    {
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
        var calendarData = await this.CalendarDataService.BuildCalendarDataAsync(
            this.Appointments ?? Enumerable.Empty<AppointmentDto>(),
            this.MainStats ?? new Dictionary<string, List<AppointmentStats>>(),
            this.MainStatsGrayedOut ?? new Dictionary<string, bool>(),
            this.Holidays ?? new Dictionary<string, string>());
        buildSw.Stop();
        Console.WriteLine($"[Performance] CalendarCanvas.InitializeCalendarAsync BuildCalendarData: {buildSw.ElapsedMilliseconds}ms");

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
        Console.WriteLine($"[Performance] CalendarCanvas.InitializeCalendarAsync BuildDataObject/BuildOptions: {interopSw.ElapsedMilliseconds}ms");

        var jsInitSw = Stopwatch.StartNew();
        await this.JSRuntime.InvokeVoidAsync(
            "MedockCalendar.init",
            this.ContainerId,
            data,
            options,
            this._dotNetRef);
        jsInitSw.Stop();
        Console.WriteLine($"[Performance] CalendarCanvas.InitializeCalendarAsync JS init: {jsInitSw.ElapsedMilliseconds}ms");

        // Set initial view
        var jsViewSw = Stopwatch.StartNew();
        await this.JSRuntime.InvokeVoidAsync(
            "MedockCalendar.changeView",
            this.ViewType,
            this.CurrentDate.ToString("yyyy-MM-dd"));
        jsViewSw.Stop();
        Console.WriteLine($"[Performance] CalendarCanvas.InitializeCalendarAsync JS changeView: {jsViewSw.ElapsedMilliseconds}ms");
    }
}

