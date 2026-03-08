// <copyright file="Sc.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace Aloe.Utils.Win32.ScCommand;

/// <summary>
/// Windowsサービスの作成・削除を行うユーティリティクラス
/// </summary>
[SupportedOSPlatform("windows")]
public static class Sc
{
    /// <summary>
    /// Windowsサービスを作成します
    /// </summary>
    /// <param name="serviceName">サービス名</param>
    /// <param name="serviceFullPath">サービス実行ファイルのフルパス</param>
    /// <param name="description">サービスの説明（オプション）</param>
    /// <param name="startType">サービスの開始タイプ（auto/demand/disabled）</param>
    /// <param name="account">サービスを実行するアカウント</param>
    /// <param name="dependencies">依存するサービスのリスト（オプション）</param>
    /// <param name="logger">ロガー（オプション）</param>
    /// <returns>サービスの作成に成功した場合はtrue、それ以外はfalse</returns>
    public static bool CreateService(
        string serviceName,
        string serviceFullPath,
        string? description = null,
        string startType = "auto",
        string account = "LocalSystem",
        string? dependencies = null,
        ILogger? logger = null)
    {
        // 前提チェック
        if (String.IsNullOrWhiteSpace(serviceFullPath) || !System.IO.File.Exists(serviceFullPath))
        {
            logger?.LogError("Service binary not found: {Path}", serviceFullPath);
            return false;
        }

        if (!Path.IsPathRooted(serviceFullPath))
        {
            logger?.LogError("Service path is not rooted: {Path}", serviceFullPath);
            return false;
        }

        // ① SCManager を開いて SafeHandle で管理
        using SafeSCManagerHandle scm = NativeMethods.OpenSCManager(
            machineName: null,
            databaseName: null,
            desiredAccess: NativeMethods.ScManagerCreateService);

        if (scm.IsInvalid)
        {
            logger?.LogWin32Error("OpenSCManager failed");
            return false;
        }

        // ② 開始タイプ文字列 → 数値
        uint dwStart = startType.ToLowerInvariant() switch
        {
            "auto" => 2,
            "demand" => 3,
            "disabled" => 4,
            _ => 2,
        };

        // ③ サービスの作成
        using SafeServiceHandle svc = NativeMethods.CreateService(
            hSCManager: scm,
            lpServiceName: serviceName,
            lpDisplayName: serviceName,
            dwDesiredAccess: NativeMethods.ServiceAllAccess,
            dwServiceType: NativeMethods.ServiceWin32OwnProcess,
            dwStartType: dwStart,
            dwErrorControl: 1, // SERVICE_ERROR_NORMAL
            lpBinaryPathName: serviceFullPath,
            lpLoadOrderGroup: null,
            lpdwTagId: IntPtr.Zero,
            lpDependencies: dependencies,
            lpServiceStartName: account,
            lpPassword: null);

        if (svc.IsInvalid)
        {
            logger?.LogWin32Error($"CreateService failed for '{serviceName}'");
            return false;
        }

        logger?.LogInformation("Service '{Name}' created.", serviceName);

        // ④ 説明設定（オプション）
        if (!String.IsNullOrWhiteSpace(description))
        {
            var desc = new NativeMethods.ServiceDescription { lpDescription = description! };
            if (NativeMethods.ChangeServiceConfig2(svc, NativeMethods.ServiceConfigDescription, ref desc))
            {
                logger?.LogInformation("Description set for '{Name}'.", serviceName);
            }
            else
            {
                logger?.LogWin32Error("ChangeServiceConfig2 failed");
            }
        }

        return true;
    }

    /// <summary>
    /// Windowsサービスを削除します
    /// </summary>
    /// <param name="serviceName">削除するサービス名</param>
    /// <param name="logger">ロガー（オプション）</param>
    /// <returns>サービスの削除に成功した場合はtrue、それ以外はfalse</returns>
    public static bool DeleteService(string serviceName, ILogger? logger = null)
    {
        using SafeSCManagerHandle scm = NativeMethods.OpenSCManager(
            machineName: null,
            databaseName: null,
            desiredAccess: NativeMethods.ScManagerCreateService);

        if (scm.IsInvalid)
        {
            logger?.LogWin32Error("OpenSCManager failed");
            return false;
        }

        using SafeServiceHandle svc = NativeMethods.OpenService(
            hSCManager: scm,
            lpServiceName: serviceName,
            dwDesiredAccess: NativeMethods.ServiceAllAccess);

        if (svc.IsInvalid)
        {
            logger?.LogWin32Error($"OpenService failed for '{serviceName}'");
            return false;
        }

        if (!NativeMethods.DeleteService(svc))
        {
            logger?.LogWin32Error($"DeleteService failed for '{serviceName}'");
            return false;
        }

        logger?.LogInformation("Service '{Name}' deleted.", serviceName);
        return true;
    }
}
