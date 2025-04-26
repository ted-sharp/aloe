using System.Windows;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvApp.Configuration;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;

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

            ts.Stamp("Mutex Checking...");

            const string mutexName = "AloeMedockResvAppMutex";
            using var mutex = new Mutex(false, mutexName, out var isFirstInstance);

            if (!isFirstInstance)
            {
                // 既存インスタンスへ引数を通知して即終了
                NamedPipeService.Send(mutexName, args);
                return;
            }

            ts.Stamp("Mutex Checked.");

            ts.Stamp("Config Initializing...");

            // コマンドライン対応およびDI以前に使用するため先に読み込んでいます。
            var config = AloeClientConfig.CreateConfigurationRoot(args);

            ts.Stamp("Config Initialized.");

            var app = new App(config);

            ts.Stamp("NamedPipe Initializing...");

            // 最初のインスタンスだけパイプ受信サーバーを起動
            using var ipcService = new NamedPipeService();

            ipcService.ArgumentsReceived += app.IpcService_ArgumentsReceived;
            ipcService.Listen(mutexName);

            ts.Stamp("NamedPipe Initialized.");

            ts.DumpAsync();

            app.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unhandled exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }
    }

}
