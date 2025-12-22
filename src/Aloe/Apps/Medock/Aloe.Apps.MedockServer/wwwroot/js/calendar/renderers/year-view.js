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
import { renderMonthCalendar } from './month-calendar.js';

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
    const { stage, layers, currentDate, mainStats } = state;
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

        renderMonthCalendar(year, month, x, y, w, h, {
            showMonthHeader: true,
            onMonthHeaderClick: (y, m) => {
                const state = getState();
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnMonthClicked', y, m + 1);
                }
            },
            dayHeaderStyle: 'small',
            gridStyle: 'minimal'
        });
    }

    layers.grid.batchDraw();
    layers.content.batchDraw();
    layers.interaction.batchDraw();
}

