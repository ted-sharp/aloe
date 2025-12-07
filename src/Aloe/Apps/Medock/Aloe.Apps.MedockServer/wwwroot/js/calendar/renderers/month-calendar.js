/**
 * Month Calendar Renderer
 *
 * 月のカレンダー描画を共通化した関数
 * 年間カレンダーと月間カレンダーの両方で使用
 */

import { getState } from '../state.js';
import { CONFIG } from '../config.js';
import { getFirstDayOfMonth, getLastDayOfMonth, isToday } from '../utils/date-utils.js';
import { renderDayBarChart } from './bar-chart.js';

/**
 * 月のカレンダーを描画
 */
export function renderMonthCalendar(year, month, x, y, width, height, options = {}) {
    const state = getState();
    const { layers } = state;

    const opts = {
        showMonthHeader: false,
        onMonthHeaderClick: null,
        headerHeight: null,
        dayHeaderStyle: 'small',
        showEmptyCells: false,
        gridStyle: 'minimal',
        showWeekendBackground: true,
        ...options
    };

    if (width < 20 || height < 20) {
        return;
    }

    const monthNames = ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'];

    let monthHeaderOffset = 0;
    if (opts.showMonthHeader) {
        const headerHeight = Math.min(22, height / 4);
        const headerCornerRadius = Math.min(4, width / 2, headerHeight / 2);

        const headerBg = new Konva.Rect({
            x: x,
            y: y - 2,
            width: Math.max(0, width),
            height: headerHeight,
            fill: 'transparent',
            cornerRadius: Math.max(0, headerCornerRadius)
        });
        layers.grid.add(headerBg);

        const header = new Konva.Text({
            x: x,
            y: y,
            width: width,
            text: monthNames[month],
            fontSize: CONFIG.font.sizeLarge,
            fontFamily: CONFIG.font.family,
            fontStyle: 'bold',
            fill: '#374151',
            align: 'center'
        });
        layers.grid.add(header);

        if (opts.onMonthHeaderClick) {
            const headerHitArea = new Konva.Rect({
                x: x,
                y: y - 2,
                width: width,
                height: 22,
                fill: 'transparent'
            });
            headerHitArea.on('mouseenter', function () {
                headerBg.fill('rgba(59, 130, 246, 0.1)');
                document.body.style.cursor = 'pointer';
                layers.grid.batchDraw();
            });
            headerHitArea.on('mouseleave', function () {
                headerBg.fill('transparent');
                document.body.style.cursor = 'default';
                layers.grid.batchDraw();
            });
            headerHitArea.on('click', function () {
                opts.onMonthHeaderClick(year, month);
            });
            layers.interaction.add(headerHitArea);
        }

        monthHeaderOffset = 25;
    }

    const firstDay = getFirstDayOfMonth(year, month);
    const lastDay = getLastDayOfMonth(year, month);
    const startDayOfWeek = firstDay.getDay();
    const daysInMonth = lastDay.getDate();
    const rows = Math.ceil((startDayOfWeek + daysInMonth) / 7);

    const gridTop = y + monthHeaderOffset;
    let gridHeight;
    let headerHeight;
    
    if (opts.headerHeight !== null) {
        headerHeight = opts.headerHeight;
        gridHeight = height - monthHeaderOffset - headerHeight;
    } else {
        gridHeight = height - monthHeaderOffset - 5;
        headerHeight = gridHeight / (rows + 1);
    }

    const cellWidth = width / 7;
    const cellHeight = gridHeight / (rows + 1);

    const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
    const dayHeaderY = opts.dayHeaderStyle === 'large' ? gridTop + 12 : gridTop;
    const dayHeaderFontSize = opts.dayHeaderStyle === 'large' ? CONFIG.font.sizeLarge : CONFIG.font.sizeSmall;
    const dayHeaderHeight = opts.dayHeaderStyle === 'large' ? headerHeight : cellHeight;

    for (let i = 0; i < 7; i++) {
        if (opts.dayHeaderStyle === 'large') {
            const headerBg = new Konva.Rect({
                x: x + i * cellWidth,
                y: gridTop,
                width: cellWidth,
                height: headerHeight,
                fill: '#f9fafb',
                stroke: CONFIG.colors.grid,
                strokeWidth: 1
            });
            layers.grid.add(headerBg);
        }

        const dayHeader = new Konva.Text({
            x: x + i * cellWidth,
            y: dayHeaderY,
            width: cellWidth,
            text: dayNames[i],
            fontSize: dayHeaderFontSize,
            fontFamily: CONFIG.font.family,
            fontStyle: opts.dayHeaderStyle === 'large' ? 'bold' : 'normal',
            fill: i === 0 ? CONFIG.colors.weekend.sun : i === 6 ? CONFIG.colors.weekend.sat : (opts.dayHeaderStyle === 'large' ? '#374151' : '#6b7280'),
            align: 'center'
        });
        layers.grid.add(dayHeader);
    }

    function snapToPixel(val) {
        return Math.floor(val) + 0.5;
    }

    const dayGridTop = gridTop + dayHeaderHeight;
    const dayGridHeight = gridHeight - dayHeaderHeight;

    // 空セルの描画（月間表示用）
    if (opts.showEmptyCells && opts.gridStyle === 'full') {
        let day = 1;
        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < 7; col++) {
                const cellIndex = row * 7 + col;
                const cellLeft = x + col * cellWidth;
                const cellTop = dayGridTop + row * cellHeight;
                
                if (cellIndex < startDayOfWeek || day > daysInMonth) {
                    const emptyCell = new Konva.Rect({
                        x: cellLeft,
                        y: cellTop,
                        width: cellWidth,
                        height: cellHeight,
                        fill: '#f3f4f6',
                        stroke: CONFIG.colors.grid,
                        strokeWidth: 1
                    });
                    layers.grid.add(emptyCell);
                } else {
                    const cellRect = new Konva.Rect({
                        x: cellLeft,
                        y: cellTop,
                        width: cellWidth,
                        height: cellHeight,
                        fill: 'transparent',
                        stroke: CONFIG.colors.grid,
                        strokeWidth: 1
                    });
                    layers.grid.add(cellRect);
                }
                if (cellIndex >= startDayOfWeek && day <= daysInMonth) {
                    day++;
                }
            }
        }
    }

    // グリッド線の描画（年間表示用）
    if (opts.gridStyle === 'minimal') {
        for (let row = 0; row < rows; row++) {
            for (let col = 0; col < 7; col++) {
                const cellLeft = x + col * cellWidth;
                const cellTop = dayGridTop + row * cellHeight;

                if (opts.showWeekendBackground && row < rows) {
                    let bgColor = null;
                    if (col === 0) bgColor = 'rgba(239, 68, 68, 0.08)';
                    else if (col === 6) bgColor = 'rgba(59, 130, 246, 0.08)';

                    if (bgColor) {
                        const weekendBg = new Konva.Rect({
                            x: cellLeft,
                            y: cellTop,
                            width: cellWidth,
                            height: cellHeight,
                            fill: bgColor
                        });
                        layers.background.add(weekendBg);
                    }
                }

                if (col > 0) {
                    const vLine = new Konva.Line({
                        points: [snapToPixel(cellLeft), snapToPixel(dayGridTop), snapToPixel(cellLeft), snapToPixel(dayGridTop + rows * cellHeight)],
                        stroke: '#e5e7eb',
                        strokeWidth: 1
                    });
                    layers.grid.add(vLine);
                }

                if (row > 0) {
                    const hLine = new Konva.Line({
                        points: [snapToPixel(x), snapToPixel(dayGridTop + row * cellHeight), snapToPixel(x + width), snapToPixel(dayGridTop + row * cellHeight)],
                        stroke: '#e5e7eb',
                        strokeWidth: 1
                    });
                    layers.grid.add(hLine);
                }
            }
        }

        if (rows > 0) {
            const headerBottomLine = new Konva.Line({
                points: [snapToPixel(x), snapToPixel(dayGridTop), snapToPixel(x + width), snapToPixel(dayGridTop)],
                stroke: '#e5e7eb',
                strokeWidth: 1
            });
            layers.grid.add(headerBottomLine);

            // 最終行の下側のボーダー
            const bottomLine = new Konva.Line({
                points: [snapToPixel(x), snapToPixel(dayGridTop + rows * cellHeight), snapToPixel(x + width), snapToPixel(dayGridTop + rows * cellHeight)],
                stroke: '#e5e7eb',
                strokeWidth: 1
            });
            layers.grid.add(bottomLine);
        }
    }

    // 日付セルとバーチャートの描画
    let day = 1;
    for (let row = 0; row < rows; row++) {
        for (let col = 0; col < 7; col++) {
            const cellIndex = row * 7 + col;
            if (cellIndex < startDayOfWeek || day > daysInMonth) {
                continue;
            }

            const cellLeft = x + col * cellWidth;
            const cellTop = dayGridTop + row * cellHeight;
            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const isHoliday = state.holidays.has(dateStr);

            const safeCellWidth = Math.max(0, cellWidth - 2);
            const safeCellHeight = Math.max(0, cellHeight - 2);
            const cellCornerRadius = Math.max(0, Math.min(2, safeCellWidth / 2, safeCellHeight / 2));

            if (isHoliday && !isToday(dateStr) && safeCellWidth > 0 && safeCellHeight > 0) {
                const holidayBg = new Konva.Rect({
                    x: cellLeft + 1,
                    y: cellTop + 1,
                    width: safeCellWidth,
                    height: safeCellHeight,
                    fill: 'rgba(239, 68, 68, 0.12)',
                    cornerRadius: cellCornerRadius
                });
                layers.grid.add(holidayBg);
            }

            if (isToday(dateStr) && safeCellWidth > 0 && safeCellHeight > 0) {
                const todayBg = new Konva.Rect({
                    x: cellLeft + 1,
                    y: cellTop + 1,
                    width: safeCellWidth,
                    height: safeCellHeight,
                    fill: 'rgba(16, 185, 129, 0.3)',
                    cornerRadius: cellCornerRadius
                });
                layers.grid.add(todayBg);
            }

            renderDayBarChart(cellLeft, cellTop, cellWidth, cellHeight, dateStr, day, isHoliday);

            day++;
        }
    }
}
