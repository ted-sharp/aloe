using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

// Win32 向けAPIを使用しているため除外
#pragma warning disable CA1416

namespace Aloe.Common.AloeCoreLib.Win32;

public static class ElevationHelper
{
    /// <summary>
    /// 管理者として実行されていない場合、再起動して昇格する。
    /// </summary>
    /// <returns>昇格が必要だった場合は false（再起動後に続行しない）</returns>
    public static bool EnsureRunAsAdministrator()
    {
        if (ElevationHelper.IsAdmin())
        {
            // すでに管理者権限の場合は昇格の必要なし
            return true;
        }

        try
        {
            // 0番目は実行ファイル名なのでスキップ
            var args = Environment.GetCommandLineArgs()
                .Skip(1)
                .ToArray();

            // ダブルクォートなどはなくなった状態で来るので再び付与する
            var newArgs = String.Join(" ", args.Select(arg => $"\"{arg}\""));

            var psi = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath,
                Arguments = newArgs,
                UseShellExecute = true,
                Verb = "runas",
            };

            Process.Start(psi);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            Console.WriteLine("ユーザーが管理者昇格をキャンセルしました。");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return false;
    }

    /// <summary>
    /// 現在のプロセスが管理者として実行されているかどうかを判定。
    /// </summary>
    private static bool IsAdmin()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}

