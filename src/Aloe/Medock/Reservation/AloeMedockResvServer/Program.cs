
using CommandLine;
using Microsoft.Extensions.Hosting;

namespace Aloe.Medock.Reservation.AloeMedockResvServer;

/// <summary>
/// コマンドライン引数をマッピングします。
/// </summary>
internal class Arguments
{
    /// <summary>
    /// サンプルデータの挿入を試行します。
    /// 空の場合のみ挿入できます。
    /// </summary>
    [Option("seed", HelpText = "Try insert sample data.")]
    public bool IsSeed { get; set; }

    /// <summary>
    /// DB のログを出力します。
    /// </summary>
    [Option("sql", HelpText = "Enable DB Logging.")]
    public bool IsSqlLoggingEnabled { get; set; }

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

public static class Program
{
    public static async Task Main(string[] args)
    {
        var arguments = Arguments.Parse(args);

        try
        {
            if (arguments.IsSeed)
            {
                // サンプルデータを挿入する
                //var host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                var host = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args)
                    .ConfigureSeeder(arguments.IsSqlLoggingEnabled)
                    .Build();

                var seeder = host.Services.GetRequiredService<Seeder>();
                await seeder.InsertDataAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                // Kestrel で待ち受ける
                var host = Microsoft.AspNetCore.Builder.WebApplication.CreateSlimBuilder(args)
                    .ConfigureServer(arguments.IsSqlLoggingEnabled)
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

            if (arguments.IsSeed)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                _ = Console.ReadKey();
            }
        }

    }

}
