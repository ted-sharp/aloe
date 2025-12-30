using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services.Dtos;
using Microsoft.JSInterop;
using System.Diagnostics;
using System.Text.Json;

namespace Aloe.Apps.MedockServer.Components.Calendar;

public partial class CalendarCanvas
{
    private async Task UpdateDataAsync()
    {
        if (!this._isInitialized) return;

        var sw = Stopwatch.StartNew();
        try
        {
            var buildSw = Stopwatch.StartNew();

            var calendarData = await this.CalendarDataService.BuildCalendarDataAsync(
                this.Appointments ?? Enumerable.Empty<AppointmentDto>(),
                this.MainStats ?? new Dictionary<string, List<AppointmentStats>>(),
                this.MainStatsGrayedOut ?? new Dictionary<string, bool>(),
                this.Holidays ?? new Dictionary<string, string>(),
                this.FilterTimeSlots,
                this.EquipmentStatsOptimized,
                this.BusinessHours);

            buildSw.Stop();
            Console.WriteLine($"[Performance] BuildCalendarData: {buildSw.ElapsedMilliseconds}ms");
            Console.WriteLine($"  - Appointments: {calendarData.Appointments.Count}");
            Console.WriteLine($"  - MainStats dates: {calendarData.MainStats.Count}");
            Console.WriteLine($"  - EquipmentStats dates: {calendarData.EquipmentStats.Count}");
            var totalEquipmentResources = calendarData.EquipmentStats.Sum(kvp => kvp.Value.Count);
            Console.WriteLine($"  - Total Equipment Resources: {totalEquipmentResources}");

            var interopSw = Stopwatch.StartNew();
            var data = CalendarCanvasInterop.BuildDataObject(calendarData);
            interopSw.Stop();
            Console.WriteLine($"[Performance] BuildDataObject: {interopSw.ElapsedMilliseconds}ms");

            // JSON シリアライゼーションのサイズを計測
            var serializeSw = Stopwatch.StartNew();
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(data);
            serializeSw.Stop();
            var sizeKb = jsonBytes.Length / 1024.0;
            Console.WriteLine($"[Performance] JSON serialization: {serializeSw.ElapsedMilliseconds}ms (size: {sizeKb:F2} KB)");

            var jsSw = Stopwatch.StartNew();
            await this.JSRuntime.InvokeVoidAsync("MedockCalendar.updateData", data);
            jsSw.Stop();
            Console.WriteLine($"[Performance] JS updateData: {jsSw.ElapsedMilliseconds}ms");
        }
        catch (TaskCanceledException)
        {
            // コンポーネントが破棄されたり、パラメータが頻繁に更新された場合に発生する可能性がある
            // これは正常な動作なので無視する
        }
        finally
        {
            sw.Stop();
            Console.WriteLine($"[Performance] UpdateDataAsync total: {sw.ElapsedMilliseconds}ms");
            Console.WriteLine("---");
        }
    }

    private async Task ChangeViewAsync()
    {
        if (!this._isInitialized) return;

        try
        {
            // Update options if weekDays, showSlots, or showSimpleView changed
            if (this._lastWeekDays != this.WeekDays || this._lastShowSlots != this.ShowSlots || this._lastShowSimpleView != this.ShowSimpleView)
            {
                var options = CalendarCanvasInterop.BuildOptions(
                    this.WeekDays,
                    this.ShowSlots,
                    this.ShowSimpleView,
                    this.StartHour,
                    this.EndHour,
                    this.BusinessHours);
                await this.JSRuntime.InvokeVoidAsync("MedockCalendar.setOptions", options);
            }

            await this.JSRuntime.InvokeVoidAsync(
                "MedockCalendar.changeView",
                this.ViewType,
                this.CurrentDate.ToString("yyyy-MM-dd"));
        }
        catch (TaskCanceledException)
        {
            // コンポーネントが破棄されたり、パラメータが頻繁に更新された場合に発生する可能性がある
            // これは正常な動作なので無視する
        }
    }
}

