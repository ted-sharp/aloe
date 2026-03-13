using System;
using System.Threading;
using Aloe.Apps.SyncBootBridgeLib.Helpers;
using Aloe.Apps.SyncBootBridgeLib.Repositories;
using Aloe.Apps.SyncBootBridgeLib.Services;

namespace Aloe.Apps.SyncBootBridge
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // ClickOnce URL引数を統合
                args = ClickOnceHelper.MergeArguments(
                    args,
                    ClickOnceHelper.GetClickOnceArguments());

                var options = CommandLineOptions.Parse(args);

                if (options.RandomDelaySeconds > 0)
                {
                    var random = new Random();
                    var delayMs = random.Next(0, options.RandomDelaySeconds * 1000);
                    Thread.Sleep(delayMs);
                }

                if (options.ShowConsole)
                {
                    ConsoleManager.EnsureConsoleVisible();
                    Console.WriteLine("[情報] 統合後の引数:");
                    foreach (var arg in args)
                    {
                        Console.WriteLine($"  {arg}");
                    }
                }

                Console.WriteLine("[情報] SyncBootBridge 開始");

                var manifestRepo = new IniManifestRepository();
                var fileSynchronizer = new FileSynchronizer();
                var zipExtractor = new ZipExtractor();
                var syncOrchestrator = new SyncOrchestrator(fileSynchronizer, zipExtractor);
                var appSelector = new AppSelector();
                var appLauncher = new AppLauncher();

                var bootstrapper = new SyncBootBridgeBootstrapper(
                    manifestRepo,
                    syncOrchestrator,
                    appSelector,
                    appLauncher
                );

                if (options.SyncOnly)
                {
                    bootstrapper.ExecuteSyncOnly(args);
                }
                else
                {
                    bootstrapper.Execute(args);
                }

                Console.WriteLine("[情報] SyncBootBridge 終了");
            }
            catch (Exception ex)
            {
                ConsoleManager.EnsureConsoleVisible();
                Console.WriteLine($"[エラー] {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }
    }
}
