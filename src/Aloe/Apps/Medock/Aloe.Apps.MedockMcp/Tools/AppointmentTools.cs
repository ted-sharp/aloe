using System.ComponentModel;
using System.Text.Json;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.Dtos.Appointments;
using Aloe.Apps.MedockLib.Services.VoiceCommand;
using ModelContextProtocol.Server;

namespace Aloe.Apps.MedockMcp.Tools;

[McpServerToolType]
public class AppointmentTools(
    IAppointmentService appointmentService,
    IDateTimeProvider dateTimeProvider,
    IVoiceCommandParser voiceCommandParser)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [McpServerTool, Description("予約を作成する。日付(yyyy-MM-dd)、開始時刻(分, 例: 540=09:00)、患者名を指定。FloorIdが不明な場合はデフォルトを使用。")]
    public async Task<string> CreateAppointment(
        string date,
        int startMin,
        string? patientName = null,
        string? memo = null)
    {
        if (!DateOnly.TryParse(date, out var parsedDate))
            return $"エラー: 日付の形式が不正です: {date} (yyyy-MM-dd形式で指定してください)";

        // デフォルトの施設・フロアを取得
        // MCP経由ではユーザーコンテキストが無いため、最初の施設のデフォルトフロアを使用
        Guid? patientId = null;
        if (patientName is not null)
        {
            // 施設IDはデフォルトフロアから推測（簡略化のため直接フロアIDを使う）
            // 本来は認証情報から施設IDを取得するべき
        }

        var dto = new CreateAppointmentDto
        {
            Date = parsedDate,
            StartMin = startMin,
            PatientId = patientId,
            Memo = memo,
        };

        // FloorId はデフォルトを使用する必要があるが、facilityId が必要
        // MCP経由では認証コンテキストが無いため、メモに患者名を含める
        if (patientName is not null && dto.Memo is null)
            dto.Memo = $"患者名: {patientName}";
        else if (patientName is not null)
            dto.Memo = $"患者名: {patientName} / {dto.Memo}";

        var result = await appointmentService.CreateAppointmentAsync(dto);

        if (!result.IsSuccess)
            return $"エラー: {result.ErrorMessage}";

        var appt = result.Value!;
        return JsonSerializer.Serialize(new
        {
            message = "予約を作成しました",
            appointment = new
            {
                appt.Id,
                date = appt.Date.ToString("yyyy-MM-dd"),
                startTime = MinutesToTimeString(appt.StartMin),
                appt.PatientName,
                appt.Memo,
                appt.Status,
            },
        }, JsonOptions);
    }

    [McpServerTool, Description("指定期間の予約一覧を取得する。startDate(yyyy-MM-dd)は必須、endDateは省略時はstartDateと同じ日。patientNameで絞り込み可能。")]
    public async Task<string> SearchAppointments(
        string startDate,
        string? endDate = null,
        string? patientName = null)
    {
        if (!DateOnly.TryParse(startDate, out var parsedStart))
            return $"エラー: 開始日の形式が不正です: {startDate}";

        var parsedEnd = parsedStart;
        if (endDate is not null && !DateOnly.TryParse(endDate, out parsedEnd))
            return $"エラー: 終了日の形式が不正です: {endDate}";

        var result = await appointmentService.GetAppointmentsAsync(parsedStart, parsedEnd);

        if (!result.IsSuccess)
            return $"エラー: {result.ErrorMessage}";

        var appointments = result.Value!;

        // 患者名フィルター
        if (patientName is not null)
        {
            appointments = appointments
                .Where(a => a.PatientName?.Contains(patientName) == true)
                .ToList();
        }

        return JsonSerializer.Serialize(new
        {
            message = $"{parsedStart:yyyy-MM-dd}～{parsedEnd:yyyy-MM-dd} の予約一覧",
            count = appointments.Count,
            appointments = appointments.Select(a => new
            {
                a.Id,
                date = a.Date.ToString("yyyy-MM-dd"),
                startTime = MinutesToTimeString(a.StartMin),
                a.PatientName,
                a.OrganizationName,
                a.Status,
                a.Memo,
            }),
        }, JsonOptions);
    }

    [McpServerTool, Description("予約をキャンセル（論理削除）する。予約IDをGUID形式で指定。")]
    public async Task<string> CancelAppointment(string appointmentId)
    {
        if (!Guid.TryParse(appointmentId, out var parsedId))
            return $"エラー: 予約IDの形式が不正です: {appointmentId}";

        var result = await appointmentService.DeleteAppointmentAsync(parsedId);

        if (!result.IsSuccess)
            return $"エラー: {result.ErrorMessage}";

        return JsonSerializer.Serialize(new
        {
            message = "予約をキャンセルしました",
            appointmentId,
        }, JsonOptions);
    }

    [McpServerTool, Description("日本語の自然言語テキストを予約コマンドにパースする。音声認識の結果テキストを渡すと、インテント・日付・時刻・患者名を抽出して返す。")]
    public string ParseVoiceCommand(string transcript)
    {
        var result = voiceCommandParser.Parse(transcript);

        return JsonSerializer.Serialize(new
        {
            result.Intent,
            date = result.Date?.ToString("yyyy-MM-dd"),
            startMin = result.StartMin,
            startTime = result.StartMin.HasValue ? MinutesToTimeString(result.StartMin.Value) : null,
            result.PatientName,
            result.Memo,
            result.OriginalTranscript,
            result.Summary,
        }, JsonOptions);
    }

    [McpServerTool, Description("今日の日付と現在時刻を取得する。予約操作の参照用。")]
    public string GetCurrentDateTime()
    {
        var now = dateTimeProvider.Now;
        var today = dateTimeProvider.TodayDateOnly;

        return JsonSerializer.Serialize(new
        {
            today = today.ToString("yyyy-MM-dd"),
            dayOfWeek = today.DayOfWeek.ToString(),
            dayOfWeekJa = today.DayOfWeek switch
            {
                DayOfWeek.Monday => "月曜日",
                DayOfWeek.Tuesday => "火曜日",
                DayOfWeek.Wednesday => "水曜日",
                DayOfWeek.Thursday => "木曜日",
                DayOfWeek.Friday => "金曜日",
                DayOfWeek.Saturday => "土曜日",
                DayOfWeek.Sunday => "日曜日",
                _ => "",
            },
            currentTime = now.ToString("HH:mm"),
        }, JsonOptions);
    }

    private static string MinutesToTimeString(int minutes) =>
        $"{minutes / 60}:{minutes % 60:D2}";
}
