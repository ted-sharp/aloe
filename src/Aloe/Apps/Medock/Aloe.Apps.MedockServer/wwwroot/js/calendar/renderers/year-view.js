/**
 * Year View Renderer
 *
 * 年間カレンダー表示（12ヶ月のミニカレンダーをレスポンシブグリッドで表示）
 * - >= 1200px: 4列 x 3行（デスクトップ）
 * - 768px - 1199px: 3列 x 4行（タブレット）
 * - < 768px: 2列 x 6行（スマホ縦）
 */

import { getState } from '../state.js';
import { CONFIG } from '../config.js';
import { getFirstDayOfMonth, getLastDayOfMonth, isToday } from '../utils/date-utils.js';
import { renderDayBarChart } from './bar-chart.js';

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
 * 年間カレンダーを描画
 */
export function renderYearView() {
    const state = getState();
    const { stage, layers, currentDate, dayStats } = state;
    const width = stage.width();
    const height = stage.height();

    // Clear layers
    layers.background.destroyChildren();
    layers.grid.destroyChildren();
    layers.content.destroyChildren();
    layers.interaction.destroyChildren();

    const year = currentDate.getFullYear();

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

        renderMonthMiniCalendar(month, x, y, w, h, year);
    }

    layers.grid.batchDraw();
    layers.content.batchDraw();
    layers.interaction.batchDraw();
}

/**
 * 個別の月ミニカレンダーを描画
 * @param {number} month - 月（0-11）
 * @param {number} x - 左上X座標
 * @param {number} y - 左上Y座標
 * @param {number} width - 幅
 * @param {number} height - 高さ
 * @param {number} year - 年
 */
function renderMonthMiniCalendar(month, x, y, width, height, year) {
    const state = getState();
    const { layers, dayStats } = state;
    const monthNames = ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'];

    // サイズが小さすぎる場合は描画をスキップ
    if (width < 20 || height < 20) {
        return;
    }

    // Month header (クリック可能)
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

    // 月ヘッダーのインタラクション
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
        // 月クリック → 月間表示に切り替え
        if (state.dotNetRef) {
            state.dotNetRef.invokeMethodAsync('OnMonthClicked', year, month + 1);
        }
    });
    layers.interaction.add(headerHitArea);

    // Day grid
    const gridTop = y + 25;
    const gridHeight = height - 30;
    const cellWidth = width / 7;
    const firstDay = getFirstDayOfMonth(year, month);
    const lastDay = getLastDayOfMonth(year, month);
    const startDayOfWeek = firstDay.getDay();
    const daysInMonth = lastDay.getDate();
    const rows = Math.ceil((startDayOfWeek + daysInMonth) / 7);
    const cellHeight = gridHeight / (rows + 1);

    // Day of week headers
    const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
    for (let i = 0; i < 7; i++) {
        const dayHeader = new Konva.Text({
            x: x + i * cellWidth,
            y: gridTop,
            width: cellWidth,
            text: dayNames[i],
            fontSize: CONFIG.font.sizeSmall,
            fontFamily: CONFIG.font.family,
            fill: i === 0 ? CONFIG.colors.weekend.sun : i === 6 ? CONFIG.colors.weekend.sat : '#6b7280',
            align: 'center'
        });
        layers.grid.add(dayHeader);
    }

    // Draw grid lines and weekend backgrounds
    // サブピクセルレンダリング問題を回避するために座標を0.5pxオフセット
    function snapToPixel(val) {
        return Math.floor(val) + 0.5;
    }

    for (let row = 0; row <= rows; row++) {
        for (let col = 0; col < 7; col++) {
            const cellLeft = x + col * cellWidth;
            const cellTop = gridTop + (row + 1) * cellHeight;

            // Weekend background color
            if (row < rows) {
                let bgColor = null;
                if (col === 0) bgColor = 'rgba(239, 68, 68, 0.08)'; // Sunday - light red
                else if (col === 6) bgColor = 'rgba(59, 130, 246, 0.08)'; // Saturday - light blue

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

            // Vertical grid lines (曜日ヘッダーから最下段まで)
            if (col > 0) {
                const vLine = new Konva.Line({
                    points: [snapToPixel(cellLeft), snapToPixel(gridTop), snapToPixel(cellLeft), snapToPixel(gridTop + (rows + 1) * cellHeight)],
                    stroke: '#e5e7eb',
                    strokeWidth: 1
                });
                layers.grid.add(vLine);
            }
        }

        // Horizontal grid lines
        if (row > 0) {
            const hLine = new Konva.Line({
                points: [snapToPixel(x), snapToPixel(gridTop + (row + 1) * cellHeight), snapToPixel(x + width), snapToPixel(gridTop + (row + 1) * cellHeight)],
                stroke: '#e5e7eb',
                strokeWidth: 1
            });
            layers.grid.add(hLine);
        }
    }

    // 曜日ヘッダーと日付セルの境界線を追加
    const headerBottomLine = new Konva.Line({
        points: [snapToPixel(x), snapToPixel(gridTop + cellHeight), snapToPixel(x + width), snapToPixel(gridTop + cellHeight)],
        stroke: '#e5e7eb',
        strokeWidth: 1
    });
    layers.grid.add(headerBottomLine);

    // Day cells with bar charts
    let day = 1;
    for (let row = 0; row < rows; row++) {
        for (let col = 0; col < 7; col++) {
            const cellIndex = row * 7 + col;
            if (cellIndex < startDayOfWeek || day > daysInMonth) continue;

            const cellLeft = x + col * cellWidth;
            const cellTop = gridTop + (row + 1) * cellHeight;
            const cellX = cellLeft + cellWidth / 2;
            const cellY = cellTop + cellHeight / 2;
            const radius = Math.min(cellWidth, cellHeight) / 2 - 2;

            const dateStr = `${year}-${String(month + 1).padStart(2, '0')}-${String(day).padStart(2, '0')}`;
            const isHoliday = state.holidays.has(dateStr);

            // セルサイズの安全な計算
            const safeCellWidth = Math.max(0, cellWidth - 2);
            const safeCellHeight = Math.max(0, cellHeight - 2);
            const cellCornerRadius = Math.max(0, Math.min(2, safeCellWidth / 2, safeCellHeight / 2));

            // Holiday background (light red, same as Sunday)
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

            // Today's cell background (bright green rectangle)
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
