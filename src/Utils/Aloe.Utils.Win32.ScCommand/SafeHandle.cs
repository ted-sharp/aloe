// <copyright file="SafeHandle.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1649 // File name should match first type name
namespace Aloe.Utils.Win32.ScCommand;

/// <summary>
/// SCManager (サービスコントローラーマネージャ) 用 SafeHandle
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class SafeSCManagerHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SafeSCManagerHandle"/> class.
    /// </summary>
    private SafeSCManagerHandle() : base(true) { }

    /// <summary>
    /// ハンドルを解放します。
    /// </summary>
    /// <returns>ハンドルの解放が成功した場合はtrue、それ以外の場合はfalse。</returns>
    protected override bool ReleaseHandle()
        => NativeMethods.CloseServiceHandle(this.handle);
}

/// <summary>
/// Service (個別サービス) 用 SafeHandle
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SafeServiceHandle"/> class.
    /// </summary>
    private SafeServiceHandle() : base(true) { }

    /// <summary>
    /// ハンドルを解放します。
    /// </summary>
    /// <returns>ハンドルの解放が成功した場合はtrue、それ以外の場合はfalse。</returns>
    protected override bool ReleaseHandle()
        => NativeMethods.CloseServiceHandle(this.handle);
}
