using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Common.AloeCoreLib.Util;

/// <summary>
/// 簡易計測用クラスです。
/// </summary>
/// <remarks>
/// ベンチマークには BenchmarkDotNet を使用してください。
/// </remarks>
public class Timestamper
{
    private record struct TimestampPoint(long Timestamp, string Name, string Message);

    private static readonly Lazy<Timestamper> s_global = new(() => new Timestamper(nameof(Timestamper.Global)));

    /// <summary>
    /// 参照すると、Lazy で作成されます。
    /// </summary>
    public static Timestamper Global => Timestamper.s_global.Value;

    /// <summary>
    /// 計測ポイント名です。
    /// </summary>
    private readonly string _caption;

    /// <summary>
    /// 経過時間の計測ポイントです。
    /// </summary>
    private readonly List<TimestampPoint> _points = new(32);

    /// <summary>
    /// Ctor.
    /// </summary>
    public Timestamper(string caption)
    {
        this._caption = caption;

        this.Stamp("Start");
    }

    /// <summary>
    /// 時間を記録します。
    /// </summary>
    [Conditional("DEBUG")]
    [DebuggerHidden()]
    public void Stamp(string name, string message = "")
    {
        this._points.Add(new TimestampPoint(Stopwatch.GetTimestamp(), name, message));
    }

    /// <summary>
    /// Debug 出力に計測結果を印字します。
    /// </summary>
    [Conditional("DEBUG")]
    [DebuggerHidden()]
    public async void DumpAsync()
    {
        await Task.Run(() =>
        {
            var log = this.Build();
            Debug.WriteLine(log);
        });
    }

    /// <summary>
    /// ログに整形します。
    /// </summary>
    [DebuggerHidden()]
    public string Build()
    {
        var str = new StringBuilder();
        str.AppendLine();
        str.AppendLine($"===== {nameof(Timestamper)}: {this._caption}");
        str.AppendLine("0=========1=========2=========3=========4=========5=========6=========7=========");

        if (this._points.Count <= 1)
        {
            str.AppendLine("The timestamps are too short.");
            return str.ToString();
        }

        var nameMaxLen = this._points.Max(x => x.Name.Length);

        var first = this._points[0].Timestamp;
        var last = this._points[^1].Timestamp;
        var total = Stopwatch.GetElapsedTime(first, last);

        var elapsedMaxLen = $"{total.TotalMilliseconds:N1}".Length;

        var deltaMaxSpan = this._points
            .Zip(this._points.Skip(1),
                (prev, curr) => Stopwatch.GetElapsedTime(prev.Timestamp, curr.Timestamp))
            .MaxBy(x => x.TotalMilliseconds);

        var deltaMaxLen = $"{deltaMaxSpan.TotalMilliseconds:N3}".Length;


        for (var i = 1; i < this._points.Count; i++)
        {
            var point = this._points[i];

            // 名前を位置揃えで追加
            var name = point.Name.PadLeft(nameMaxLen);
            str.Append($"{name}: ");

            // 時間を位置揃えで追加
            var elapsed = Stopwatch.GetElapsedTime(first, point.Timestamp);
            var ms = $"{elapsed.TotalMilliseconds:N1}".PadLeft(elapsedMaxLen);
            str.Append($"{ms} ms ");

            var prev = this._points[i - 1].Timestamp;
            var delta = Stopwatch.GetElapsedTime(prev, point.Timestamp);
            var dt = $"{delta.TotalMilliseconds:N3}".PadLeft(deltaMaxLen);
            str.Append($"(dT {dt} ms)");

            // メッセージを末尾に追加
            if (!String.IsNullOrWhiteSpace(point.Message))
            {
                str.Append($" - {point.Message}");
            }
            str.AppendLine();
        }

        var ttlName = "Total seconds";
        ttlName = ttlName.PadLeft(nameMaxLen);
        str.Append($"{ttlName}: ");

        str.AppendLine($"{total.TotalSeconds,6} sec");
        str.AppendLine();

        return str.ToString();
    }
}
