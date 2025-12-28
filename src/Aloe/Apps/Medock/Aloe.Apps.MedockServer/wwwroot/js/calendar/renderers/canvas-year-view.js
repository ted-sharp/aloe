/**
 * Canvas Year View Renderer
 * 
 * 年間カレンダー表示（Canvas API版）
 * 12ヶ月のミニカレンダーをレスポンシブグリッドで表示
 */

import { CONFIG } from '../config.js';
import { renderCanvasMonthCalendar } from './canvas-month-calendar.js';
import { renderCanvasDayBarChart } from './canvas-bar-chart.js';
import { renderCanvasLineChart } from './canvas-line-chart.js';
import { getRenderState, resetRenderState } from './canvas-render-state.js';

/**
 * コンテナ幅に基づいてグリッドレイアウトを決定
 * @param {number} width - コンテナ幅
 * @returns {{ cols: number, rows: number }}
 */
function getGridLayout(width) {
    if (width >= 1200) {
        return { cols: 4, rows: 3 };  // デスクトップ、ウルトラワイド
    } else if (width >= 768) {
        return { cols: 3, rows: 4 };  // タブレット、ラップトップ
    } else {
        return { cols: 2, rows: 6 };  // スマホ縦画面
    }
}

/**
 * 年間カレンダーを描画（Canvas API版）
 * @param {object} canvasManager - CanvasManagerインスタンス
 * @param {object} state - アプリケーション状態
 */
export function renderCanvasYearView(canvasManager, state) {
    const contexts = canvasManager.getAllContexts();
    const width = canvasManager.width;
    const height = canvasManager.height;

    // レイヤーをクリア
    canvasManager.clearAll();

    // Render Stateをリセット
    resetRenderState();
    const renderState = getRenderState();
    renderState.setViewType('year');

    const year = state.currentDate.getFullYear();

    // レスポンシブグリッドレイアウトを取得
    const { cols, rows } = getGridLayout(width);

    const monthWidth = width / cols;
    const monthHeight = height / rows;

    // パディングをコンテナサイズに応じて調整
    const padding = Math.max(4, Math.min(10, width * 0.01));

    for (let month = 0; month < 12; month++) {
        const col = month % cols;
        const row = Math.floor(month / cols);
        const x = col * monthWidth + padding;
        const y = row * monthHeight + padding;
        const w = monthWidth - padding * 2;
        const h = monthHeight - padding * 2;

        // 月カレンダーのグリッドとヘッダーを描画
        const monthInfo = renderCanvasMonthCalendar(contexts, state, year, month, x, y, w, h, {
            showMonthHeader: true,
            onMonthHeaderClick: (y, m) => {
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnMonthClicked', y, m + 1);
                }
            },
            dayHeaderStyle: 'small',
            gridStyle: 'minimal'
        });

        if (!monthInfo) {
            continue;
        }

        // 月情報をRenderStateに追加（階層的Hit Test用）
        renderState.addMonth({
            index: month,
            bounds: { x, y, width: w, height: h },
            monthHeaderBounds: monthInfo.monthHeaderBounds,
            onMonthHeaderClick: monthInfo.onMonthHeaderClick
        });

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
        const startHour = state.options.startHour || 8;
        const endHour = state.options.endHour || 18;

        // 各日付セルのバーチャートを描画
        let day = 1;
        for (let r = 0; r < rows; r++) {
            for (let c = 0; c < 7; c++) {
                const cellIndex = r * 7 + c;
                if (cellIndex < startDayOfWeek || day > daysInMonth) {
                    continue;
                }

                const cellLeft = x + c * cellWidth;
                const cellTop = dayGridTop + r * cellHeight;
                const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
                const isHoliday = state.holidays.has(dateStr);

                // バーチャートを描画
                renderCanvasDayBarChart(contexts, state, {
                    cellLeft,
                    cellTop,
                    cellWidth,
                    cellHeight,
                    dateStr,
                    dayNumber: day,
                    isHoliday
                });

                // Equipment折れ線グラフを描画（各セル内、時間軸に沿って）
                const equipmentStats = state.equipmentStats.get(dateStr);
                if (equipmentStats) {
                    // 日付テキストの高さを計算（バーチャートと同じ）
                    const dateFontSize = CONFIG.font.sizeDateYear;
                    const dayTextHeight = dateFontSize + 4;
                    const barAreaTop = cellTop + dayTextHeight;
                    const labelAreaHeight = (cellWidth >= 40 && cellHeight >= 50) ? 10 : 0;
                    const barAreaHeight = Math.max(0, cellHeight - dayTextHeight - 4 - labelAreaHeight);

                    renderCanvasLineChart(contexts.get('content'), {
                        cellLeft,
                        cellTop,
                        cellWidth,
                        cellHeight,
                        dateStr,
                        barAreaTop,
                        barAreaHeight,
                        equipmentStats,
                        startHour,
                        endHour,
                        lunchStartHour,
                        lunchEndHour,
                        isYearView: true
                    });
                }

                day++;
            }
        }
    }
}


