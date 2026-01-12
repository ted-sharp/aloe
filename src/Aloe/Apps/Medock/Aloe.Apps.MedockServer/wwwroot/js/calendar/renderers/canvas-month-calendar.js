/**
 * Canvas Month Calendar Renderer
 * 
 * 月のカレンダー描画（Canvas API版）
 * 年間カレンダーと月間カレンダーの両方で使用
 */

import { CONFIG } from '../config.js';
import { getFirstDayOfMonth, getLastDayOfMonth, isToday } from '../utils/date-utils.js';
import { drawRect, drawLine, drawText, snapToPixel } from '../utils/canvas-utils.js';
import { getRenderState } from './canvas-render-state.js';
import { updateConfigColorsForCurrentTheme } from '../utils/theme-utils.js';

/**
 * 月のカレンダーを描画（Canvas API版）
 * @param {Map<string, CanvasRenderingContext2D>} contexts - レイヤーコンテキストのマップ
 * @param {object} state - アプリケーション状態
 * @param {number} year - 年
 * @param {number} month - 月 (0-11)
 * @param {number} x - X座標
 * @param {number} y - Y座標
 * @param {number} width - 幅
 * @param {number} height - 高さ
 * @param {object} options - オプション
 * @returns {object} 月情報 { bounds, dayGridTop, dayGridHeight, cellWidth, cellHeight, rows, startDayOfWeek, daysInMonth }
 */
export function renderCanvasMonthCalendar(contexts, state, year, month, x, y, width, height, options = {}) {
    // CONFIG.colorsが初期化されていない場合は初期化（空オブジェクトもチェック）
    if (!CONFIG.colors || Object.keys(CONFIG.colors).length === 0 || !CONFIG.colors.grid) {
        updateConfigColorsForCurrentTheme(CONFIG);
    }

    const opts = {
        showMonthHeader: false,
        onMonthHeaderClick: null,
        headerHeight: null,
        dayHeaderStyle: 'small',
        showEmptyCells: false,
        gridStyle: 'minimal',
        ...options
    };

    if (width < 20 || height < 20) {
        return null;
    }

    const gridCtx = contexts.get('grid');
    const renderState = getRenderState();

    const monthNames = ['1月', '2月', '3月', '4月', '5月', '6月', '7月', '8月', '9月', '10月', '11月', '12月'];

    let monthHeaderOffset = 0;
    let monthHeaderBounds = null;

    // 月ヘッダーの描画
    if (opts.showMonthHeader) {
        const headerHeight = Math.min(22, height / 4);
        const headerCornerRadius = Math.min(4, width / 2, headerHeight / 2);

        // 月ヘッダー背景（透明、クリック用の領域として記録）
        monthHeaderBounds = {
            x: x,
            y: y - 2,
            width: Math.max(0, width),
            height: headerHeight
        };

        // 月名テキスト
        drawText(gridCtx, {
            text: monthNames[month],
            x: x,
            y: y,
            width: width,
            fill: CONFIG.colors.text,
            fontSize: CONFIG.font.sizeLarge,
            fontStyle: 'bold',
            align: 'center'
        });

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

    // 曜日ヘッダーの描画
    const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
    const dayHeaderFontSize = opts.dayHeaderStyle === 'large' ? CONFIG.font.sizeLarge : CONFIG.font.sizeSmall;
    const dayHeaderHeight = opts.dayHeaderStyle === 'large' ? headerHeight : Math.min(headerHeight, cellHeight * 0.3);
    const dayHeaderY = opts.dayHeaderStyle === 'large' ? gridTop + 12 : gridTop + dayHeaderHeight / 2 - dayHeaderFontSize / 2;

    for (let i = 0; i < 7; i++) {
        const headerX = x + i * cellWidth;

        if (opts.dayHeaderStyle === 'large') {
            // 大きなヘッダー背景
            drawRect(gridCtx, {
                x: headerX,
                y: gridTop,
                width: cellWidth,
                height: headerHeight,
                fill: CONFIG.colors.dayHeaderBg,
                stroke: CONFIG.colors.grid || '#d1d5db',
                strokeWidth: 1
            });
        } else if (opts.dayHeaderStyle === 'small') {
            // 小さなヘッダー枠線
            drawRect(gridCtx, {
                x: headerX,
                y: gridTop,
                width: cellWidth,
                height: dayHeaderHeight,
                fill: 'transparent',
                stroke: CONFIG.colors.grid || '#d1d5db',
                strokeWidth: 1
            });
        }

        // 曜日名
        const dayColor = i === 0 ? CONFIG.colors.weekend.sun
                       : i === 6 ? CONFIG.colors.weekend.sat
                       : CONFIG.colors.dayHeaderText;

        drawText(gridCtx, {
            text: dayNames[i],
            x: headerX,
            y: dayHeaderY,
            width: cellWidth,
            fill: dayColor,
            fontSize: dayHeaderFontSize,
            fontStyle: opts.dayHeaderStyle === 'large' ? 'bold' : 'normal',
            align: 'center'
        });
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
                    // 空セル
                    drawRect(gridCtx, {
                        x: cellLeft,
                        y: cellTop,
                        width: cellWidth,
                        height: cellHeight,
                        fill: CONFIG.colors.background,
                        stroke: CONFIG.colors.grid || '#d1d5db',
                        strokeWidth: 1
                    });
                } else {
                    // 通常セル
                    drawRect(gridCtx, {
                        x: cellLeft,
                        y: cellTop,
                        width: cellWidth,
                        height: cellHeight,
                        fill: 'transparent',
                        stroke: CONFIG.colors.grid || '#d1d5db',
                        strokeWidth: 1
                    });
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

                // 縦線（左端以外）
                if (col > 0) {
                    drawLine(gridCtx, {
                        points: [
                            snapToPixel(cellLeft), 
                            snapToPixel(dayGridTop), 
                            snapToPixel(cellLeft), 
                            snapToPixel(dayGridTop + rows * cellHeight)
                        ],
                        stroke: CONFIG.colors.grid || '#d1d5db',
                        strokeWidth: 1
                    });
                }

                // 横線（上端以外）
                if (row > 0) {
                    drawLine(gridCtx, {
                        points: [
                            snapToPixel(x), 
                            snapToPixel(dayGridTop + row * cellHeight), 
                            snapToPixel(x + width), 
                            snapToPixel(dayGridTop + row * cellHeight)
                        ],
                        stroke: CONFIG.colors.grid || '#d1d5db',
                        strokeWidth: 1
                    });
                }
            }
        }

        if (rows > 0) {
            // 左端の縦線
            drawLine(gridCtx, {
                points: [
                    snapToPixel(x), 
                    snapToPixel(dayGridTop), 
                    snapToPixel(x), 
                    snapToPixel(dayGridTop + rows * cellHeight)
                ],
                stroke: '#e5e7eb',
                strokeWidth: 1
            });

            // 右端の縦線
            drawLine(gridCtx, {
                points: [
                    snapToPixel(x + width), 
                    snapToPixel(dayGridTop), 
                    snapToPixel(x + width), 
                    snapToPixel(dayGridTop + rows * cellHeight)
                ],
                stroke: '#e5e7eb',
                strokeWidth: 1
            });

            // ヘッダー下の横線
            drawLine(gridCtx, {
                points: [
                    snapToPixel(x), 
                    snapToPixel(dayGridTop), 
                    snapToPixel(x + width), 
                    snapToPixel(dayGridTop)
                ],
                stroke: '#e5e7eb',
                strokeWidth: 1
            });

            // 最終行の下側の横線
            drawLine(gridCtx, {
                points: [
                    snapToPixel(x), 
                    snapToPixel(dayGridTop + rows * cellHeight), 
                    snapToPixel(x + width), 
                    snapToPixel(dayGridTop + rows * cellHeight)
                ],
                stroke: '#e5e7eb',
                strokeWidth: 1
            });
        }
    }

    // 今日の日付のハイライト（日付セル描画の前に）
    const backgroundCtx = contexts.get('background');
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

            const safeCellWidth = Math.max(0, cellWidth - 2);
            const safeCellHeight = Math.max(0, cellHeight - 2);
            const cellCornerRadius = Math.max(0, Math.min(2, safeCellWidth / 2, safeCellHeight / 2));

            // 今日の日付のハイライト
            if (isToday(dateStr) && safeCellWidth > 0 && safeCellHeight > 0) {
                drawRect(backgroundCtx, {
                    x: cellLeft + 1,
                    y: cellTop + 1,
                    width: safeCellWidth,
                    height: safeCellHeight,
                    fill: 'rgba(16, 185, 129, 0.3)',
                    cornerRadius: cellCornerRadius
                });
            }

            // セル情報をRenderStateに追加
            renderState.addCell(dateStr, {
                x: cellLeft,
                y: cellTop,
                width: cellWidth,
                height: cellHeight,
                month: month,
                day: day
            });

            day++;
        }
    }

    // 月情報を返す（バーチャート描画用）
    return {
        bounds: { x, y, width, height },
        monthHeaderBounds,
        dayGridTop,
        dayGridHeight,
        cellWidth,
        cellHeight,
        rows,
        startDayOfWeek,
        daysInMonth,
        onMonthHeaderClick: opts.onMonthHeaderClick
    };
}


