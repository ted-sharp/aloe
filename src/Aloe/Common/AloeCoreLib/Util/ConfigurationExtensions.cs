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
    // TODO: Microsoft.Extensions.Configuration.Json が必要なため、ここではないほうがよいかも？
    /// <summary>
    /// 設定ファイルを登録します。
    /// </summary>
    public static IConfigurationBuilder AddJsonFiles(this IConfigurationBuilder builder, IEnumerable<string> files)
    {
        var enumerable = files as string[] ?? files.ToArray();
        foreach (var file in enumerable)
        {
            builder.AddJsonFile(file, optional: true, reloadOnChange: true);
        }

        return builder;
    }

    /// <summary>
    /// DIに設定用のクラスを登録します。
    /// </summary>
    public static IHostApplicationBuilder BindSection<T>(this IHostApplicationBuilder builder)
        where T : class
    {
        builder.Services
            .Configure<T>(options => builder.Configuration.GetSection(typeof(T).Name)
            .Bind(options));

        return builder;
    }

    /// <summary>
    /// 指定された IConfiguration から設定値を取得し、<typeparamref name="T"/> 型のインスタンスを生成します。
    /// </summary>
    public static T BindSection<T>(this IConfiguration configuration)
        where T : class, new()
    {
        var instance = configuration
            .GetSection(typeof(T).Name)
            .Get<T>();
        return instance ?? new T();
    }
}
