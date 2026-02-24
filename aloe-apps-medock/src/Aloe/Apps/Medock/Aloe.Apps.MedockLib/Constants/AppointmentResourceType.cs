namespace Aloe.Apps.MedockLib.Constants;

/// <summary>
/// 予約リソースタイプ
/// カレンダーでの表示方法を決定する
/// </summary>
public enum AppointmentResourceType
{
    /// <summary>
    /// Main: メインスロット（1）
    /// 30分区切りで業務時間を分割し、カレンダーに棒グラフとして表示
    /// ロッカーなど、一日のメインスロットとして使用
    /// 午前・午後それぞれ最大80人
    /// 計算: 80人 ÷ 4時間 ÷ 2スロット/時間 = 1時間当たり20人（1スロット10人）
    /// </summary>
    Main = 1,

    /// <summary>
    /// Equipment: 設備（2）
    /// カレンダーに折れ線グラフとして表示
    /// 内視鏡、エコー、CT、MR、胃Baなど検査設備
    /// </summary>
    Equipment = 2,

    /// <summary>
    /// Environment: 環境（3）
    /// カレンダーに背景積み立てグラフとして表示
    /// 環境関連リソース
    /// </summary>
    Environment = 3,

    /// <summary>
    /// Others: その他（99）
    /// カレンダーには直接表示しないリソース
    /// </summary>
    Others = 99
}

