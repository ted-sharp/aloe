
using CommandLine;

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

        var host = WebApplication.CreateSlimBuilder(args)
            .ConfigureBuilder()
            .ConfigureKestrel()
            .Build();

        try
        {
            if (arguments.IsSeed)
            {
                var seeder = host.Services.GetRequiredService<Seeder>();
                await seeder.InsertDataAsync();
                return;
            }

            await host.ConfigureApp()
                .RunAsync();
        }
        finally
        {
            await host.DisposeAsync();

            // Serilogはグローバルで保持しているので明示的に開放する
            await Serilog.Log.CloseAndFlushAsync();

            if (arguments.IsSeed)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                _ = Console.ReadKey();
            }
        }
    }

}
