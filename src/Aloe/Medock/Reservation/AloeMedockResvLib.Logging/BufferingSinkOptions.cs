using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Logging;

public class BufferingSinkOptions
{
    /// <summary>
    /// 一度に出力するバッチのサイズです。デフォルトは 1000.
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    ///定期的に出力する時間です。デフォルトは 2 sec.
    /// </summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// ログがひとつだけの場合は即出力します。
    /// </summary>
    public bool EagerlyEmitFirstEvent { get; set; } = true;
}
