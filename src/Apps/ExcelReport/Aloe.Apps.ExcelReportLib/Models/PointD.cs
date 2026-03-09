// <copyright file="PointD.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

namespace Aloe.Apps.ExcelReportLib.Models;

/// <summary>
/// ポイント(pt)単位の2D座標を表す値型。
/// </summary>
/// <param name="X">X座標(pt)。</param>
/// <param name="Y">Y座標(pt)。</param>
public readonly record struct PointD(double X, double Y);

/// <summary>
/// ポイント(pt)単位の矩形を表す値型。
/// </summary>
/// <param name="X">左上X座標(pt)。</param>
/// <param name="Y">左上Y座標(pt)。</param>
/// <param name="Width">幅(pt)。</param>
/// <param name="Height">高さ(pt)。</param>
public readonly record struct RectD(double X, double Y, double Width, double Height);
