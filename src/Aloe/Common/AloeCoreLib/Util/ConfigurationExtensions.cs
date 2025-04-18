using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aloe.Common.AloeCoreLib.Util;

// ReSharper disable ArrangeStaticMemberQualifier

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
        return settings ?? new T();
    }

    /// <summary>
    /// DIに設定用のクラスを登録します。
    /// </summary>
    public static IHostApplicationBuilder AddSettings<T>(this IHostApplicationBuilder builder)
        where T : class
    {
        builder.Services.Configure<T>(options => builder.Configuration.GetSection(typeof(T).Name)
            .Bind(options));

        return builder;
    }
}
