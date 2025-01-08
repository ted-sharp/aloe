using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using MagicOnion;
using Microsoft.Extensions.Hosting;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvLib.Logging;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CommandLine;
using System.Windows.Controls;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

// ここでは汎用的な設定を行うため、CA1859 は抑制します。
#pragma warning disable IDE0079 // 不要な抑制を削除する (IDE0079)
#pragma warning disable CA1859 // パフォーマンスの向上のために可能な場合は具象型を使用する

internal static class HostExtensions
{
    /// <summary>
    /// 構成の追加を行います。
    /// </summary>
    internal static HostApplicationBuilder ConfigureBuilder(this HostApplicationBuilder builder, Arguments arguments, RichTextBox? logTextBox)
    {
        builder
            .AddServices()
            .AddSerilog(logTextBox);

        if (arguments.IsStandalone)
        {
            builder.AddStandaloneService();
        }
        else
        {
            builder.AddMagicOnionClient();
        }

        return builder;
    }

    /// <summary>
    /// DIに必要なクラスを登録します。
    /// </summary>
    private static IHostApplicationBuilder AddServices(this IHostApplicationBuilder builder)
    {
        //builder.Services.AddHostedService<WpfHostService>();

        //builder.Services.Configure<GrpcConfig>(builder.Configuration.GetSection("Client:Targets:gRPC"));

        builder.Services.AddSingleton<Application, App>();
        builder.Services.AddSingleton<WindowService>();

        // Singleton で IMemoryCache が登録されます
        //builder.Services.AddMemoryCache();
        builder.Services.AddMemoryCache(options =>
        {
            // 有効期限のチェック間隔
            options.ExpirationScanFrequency = TimeSpan.FromSeconds(5);
        });
        builder.Services.AddTransient<ReservationEquipmentCacheService>();

        // ViewModel
        builder.Services.AddTransient<NotifyIconViewModel>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<InformationBarViewModel>();
        builder.Services.AddTransient<FunctionBarViewModel>();
        builder.Services.AddTransient<ReservationMainViewModel>();
        builder.Services.AddTransient<ReservationEquipViewModel>();

        // Window
        builder.Services.AddTransient<LoginWindow>();
        builder.Services.AddTransient<ReservationMainWindow>();
        builder.Services.AddTransient<ReservationEquipWindow>();
        builder.Services.AddTransient<ReservationEquipBookingWindow>();
        builder.Services.AddTransient<ReservationDailyWindow>();
        builder.Services.AddTransient<ReservationDailyBookingWindow>();
        //builder.Services.AddTransient<OrganizationWindow>();
        //builder.Services.AddTransient<PatientWindow>();
        builder.Services.AddTransient<OrganizationPatientSearchWindow>();
        builder.Services.AddTransient<MaintenanceWindow>();

        return builder;
    }

    /// <summary>
    /// gRPC と MagicOnion と関連クラスを追加します。
    /// </summary>
    private static IHostApplicationBuilder AddMagicOnionClient(this IHostApplicationBuilder builder)
    {
        // TODO: コマンドライン引数からも設定できるようにする

        // クライアントの gRPC ターゲット設定を読み取る
        //var grpcConfigSection = builder.Configuration.GetSection("Client:Targets:gRPC");
        //var grpcUrl = grpcConfigSection.GetValue<string>("Url");
        var grpcUrl = builder.Configuration.GetGrpcUrl();
        if (String.IsNullOrEmpty(grpcUrl))
        {
            throw new InvalidOperationException("gRPC URL is not configured.");
        }

        // GrpcChannel を登録
        builder.Services.AddSingleton(_ =>
        {
            var opt = new GrpcChannelOptions
            {
                HttpHandler = new HttpClientHandler(),
            };

            return GrpcChannel.ForAddress(grpcUrl, opt);
        });

        // MagicOnion クライアントを登録
        AddSingletonGrpcService<IAuthGrpcService>();
        AddSingletonGrpcService<IReservationEquipmentGrpcService>();

        return builder;

        // local function
        void AddSingletonGrpcService<T>()
            where T : class, IService<T>
        {
            builder.Services.AddSingleton<T>(services =>
            {
                // GrpcChannel を取得し MagicOnion クライアントを作成
                var channel = services.GetRequiredService<GrpcChannel>();
                return MagicOnionClient.Create<T>(channel);
            });
        }
    }

    /// <summary>
    /// 設定から gRPC 用のURLを取得します。
    /// </summary>
    public static string GetGrpcUrl(this IConfiguration config)
    {
        var grpcConfigSection = config.GetSection("Client:Targets:gRPC");

        var grpcUrl = grpcConfigSection.GetValue<string>("Url");
        if (String.IsNullOrEmpty(grpcUrl))
        {
            throw new InvalidOperationException("gRPC URL is not configured.");
        }

        return grpcUrl;
    }

    /// <summary>
    /// gRPC を介さずに直接アクセスするための関連クラスを追加します。
    /// 起動時の引数などにより <see cref="AddMagicOnionClient"/> と呼び分けてください。
    /// </summary>
    private static IHostApplicationBuilder AddStandaloneService(this IHostApplicationBuilder builder)
    {
        #region EFCore

        builder.Configuration.AddUserSecrets<App>();

        // DateTime は EFCore 6.0 以降は with timezone にマッピングされるので、それを without timezone にします。
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // TODO: コマンドライン引数からも設定できるようにする
        var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
        //builder.Services.AddDbContext<AppDbContext>(options =>
        //{
        //    options.UseNpgsql(connStr);

        //    if (builder.Environment.IsDevelopment())
        //    {
        //        options.EnableSensitiveDataLogging();
        //    }
        //});

        // EFCore はスレッドセーフではないので、ファクトリから都度生成します。
        builder.Services.AddDbContextFactory<AppDbContext>(options =>
        {
            options.UseNpgsql(connStr);
            if (builder.Environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
            }
        });

        #endregion EFCore

        #region DomainService

        builder.Services.AddScoped<IPolicyService, PolicyService>();
        builder.Services.AddScoped<IPreferenceService, PreferenceService>();

        #endregion DomainService

        #region MagicOnion(Direct)

        // GrpcChannel ではなく、直接サーバー側のサービスを使えるようにします。

        builder.Services.AddTransient<IAuthGrpcService, AuthGrpcService>();
        builder.Services.AddTransient<IReservationEquipmentGrpcService, ReservationEquipmentGrpcService>();

        #endregion MagicOnion(Direct)

        return builder;
    }
}
