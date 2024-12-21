
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
    /// ログを抑制します。
    /// コンソール出力とロガーが無効化されます。
    /// </summary>
    /// <remarks>
    /// 計測時に余計なログを出したくないときに使います。
    /// </remarks>
    [Option('q', "quiet", HelpText = "Enable quiet/silent mode.")]
    public bool IsSilent { get; set; }
}

public static class Program
{
    private static Arguments s_arguments = null!;

    public static async Task Main(string[] args)
    {
        s_arguments = Parser.Default.ParseArguments<Arguments>(args)
            .WithNotParsed(x =>
            {
                Console.WriteLine($"Arguments Parse Error: {args}");
            })
            .Value ?? new();

        if (s_arguments.IsSilent)
        {
            // コンソール出力を無効化
            Console.SetOut(TextWriter.Null);
        }

        var host = WebApplication.CreateSlimBuilder(args)
            .ConfigureBuilder()
            .ConfigureKestrel()
            .Build();

        try
        {
            if (s_arguments.IsSeed)
            {
                var seeder = host.Services.GetRequiredService<Seeder>();
                await seeder.InsertDataAsync();
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                _ = Console.ReadKey();
                return;
            }

            host.ConfigureApp()
                .Run();
        }
        finally
        {
            await host.DisposeAsync();
        }
    }

}
