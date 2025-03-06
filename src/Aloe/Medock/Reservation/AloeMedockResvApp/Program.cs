using System.Windows;
using Aloe.Common.AloeCoreLib.Util;
using Microsoft.Extensions.Configuration;

using Aloe.Medock.Reservation.AloeMedockResvApp.Settings;

namespace Aloe.Medock.Reservation.AloeMedockResvApp;

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
            var ts = new Timestamper("Main");

            ts.Stamp("Config Initializing...");

            var config = AloeClientSettings.CreateConfiguration(args);
            var settings = config.GetSettings<AloeClientSettings>();

            ts.Stamp("Config Initialized.");

            ts.DumpAsync();

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

            var app = new App(config, settings);
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
