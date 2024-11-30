using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace AloeReservationGrid.Lib.CoreLib.Util;

public static class DumpExtensions
{
    /// <summary>
    /// バイト配列の内容を16進数で出力します。
    /// </summary>
    public static void DumpDebug(
        this ILogger logger,
        byte[] bytes
    )
    {
        if (logger == null! || !logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        if (bytes == null! || bytes.Length == 0)
        {
            return;
        }

        const string h1 = "00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F";
        const string h2 = "-- -- -- -- -- -- -- -- -- -- -- -- -- -- -- --";

        var str = new StringBuilder(h1.Length + h2.Length + bytes.Length * 4);

        str.AppendLine();
        str.AppendLine(h1);
        str.AppendLine(h2);

        for (var i = 0; i < bytes.Length; i += 16)
        {
            var lineBytes = bytes.Skip(i).Take(16).ToArray();
            var hexString = BitConverter.ToString(lineBytes).Replace("-", " ");
            str.AppendLine(hexString);
        }

        logger.LogDebug(str.ToString());
    }
}
