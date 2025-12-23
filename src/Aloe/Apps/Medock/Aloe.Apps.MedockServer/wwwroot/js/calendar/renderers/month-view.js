/**
 * Month View Renderer
 *
 * 月間カレンダー表示（1ヶ月全体を7列グリッドで表示）
 */

import { getState } from '../state.js';
import { CONFIG } from '../config.js';
import { getFirstDayOfMonth, getLastDayOfMonth, isToday } from '../utils/date-utils.js';
import { renderDayBarChart } from './bar-chart/index.js';
import { renderMonthCalendar } from './month-calendar.js';

/**
 * 月間カレンダーを描画
 */
export function renderMonthView() {
    const state = getState();
    const { stage, layers, currentDate } = state;
    const width = stage.width();
    const height = stage.height();

    layers.background.destroyChildren();
    layers.grid.destroyChildren();
    layers.content.destroyChildren();
    layers.interaction.destroyChildren();

    const year = currentDate.getFullYear();
    const month = currentDate.getMonth();

    renderMonthCalendar(year, month, 0, 0, width, height, {
        showMonthHeader: false,
        headerHeight: 40,
        dayHeaderStyle: 'large',
        showEmptyCells: true,
        gridStyle: 'full'
    });

    layers.grid.batchDraw();
    layers.content.batchDraw();
    layers.interaction.batchDraw();
}
