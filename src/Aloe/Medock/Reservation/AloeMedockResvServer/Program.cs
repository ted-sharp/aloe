
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvServer.Settings;
using CommandLine;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.Tracing;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var isSeed = false;
        try
        {
            // サービスで起動した場合に exe の位置に変更する必要がある
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            var config = AloeServerSettings.CreateConfiguration(args);
            var settings = config.GetSettings<AloeServerSettings>();
            isSeed = settings.IsSeed;

            if (isSeed)
            {
                // サンプルデータを挿入する
                //var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                var host = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args)
                    .ConfigureSeeder(settings.IsSqlLogging, settings.ConnectionStringName)
                    .Build();

                var seeder = host.Services.GetRequiredService<Seeder>();
                await seeder.InsertDataAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                // Kestrel で待ち受ける
                var host = Microsoft.AspNetCore.Builder.WebApplication.CreateSlimBuilder(args)
                    .ConfigureServer(settings.IsSqlLogging, settings.ConnectionStringName)
                    .ConfigureKestrel()
                    .Build();

                await host.ConfigureServerWebApp()
                    .RunAsync()
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            // Serilogはグローバルで保持しているので明示的に開放する
            await Serilog.Log.CloseAndFlushAsync()
                .ConfigureAwait(false);

            if (isSeed)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                _ = Console.ReadKey();
            }
        }

    }

}
