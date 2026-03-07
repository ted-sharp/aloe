using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Aloe.Apps.MedockLib.Services.VoiceCommand;

/// <summary>
/// Azure OpenAI を使ったLLMベースの音声コマンドパーサー。
/// エラー時は VoiceCommandParser（正規表現）にフォールバック。
/// </summary>
public sealed class AzureOpenAiVoiceCommandParser(
    IOptions<AzureOpenAiSettings> settings,
    VoiceCommandParser fallbackParser,
    IDateTimeProvider dateTimeProvider,
    ILogger<AzureOpenAiVoiceCommandParser> logger) : IVoiceCommandParser
{
    private static readonly string SystemPrompt = """
        あなたは日本語の音声コマンドを解析するシステムです。
        ユーザーの音声入力を以下のJSON形式で返してください。他のテキストは一切含めないでください。

        {
          "intent": "create" | "tentative" | "cancel" | "search" | "filter" | "unknown",
          "date": "YYYY-MM-DD" または null,
          "time": "HH:mm" または null,
          "patientName": "患者名" または null,
          "filter": {
            "days": [0-6の配列, 0=日曜...6=土曜] または null,
            "timeSlots": ["09:00-12:00"] または ["13:00-17:00"] または null,
            "clearAll": true または false,
            "requiredCapacity": 数値 または null
          } または null
        }

        intentの判断基準:
        - create: 通常の予約を入れる・追加・登録
        - tentative: 仮予約・仮で入れる・とりあえず予約
        - cancel: キャンセル・取り消し・削除
        - search: 確認・検索・調べる・一覧・空き確認
        - filter: フィルター・絞り込み・〇曜日だけ表示・午前のみ・全部表示
        - unknown: 上記以外
        """;

    public VoiceCommandResult Parse(string transcript)
    {
        try
        {
            var today = dateTimeProvider.TodayDateOnly;
            var client = new AzureOpenAIClient(
                new Uri(settings.Value.Endpoint!),
                new AzureKeyCredential(settings.Value.ApiKey!));
            var chatClient = client.GetChatClient(settings.Value.DeploymentName!);

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage($"{SystemPrompt}\n\n今日の日付: {today:yyyy-MM-dd}"),
                new UserChatMessage(transcript)
            };

            var completion = chatClient.CompleteChat(messages,
                new ChatCompletionOptions { ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat() });

            var json = completion.Value.Content[0].Text;
            return BuildResultFromJson(json, transcript, today);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Azure OpenAI parse failed, falling back to regex parser");
            return fallbackParser.Parse(transcript);
        }
    }

    private static VoiceCommandResult BuildResultFromJson(string json, string transcript, DateOnly today)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var intent = root.GetProperty("intent").GetString() switch
        {
            "create" => VoiceCommandIntent.CreateAppointment,
            "tentative" => VoiceCommandIntent.TentativeAppointment,
            "cancel" => VoiceCommandIntent.CancelAppointment,
            "search" => VoiceCommandIntent.SearchAppointment,
            "filter" => VoiceCommandIntent.SetFilter,
            _ => VoiceCommandIntent.Unknown,
        };

        DateOnly? date = null;
        if (root.TryGetProperty("date", out var dateProp) && dateProp.ValueKind != JsonValueKind.Null)
            if (DateOnly.TryParse(dateProp.GetString(), out var d)) date = d;

        int? startMin = null;
        if (root.TryGetProperty("time", out var timeProp) && timeProp.ValueKind != JsonValueKind.Null)
        {
            var timeStr = timeProp.GetString();
            if (timeStr is not null && TimeOnly.TryParse(timeStr, out var t))
                startMin = t.Hour * 60 + t.Minute;
        }

        string? patientName = null;
        if (root.TryGetProperty("patientName", out var nameProp) && nameProp.ValueKind != JsonValueKind.Null)
            patientName = nameProp.GetString();

        VoiceCommandFilterParams? filterParams = null;
        if (intent == VoiceCommandIntent.SetFilter &&
            root.TryGetProperty("filter", out var filterProp) &&
            filterProp.ValueKind != JsonValueKind.Null)
        {
            var clearAll = filterProp.TryGetProperty("clearAll", out var clearProp) && clearProp.GetBoolean();
            List<int>? days = null;
            if (filterProp.TryGetProperty("days", out var daysProp) && daysProp.ValueKind == JsonValueKind.Array)
                days = daysProp.EnumerateArray().Select(e => e.GetInt32()).ToList();
            List<string>? timeSlots = null;
            if (filterProp.TryGetProperty("timeSlots", out var tsProp) && tsProp.ValueKind == JsonValueKind.Array)
                timeSlots = tsProp.EnumerateArray().Select(e => e.GetString()!).ToList();
            int? cap = null;
            if (filterProp.TryGetProperty("requiredCapacity", out var capProp) && capProp.ValueKind == JsonValueKind.Number)
                cap = capProp.GetInt32();
            filterParams = new VoiceCommandFilterParams(days, timeSlots, clearAll, cap);
        }

        var summary = BuildSummary(intent, date, startMin, patientName, filterParams);
        return new VoiceCommandResult(intent, date, startMin, patientName, null, transcript, summary, filterParams);
    }

    private static string BuildSummary(VoiceCommandIntent intent, DateOnly? date, int? startMin,
        string? patientName, VoiceCommandFilterParams? filter)
    {
        if (intent == VoiceCommandIntent.SetFilter && filter?.ClearAll == true)
            return "フィルタークリア";

        if (intent == VoiceCommandIntent.SetFilter)
        {
            var parts2 = new List<string>();
            if (filter?.SelectedDays is { Count: > 0 } days)
                parts2.Add(string.Join("・", days.Select(d => "日月火水木金土"[d] + "曜")));
            if (filter?.TimeSlots is { Count: > 0 } ts)
                parts2.Add(ts[0].StartsWith("09") ? "午前" : "午後");
            if (filter?.RequiredCapacity is not null)
                parts2.Add($"空き{filter.RequiredCapacity}以上");
            parts2.Add("フィルター設定");
            return string.Join(" ", parts2);
        }

        var parts = new List<string>();
        if (date.HasValue) parts.Add(date.Value.ToString("M月d日"));
        if (startMin.HasValue) parts.Add($"{startMin.Value / 60}:{startMin.Value % 60:D2}");
        if (patientName is not null) parts.Add($"{patientName}さん");
        parts.Add(intent switch
        {
            VoiceCommandIntent.CreateAppointment => "予約作成",
            VoiceCommandIntent.TentativeAppointment => "仮予約作成",
            VoiceCommandIntent.CancelAppointment => "予約キャンセル",
            VoiceCommandIntent.SearchAppointment => "予約検索",
            _ => "不明なコマンド",
        });
        return string.Join(" ", parts);
    }
}
