/**
 * Canvas Month View Renderer
 * 
 * 月間カレンダー表示（Canvas API版）
 * 1ヶ月全体を7列グリッドで表示
 */

import { CONFIG } from '../config.js';
import { clearCanvas } from '../utils/canvas-utils.js';
import { renderCanvasMonthCalendar } from './canvas-month-calendar.js';
import { renderCanvasDayBarChart } from './canvas-bar-chart.js';
import { renderCanvasLineChart } from './canvas-line-chart.js';
import { getRenderState, resetRenderState } from './canvas-render-state.js';

/**
 * 月間カレンダーを描画（Canvas API版）
 * @param {object} canvasManager - CanvasManagerインスタンス
 * @param {object} state - アプリケーション状態
 * @param {string} fadeMode - フェードモード: 'instant', 'crossfade', 'fadethrough', 'sharedelement'
 * @param {number} fadeDuration - フェード時間（ミリ秒）
 * @param {object} transitionInfo - トランジション情報 { sourceBounds, targetDateStr }
 */
export function renderCanvasMonthView(canvasManager, state, fadeMode = 'crossfade', fadeDuration = 200, transitionInfo = null) {
    // オフスクリーンバッファに描画（ダブルバッファリング）
    const contexts = canvasManager.getAllOffscreenContexts();
    const width = canvasManager.width;
    const height = canvasManager.height;

    // オフスクリーンレイヤーをクリア
    canvasManager.clearAllOffscreen();

    // Render Stateをリセット
    resetRenderState();
    const renderState = getRenderState();
    renderState.setViewType('month');

    const year = state.currentDate.getFullYear();
    const month = state.currentDate.getMonth();

    // 月カレンダーのグリッドとヘッダーを描画
    const monthInfo = renderCanvasMonthCalendar(contexts, state, year, month, 0, 0, width, height, {
        showMonthHeader: false,
        headerHeight: 40,
        dayHeaderStyle: 'large',
        showEmptyCells: true,
        gridStyle: 'full'
    });

    if (!monthInfo) {
        return;
    }

    const { dayGridTop, dayGridHeight, cellWidth, cellHeight, rows, startDayOfWeek, daysInMonth } = monthInfo;

    // 営業時間情報を取得
    const businessHours = state.options?.businessHours;
    let lunchStartHour = null;
    let lunchEndHour = null;
    if (businessHours && businessHours.lunchStartTime && businessHours.lunchEndTime) {
        const parseTime = (timeStr) => {
            const parts = timeStr.split(':');
            return parseInt(parts[0], 10) + (parseInt(parts[1] || 0, 10) / 60);
        };
        lunchStartHour = parseTime(businessHours.lunchStartTime);
        lunchEndHour = parseTime(businessHours.lunchEndTime);
    }
    const startHour = state.options.startHour || 9;
    const endHour = state.options.endHour || 17;

    // 週行の bounds 情報を保存する配列（月↔週トランジション用）
    const weekRowBoundsMap = new Map(); // key: rowIndex, value: { rowIndex, dates: [], bounds }

    // 各日付セルのバーチャートを描画
    let day = 1;
    let barChartCallCount = 0;
    for (let row = 0; row < rows; row++) {
        for (let col = 0; col < 7; col++) {
            const cellIndex = row * 7 + col;
            if (cellIndex < startDayOfWeek || day > daysInMonth) {
                continue;
            }

            const cellLeft = col * cellWidth;
            const cellTop = dayGridTop + row * cellHeight;
            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const isHoliday = state.holidays.has(dateStr);

            // 週行の bounds 情報を記録
            if (!weekRowBoundsMap.has(row)) {
                weekRowBoundsMap.set(row, {
                    rowIndex: row,
                    dates: [],
                    bounds: { x: 0, y: cellTop, width, height: cellHeight }
                });
            }
            weekRowBoundsMap.get(row).dates.push(dateStr);

            // バーチャートを描画
            if (barChartCallCount === 0) {
            }
            renderCanvasDayBarChart(contexts, state, {
                cellLeft,
                cellTop,
                cellWidth,
                cellHeight,
                dateStr,
                dayNumber: day,
                isHoliday
            });
            barChartCallCount++;

            // Equipment折れ線グラフを描画（各セル内、時間軸に沿って）
            const equipmentStats = state.equipmentStats.get(dateStr);
            if (equipmentStats) {
                // Mainリソース統計データを取得
                const mainStats = state.mainStats.get(dateStr);
                
                // 日付テキストの高さを計算（バーチャートと同じ）
                const dateFontSize = CONFIG.font.sizeDateMonth;
                const dayTextHeight = dateFontSize + CONFIG.spacing.dayTextMargin;
                const barAreaTop = cellTop + dayTextHeight;
                const labelAreaHeight = (cellWidth >= 40 && cellHeight >= 50) ? 12 : 0;
                const barAreaHeight = Math.max(0, cellHeight - dayTextHeight - CONFIG.spacing.dayTextMargin - labelAreaHeight);

                renderCanvasLineChart(contexts.get('content'), {
                    cellLeft,
                    cellTop,
                    cellWidth,
                    cellHeight,
                    dateStr,
                    barAreaTop,
                    barAreaHeight,
                    equipmentStats,
                    mainStats,
                    startHour,
                    endHour,
                    lunchStartHour,
                    lunchEndHour,
                    isYearView: false
                });
            }

            day++;
        }
    }

    // 週行の bounds 情報を RenderState に保存
    weekRowBoundsMap.forEach((weekRowInfo) => {
        const dates = weekRowInfo.dates;
        if (dates.length > 0) {
            renderState.addWeekRow({
                rowIndex: weekRowInfo.rowIndex,
                weekStartDate: dates[0],
                weekEndDate: dates[dates.length - 1],
                bounds: weekRowInfo.bounds
            });
        }
    });

    // すべての描画が完了したら、オフスクリーンバッファをメインCanvasに一括転送
    let commitOptions = {};

    if (fadeMode === 'sharedelement' && transitionInfo) {
        // 年 → 月: 年間ビューの月bounds → 月間ビュー全体
        if (transitionInfo.transitionType === 'year-to-month') {
            // 遷移元: calendar-main.jsで保存された年間ビューの月bounds
            // 遷移先: 月間ビュー全体（canvas全体）
            if (transitionInfo.sourceBounds) {
                commitOptions = {
                    sourceBounds: transitionInfo.sourceBounds,
                    targetBounds: { x: 0, y: 0, width, height },
                    transitionType: transitionInfo.transitionType
                };
                console.log('Month View: Year-to-Month transition', commitOptions);
            } else {
                console.warn('Month View: sourceBounds not found in transitionInfo, falling back to scalefade');
                fadeMode = 'scalefade';
            }
        }
        // 週 → 月: 週ビュー全体 → 月間ビューの対応する週行
        else if (transitionInfo.transitionType === 'week-to-month') {
            // 遷移元: 週ビュー全体
            // 遷移先: 月間ビューの対応する週行
            if (transitionInfo.sourceBounds && transitionInfo.targetWeekRowIndex !== undefined) {
                const targetWeekRow = renderState.weekRows.find(wr => wr.rowIndex === transitionInfo.targetWeekRowIndex);
                if (targetWeekRow) {
                    commitOptions = {
                        sourceBounds: transitionInfo.sourceBounds,
                        targetBounds: targetWeekRow.bounds,
                        transitionType: transitionInfo.transitionType
                    };
                    console.log('Month View: Week-to-Month transition', commitOptions);
                } else {
                    console.warn('Month View: targetWeekRow not found, falling back to scalefade');
                    fadeMode = 'scalefade';
                }
            } else {
                console.warn('Month View: sourceBounds or targetWeekRowIndex not found in transitionInfo, falling back to scalefade');
                fadeMode = 'scalefade';
            }
        }
        // 月 → 年: この遷移は年ビューで処理されるため、ここには来ない
        else if (transitionInfo.transitionType === 'month-to-year') {
            console.warn('Month View: Unexpected month-to-year transition (should be handled by year view)');
            fadeMode = 'scalefade';
        }
    }

    canvasManager.commitAll(fadeMode, fadeDuration, commitOptions);
}


