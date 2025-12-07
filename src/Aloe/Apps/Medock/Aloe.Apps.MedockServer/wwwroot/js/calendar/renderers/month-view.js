/**
 * Month View Renderer
 *
 * 月間カレンダー表示（1ヶ月全体を7列グリッドで表示）
 */

import { getState } from '../state.js';
import { CONFIG } from '../config.js';
import { getFirstDayOfMonth, getLastDayOfMonth, isToday } from '../utils/date-utils.js';
import { renderDayBarChart } from './bar-chart.js';

/**
 * 月間カレンダーを描画
 */
export function renderMonthView() {
    const state = getState();
    const { stage, layers, currentDate, dayStats } = state;
    const width = stage.width();
    const height = stage.height();

    layers.background.destroyChildren();
    layers.grid.destroyChildren();
    layers.content.destroyChildren();
    layers.interaction.destroyChildren();

    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();
    const firstDay = getFirstDayOfMonth(year, month);
    const lastDay = getLastDayOfMonth(year, month);
    const startDayOfWeek = firstDay.getDay();
    const daysInMonth = lastDay.getDate();

    const headerHeight = 40;
    const cellWidth = width / 7;
    const rows = Math.ceil((startDayOfWeek + daysInMonth) / 7);
    const cellHeight = (height - headerHeight) / (rows + 1);

    // Day of week headers
    const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
    for (let i = 0; i < 7; i++) {
        // Header background
        const headerBg = new Konva.Rect({
            x: i * cellWidth,
            y: 0,
            width: cellWidth,
            height: headerHeight,
            fill: '#f9fafb',
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });
        layers.grid.add(headerBg);

        const dayHeader = new Konva.Text({
            x: i * cellWidth,
            y: 12,
            width: cellWidth,
            text: dayNames[i],
            fontSize: CONFIG.font.sizeLarge,
            fontFamily: CONFIG.font.family,
            fontStyle: 'bold',
            fill: i === 0 ? CONFIG.colors.weekend.sun : i === 6 ? CONFIG.colors.weekend.sat : '#374151',
            align: 'center'
        });
        layers.grid.add(dayHeader);
    }

    // Day cells
    let day = 1;
    for (let row = 0; row < rows; row++) {
        for (let col = 0; col < 7; col++) {
            const cellIndex = row * 7 + col;
            if (cellIndex < startDayOfWeek || day > daysInMonth) {
                // Empty cell
                const emptyCell = new Konva.Rect({
                    x: col * cellWidth,
                    y: headerHeight + row * cellHeight,
                    width: cellWidth,
                    height: cellHeight,
                    fill: '#f3f4f6',
                    stroke: CONFIG.colors.grid,
                    strokeWidth: 1
                });
                layers.grid.add(emptyCell);
                continue;
            }

            const cellX = col * cellWidth;
            const cellY = headerHeight + row * cellHeight;
            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const isHoliday = state.holidays.has(dateStr);

            // セルサイズの安全な計算
            const safeCellWidth = Math.max(0, cellWidth - 2);
            const safeCellHeight = Math.max(0, cellHeight - 2);
            const cellCornerRadius = Math.max(0, Math.min(2, safeCellWidth / 2, safeCellHeight / 2));

            // Holiday background (light red, same as Sunday)
            if (isHoliday && !isToday(dateStr) && safeCellWidth > 0 && safeCellHeight > 0) {
                const holidayBg = new Konva.Rect({
                    x: cellX + 1,
                    y: cellY + 1,
                    width: safeCellWidth,
                    height: safeCellHeight,
                    fill: 'rgba(239, 68, 68, 0.12)',
                    cornerRadius: cellCornerRadius
                });
                layers.grid.add(holidayBg);
            }

            // Today's cell background (bright green rectangle)
            if (isToday(dateStr) && safeCellWidth > 0 && safeCellHeight > 0) {
                const todayBg = new Konva.Rect({
                    x: cellX + 1,
                    y: cellY + 1,
                    width: safeCellWidth,
                    height: safeCellHeight,
                    fill: 'rgba(16, 185, 129, 0.3)',
                    cornerRadius: cellCornerRadius
                });
                layers.grid.add(todayBg);
            }

            renderDayBarChart(cellX, cellY, cellWidth, cellHeight, dateStr, day, isHoliday);

            day++;
        }
    }

    layers.grid.batchDraw();
    layers.content.batchDraw();
    layers.interaction.batchDraw();
}
