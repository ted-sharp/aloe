using System.Diagnostics;
using System.Net.Http;
using System.Windows;
using Aloe.Common.AloeCoreLib.Ini;
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

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

/// <summary>
/// コマンドライン引数をマッピングします。
/// </summary>
public class Arguments
{
    [Option('s', "standalone", HelpText = "Enable standalone mode.")]
    public bool Standalone { get; set; }

    [Option('d', "development", HelpText = "Enable development mode.")]
    public bool IsDevelopment { get; set; }


    [Option('n', "null", HelpText = "Enable NullLogger mode.")]
    public bool IsNullLogger { get; set; }


    //[Option('d', "debug", HelpText = "Enable debug mode.")]
    //public bool Debug { get; set; }
}

internal static class Program
{
    /// <summary>
    /// プログラムのエントリポイントです。
    /// Program.Main → WpfHostService.StartAsync → App と呼び出されます。
    /// </summary>
    [STAThread]
    internal static void Main(string[] args)
    {
        try
        {
            Timestamper.Global.Stamp("Main start.");

            // TODO: グローバルに保持しておく
            var arguments = Parser.Default.ParseArguments<Arguments>(args).Value;

            if (arguments.IsNullLogger)
            {
                // コンソール出力を無効化
                Console.SetOut(TextWriter.Null);
            }

            // TODO: 名前付きMutexで複数起動制御する
            // 起動済みだったら名前付きパイプで引数を送る

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
