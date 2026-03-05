namespace Aloe.Apps.MedockServer.ApplicationServices.Mobile;

/// <summary>
/// モバイルカレンダーの空き状況シンボルを表す列挙型
/// </summary>
public enum MobileSymbol { Full, Partial, Empty, NoData }

/// <summary>
/// モバイルカレンダーの空き状況シンボル決定ロジック
/// </summary>
public static class MobileSymbolHelper
{
    public static MobileSymbol GetSymbol(int capacity, int count)
    {
        if (capacity <= 0) return MobileSymbol.NoData;
        var vacancyRatio = (double)(capacity - count) / capacity;
        return vacancyRatio switch
        {
            >= 0.3 => MobileSymbol.Full,
            > 0    => MobileSymbol.Partial,
            _      => MobileSymbol.Empty
        };
    }

    public static string ToChar(MobileSymbol s) => s switch
    {
        MobileSymbol.Full    => "●",
        MobileSymbol.Partial => "▼",
        MobileSymbol.Empty   => "×",
        _                    => "-"
    };

    public static string ToColorClass(MobileSymbol s) => s switch
    {
        MobileSymbol.Full    => "text-emerald-500",
        MobileSymbol.Partial => "text-yellow-500",
        MobileSymbol.Empty   => "text-red-500",
        _                    => "text-gray-400"
    };
}
