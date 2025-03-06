using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Util;

public static class ConfigurationExtensions
{
    /// <summary>
    /// 指定された IConfiguration から設定値を取得し、<typeparamref name="T"/> 型のインスタンスを生成します。
    /// </summary>
    public static T GetSettings<T>(this IConfiguration configuration)
        where T : class, new()
    {
        var settings = configuration.GetSection(typeof(T).Name)
            .Get<T>();
        return settings ?? new();
    }
}
