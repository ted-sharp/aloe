/**
 * Canvas Month View Renderer
 * 
 * 月間カレンダー表示（Canvas API版）
 * 1ヶ月全体を7列グリッドで表示
 */

import { clearCanvas } from '../utils/canvas-utils.js';
import { renderCanvasMonthCalendar } from './canvas-month-calendar.js';
import { renderCanvasDayBarChart } from './canvas-bar-chart.js';
import { getRenderState, resetRenderState } from './canvas-render-state.js';

/**
 * 月間カレンダーを描画（Canvas API版）
 * @param {object} canvasManager - CanvasManagerインスタンス
 * @param {object} state - アプリケーション状態
 */
export function renderCanvasMonthView(canvasManager, state) {
    const contexts = canvasManager.getAllContexts();
    const width = canvasManager.width;
    const height = canvasManager.height;

    // レイヤーをクリア
    canvasManager.clearAll();

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

    // 各日付セルのバーチャートを描画
    let day = 1;
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

            day++;
        }
    }
}


