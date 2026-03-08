namespace Aloe.Apps.MedockLib.Services.VoiceCommand;

/// <summary>
/// 音声認識テキストを予約コマンドにパースするインターフェース
/// </summary>
public interface IVoiceCommandParser
{
    /// <summary>
    /// 音声認識テキストをパースして予約コマンドに変換する
    /// </summary>
    /// <param name="transcript">音声認識テキスト</param>
    /// <returns>パース結果</returns>
    VoiceCommandResult Parse(string transcript);
}
