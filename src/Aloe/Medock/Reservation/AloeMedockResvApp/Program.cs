using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services.CacheServices;
using Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using CommandLine;
using System.Reflection;
using Microsoft.Extensions.Options;
using System.IO;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using System.IO.Pipes;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

/// <summary>
/// コマンドライン引数をマッピングします。
/// </summary>
public class Arguments
{
    /// <summary>
    /// スタンドアローンモードを有効にします。
    /// 通常のClient/Serverモードではなく、単独で実行できるようになります。
    /// 直接データベースへ接続するため接続文字列の設定が必要です。
    /// </summary>
    [Option("standalone", HelpText = "Enable standalone mode.")]
    public bool IsStandalone { get; set; }

    /// <summary>
    /// スタンドアローンモードのとき、SQLのログを出力できるようにします。
    /// </summary>
    [Option("sql", HelpText = "Enable SQL Logging in standalone mode.")]
    public bool IsStandaloneSqlLogging { get; set; }

    /// <summary>
    /// 例外が発生したときにTraceレベルでログ出力します。
    /// 内部で握りつぶしている場合や、例外を制御に使っている場合でも捕捉します。
    /// </summary>
    [Option("firstchance", HelpText = "Enable Exception Logging at FirstChanceExceptionEvent.")]
    public bool IsFirstChanceExceptionLogging { get; set; }

    [Option('u', "user", HelpText = "Login User")]
    public string User { get; set; } = "";

    [Option('p', "password", HelpText = "Login User")]
    public string Password { get; set; } = "";

    [Option("pt", HelpText = "Karte Number")]
    public string KarteNumber { get; set; } = "";

    [Option("screen", HelpText = "ScreenCode")]
    public ScreenCode ScreenCode { get; set; }

    /// <summary>
    /// コマンドライン引数からパースします。
    /// </summary>
    public static Arguments Parse(string[] args)
    {
        return Parser.Default.ParseArguments<Arguments>(args)
            .WithNotParsed(x =>
            {
                Console.WriteLine($"Arguments Parse Error: {args}");
            })
            .Value ?? new();
    }
}

internal static class Program
{
    /// <summary>
    /// プログラムのエントリポイントです。
    /// Program.Main → App と呼び出されます。
    /// </summary>
    [STAThread]
    internal static void Main(string[] args)
    {
        try
        {
            Timestamper.Global.Stamp("Main start.");

            var arguments = Arguments.Parse(args);

            // TODO: 引数なしで、すでに起動中だったらアクティブにしたい
            // KarteNumber, ScreenCode が指定されていた場合は、まったく同じやつだったらアクティブにする？
            //const string mutexName = @"Global\AloeMedockResvAppMutex";
            //using var mutex = new Mutex(false, mutexName, out var isCreatedNew);
            //if (!isCreatedNew)
            //{
            //    using var pipeClient = new NamedPipeClientStream(
            //        ".",
            //        mutexName,
            //        PipeDirection.Out,
            //        PipeOptions.Asynchronous);

            //    pipeClient.Connect();

            //    using var writer = new StreamWriter(pipeClient)
            //    {
            //        AutoFlush = true,
            //    };

            //    foreach (var arg in args)
            //    {
            //        writer.WriteLine(arg);
            //    }

            //    return;
            //}

            var app = new App(arguments);
            app.InitializeComponent();
            app.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unhandled exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }
    }
}
