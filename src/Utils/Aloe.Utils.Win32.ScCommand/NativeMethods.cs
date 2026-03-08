// <copyright file="NativeMethods.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Aloe.Utils.Win32.ScCommand;

/// <summary>
/// Windowsサービス管理のためのネイティブメソッドを提供するクラス
/// </summary>
[SupportedOSPlatform("windows")]
internal static class NativeMethods
{
    /// <summary>
    /// サービスコントロールマネージャーへの接続を開くためのアクセス権限
    /// </summary>
    public const uint ScManagerCreateService = 0x0002;

    /// <summary>
    /// サービスに対するすべてのアクセス権限
    /// </summary>
    public const uint ServiceAllAccess = 0xF01FF;

    /// <summary>
    /// Win32サービスが独自のプロセスで実行されることを示すフラグ
    /// </summary>
    public const uint ServiceWin32OwnProcess = 0x00000010;

    /// <summary>
    /// サービスの説明情報を設定するための情報レベル
    /// </summary>
    public const int ServiceConfigDescription = 1;

    /// <summary>
    /// サービスコントロールマネージャーへの接続を開きます
    /// </summary>
    /// <param name="machineName">接続先のコンピュータ名。nullの場合はローカルマシンに接続します。</param>
    /// <param name="databaseName">サービスデータベース名。nullの場合はデフォルトのデータベースに接続します。</param>
    /// <param name="desiredAccess">要求するアクセス権限。ScManagerCreateServiceなどの定数を使用します。</param>
    /// <returns>サービスコントロールマネージャーのハンドル。失敗した場合はnullを返します。</returns>
    /// <exception cref="Win32Exception">API呼び出しが失敗した場合にスローされます。</exception>
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern SafeSCManagerHandle OpenSCManager(
        string? machineName,
        string? databaseName,
        uint desiredAccess);

    /// <summary>
    /// 新しいWindowsサービスを作成します
    /// </summary>
    /// <param name="hSCManager">サービスコントロールマネージャーのハンドル。OpenSCManagerで取得したハンドルを指定します。</param>
    /// <param name="lpServiceName">サービス名。システム内で一意である必要があります。</param>
    /// <param name="lpDisplayName">表示名。サービス管理ツールに表示される名前です。</param>
    /// <param name="dwDesiredAccess">要求するアクセス権限。ServiceAllAccessなどの定数を使用します。</param>
    /// <param name="dwServiceType">サービスタイプ。ServiceWin32OwnProcessなどの定数を使用します。</param>
    /// <param name="dwStartType">起動タイプ。自動起動、手動起動などを指定します。</param>
    /// <param name="dwErrorControl">エラー制御。サービス起動失敗時の動作を指定します。</param>
    /// <param name="lpBinaryPathName">実行ファイルのパス。サービスの実行ファイルの完全パスを指定します。</param>
    /// <param name="lpLoadOrderGroup">ロード順序グループ。サービスの起動順序を制御するグループ名を指定します。</param>
    /// <param name="lpdwTagId">タグID。サービスグループ内での起動順序を指定します。</param>
    /// <param name="lpDependencies">依存関係。このサービスが依存する他のサービスの名前を指定します。</param>
    /// <param name="lpServiceStartName">サービス開始アカウント名。サービスを実行するアカウントを指定します。</param>
    /// <param name="lpPassword">パスワード。サービス開始アカウントのパスワードを指定します。</param>
    /// <returns>作成されたサービスのハンドル。失敗した場合はnullを返します。</returns>
    /// <exception cref="Win32Exception">API呼び出しが失敗した場合にスローされます。</exception>
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern SafeServiceHandle CreateService(
        SafeSCManagerHandle hSCManager,
        string lpServiceName,
        string lpDisplayName,
        uint dwDesiredAccess,
        uint dwServiceType,
        uint dwStartType,
        uint dwErrorControl,
        string lpBinaryPathName,
        string? lpLoadOrderGroup,
        IntPtr lpdwTagId,
        string? lpDependencies,
        string? lpServiceStartName,
        string? lpPassword);

    /// <summary>
    /// サービスの設定を変更します
    /// </summary>
    /// <param name="hService">サービスのハンドル。OpenServiceで取得したハンドルを指定します。</param>
    /// <param name="dwInfoLevel">情報レベル。ServiceConfigDescriptionなどの定数を使用します。</param>
    /// <param name="lpInfo">サービス説明情報。変更する設定情報を指定します。</param>
    /// <returns>成功した場合はtrue、失敗した場合はfalseを返します。</returns>
    /// <exception cref="Win32Exception">API呼び出しが失敗した場合にスローされます。</exception>
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool ChangeServiceConfig2(
        SafeServiceHandle hService,
        int dwInfoLevel,
        ref ServiceDescription lpInfo);

    /// <summary>
    /// サービスハンドルを閉じます
    /// </summary>
    /// <param name="hSCObject">閉じるハンドル。サービスまたはサービスコントロールマネージャーのハンドルを指定します。</param>
    /// <returns>成功した場合はtrue、失敗した場合はfalseを返します。</returns>
    /// <exception cref="Win32Exception">API呼び出しが失敗した場合にスローされます。</exception>
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool CloseServiceHandle(IntPtr hSCObject);

    /// <summary>
    /// 既存のサービスを開きます
    /// </summary>
    /// <param name="hSCManager">サービスコントロールマネージャーのハンドル。OpenSCManagerで取得したハンドルを指定します。</param>
    /// <param name="lpServiceName">サービス名。開くサービスの名前を指定します。</param>
    /// <param name="dwDesiredAccess">要求するアクセス権限。ServiceAllAccessなどの定数を使用します。</param>
    /// <returns>サービスのハンドル。失敗した場合はnullを返します。</returns>
    /// <exception cref="Win32Exception">API呼び出しが失敗した場合にスローされます。</exception>
    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern SafeServiceHandle OpenService(
        SafeSCManagerHandle hSCManager,
        string lpServiceName,
        uint dwDesiredAccess);

    /// <summary>
    /// サービスを削除します
    /// </summary>
    /// <param name="hService">削除するサービスのハンドル。OpenServiceで取得したハンドルを指定します。</param>
    /// <returns>成功した場合はtrue、失敗した場合はfalseを返します。</returns>
    /// <exception cref="Win32Exception">API呼び出しが失敗した場合にスローされます。</exception>
    [DllImport("advapi32.dll", SetLastError = true)]
    public static extern bool DeleteService(
        SafeServiceHandle hService);

    /// <summary>
    /// 最後のWin32エラーを取得して例外をスローします
    /// </summary>
    /// <param name="message">エラーメッセージ。例外に含めるメッセージを指定します。</param>
    /// <exception cref="Win32Exception">最後に発生したWin32エラーの情報を含む例外をスローします。</exception>
    [StackTraceHidden]
    [DoesNotReturn]
    public static void ThrowLastWin32(string message)
    {
        int err = Marshal.GetLastWin32Error();
        var ex = new Win32Exception(err, message);
        throw ex;
    }

    /// <summary>
    /// Windowsサービスの説明情報を保持する構造体
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ServiceDescription
    {
        /// <summary>
        /// サービスの説明文
        /// </summary>
        public string lpDescription;
    }
}
