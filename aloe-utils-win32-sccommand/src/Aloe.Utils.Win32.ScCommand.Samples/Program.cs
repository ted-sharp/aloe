using Aloe.Utils.Win32.ScCommand;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.Principal;
using System.Threading;

namespace Aloe.Utils.Win32.ScCommand.Samples;

/// <summary>
/// サービス作成／削除のテストを行うサンプルプログラム
/// </summary>
[SupportedOSPlatform("windows")]
public static class Program
{
    public static void Main()
    {
        // 管理者権限チェック
        if (!IsAdministrator())
        {
            // app.manifest で指定しているので再起動を促されるはず
            Console.WriteLine("このプログラムは管理者権限で実行してください。");
            return;
        }

        // ロガー初期化
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger("Program");

        const string serviceName = "DummyService";
        var servicePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Aloe.Utils.Win32.ScCommand.DummyService.exe");

        try
        {
            // ① 既存サービスがあれば削除
            logger.LogInformation("サービス '{ServiceName}' の存在を確認し、削除を試みます...(なければ fail)", serviceName);
            Sc.DeleteService(serviceName, logger);

            // ② サービス作成
            logger.LogInformation("サービス '{ServiceName}' を作成します...", serviceName);
            bool created = Sc.CreateService(
                serviceName: serviceName,
                serviceFullPath: servicePath,
                description: "ダミーサービスのサンプル",
                startType: "auto",
                account: "LocalSystem",
                dependencies: null,
                logger: logger);

            if (!created)
            {
                logger.LogError("サービスの作成に失敗しました。処理を中止します。");
                return;
            }

            // ③ 少し待機して動作確認
            Thread.Sleep(TimeSpan.FromSeconds(2));

            // ④ 作成後の削除テスト
            logger.LogInformation("サービス '{ServiceName}' を削除します...", serviceName);
            bool deleted = Sc.DeleteService(serviceName, logger);
            if (!deleted)
            {
                logger.LogError("サービスの削除に失敗しました。");
            }
            else
            {
                logger.LogInformation("サービスの削除に成功しました。");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "テスト実行中に例外が発生しました。");
        }
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
