namespace Aloe.Apps.MedockLib.Services.VoiceCommand;

/// <summary>
/// 音声コマンドのインテント種別
/// </summary>
public enum VoiceCommandIntent
{
    Unknown,
    CreateAppointment,
    TentativeAppointment,
    CancelAppointment,
    SearchAppointment,
    SetFilter,
}

/// <summary>フィルター設定コマンドのパラメータ</summary>
public record VoiceCommandFilterParams(
    List<int>? SelectedDays,    // 0=日, 1=月 ... 6=土
    List<string>? TimeSlots,    // "09:00-12:00" 形式
    bool ClearAll,              // true = フィルタークリア
    int? RequiredCapacity);     // 最低空き数

/// <summary>
/// 音声コマンドのパース結果
/// </summary>
/// <param name="Intent">検出されたインテント</param>
/// <param name="Date">対象日付</param>
/// <param name="StartMin">開始時刻（0:00からの分数）</param>
/// <param name="PatientName">患者名</param>
/// <param name="Memo">メモ</param>
/// <param name="OriginalTranscript">元の音声認識テキスト</param>
/// <param name="Summary">確認用テキスト（例: 「3月5日 10:00 田中さん - 予約作成」）</param>
/// <param name="FilterParams">フィルター設定パラメータ（SetFilter インテント時のみ）</param>
public record VoiceCommandResult(
    VoiceCommandIntent Intent,
    DateOnly? Date,
    int? StartMin,
    string? PatientName,
    string? Memo,
    string OriginalTranscript,
    string Summary,
    VoiceCommandFilterParams? FilterParams = null);
