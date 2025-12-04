/**
 * Bar Chart Renderer
 *
 * 棒グラフ形式での日付セル描画
 * 年表示・月表示で使用される
 */

import { getState, setState } from '../state.js';
import { CONFIG } from '../config.js';
import { isToday, isDateInRange } from '../utils/date-utils.js';
import { showTooltip, hideTooltip } from '../ui/tooltip.js';

/**
 * 時間帯枠の充足率から色を取得
 * 空き→青、一部→黄、満杯→赤
 * @param {number} ratio - 充足率（0.0 ～ 1.0）
 * @returns {string} カラーコード
 */
export function getSlotColor(ratio) {
    if (ratio >= 1.0) {
        return CONFIG.colors.slot.full; // 満杯 - 赤
    } else if (ratio >= 0.7) {
        // 70%以上 - 赤と黄の中間
        return '#f97316'; // orange
    } else if (ratio >= 0.3) {
        return CONFIG.colors.slot.partial; // 一部埋まり - 黄
    } else if (ratio > 0) {
        // 30%未満 - 黄と青の中間
        return '#22d3ee'; // cyan
    } else {
        return CONFIG.colors.slot.empty; // 空き - 青
    }
}

/**
 * 棒グラフ形式で日付セルを描画
 * @param {number} cellLeft - セルの左端X座標
 * @param {number} cellTop - セルの上端Y座標
 * @param {number} cellWidth - セル幅
 * @param {number} cellHeight - セル高さ
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {number} dayNumber - 日にち
 * @param {boolean} isHoliday - 祝日フラグ
 */
export function renderDayBarChart(cellLeft, cellTop, cellWidth, cellHeight, dateStr, dayNumber, isHoliday = false) {
    const state = getState();
    const { layers } = state;

    // 時間帯枠データを取得
    const stats = state.dayStats.get(dateStr);
    const slots = stats?.slots || null;
    const isDateGrayed = stats?.isGrayedOut || false;

    // 日付テキスト表示エリア
    const dayTextHeight = CONFIG.font.sizeSmall + 4;
    const barAreaTop = cellTop + dayTextHeight;
    const barAreaHeight = cellHeight - dayTextHeight - 4; // 下部余白4px

    // 背景矩形
    const bgRect = new Konva.Rect({
        x: cellLeft + 1,
        y: cellTop + 1,
        width: cellWidth - 2,
        height: cellHeight - 2,
        fill: isDateGrayed ? '#f3f4f6' : CONFIG.colors.slot.background,
        cornerRadius: 2,
        opacity: isDateGrayed ? 0.6 : 1
    });
    layers.content.add(bgRect);

    // 日付テキスト
    const dayOfWeek = new Date(dateStr).getDay();
    let textColor;
    if (isDateGrayed) {
        textColor = '#9ca3af';
    } else if (isHoliday || dayOfWeek === 0) {
        textColor = CONFIG.colors.weekend.sun;
    } else if (dayOfWeek === 6) {
        textColor = CONFIG.colors.weekend.sat;
    } else {
        textColor = '#374151';
    }

    const dayText = new Konva.Text({
        x: cellLeft + 1,
        y: cellTop + 2,
        width: cellWidth - 2,
        text: String(dayNumber),
        fontSize: CONFIG.font.sizeSmall,
        fontFamily: CONFIG.font.family,
        fill: textColor,
        align: 'center',
        wrap: 'none'
    });
    layers.content.add(dayText);

    // 棒グラフ描画
    if (slots && slots.length > 0) {
        // 時間帯枠ベースの表示
        const slotCount = slots.length;
        const gapWidth = 1; // 棒の間隔
        const barAreaWidth = cellWidth - 4; // 左右余白2px
        const barWidth = (barAreaWidth - (slotCount - 1) * gapWidth) / slotCount;

        slots.forEach((slot, index) => {
            const ratio = slot.max > 0 ? slot.count / slot.max : 0;
            const isSlotGrayed = slot.isGrayedOut || isDateGrayed;
            const slotColor = isSlotGrayed ? '#9ca3af' : getSlotColor(ratio);
            const barHeight = barAreaHeight * ratio;

            const barX = cellLeft + 2 + index * (barWidth + gapWidth);
            const barY = barAreaTop + barAreaHeight - barHeight;

            const bar = new Konva.Rect({
                x: barX,
                y: barY,
                width: barWidth,
                height: barHeight,
                fill: slotColor,
                cornerRadius: 1,
                opacity: isSlotGrayed ? 0.4 : 1
            });
            layers.content.add(bar);
        });
    } else {
        // フォールバック: AM/PM 2本の棒
        const stats = state.dayStats.get(dateStr) || { am: 0, pm: 0, amMax: 10, pmMax: 10 };
        const amRatio = stats.amMax > 0 ? stats.am / stats.amMax : 0;
        const pmRatio = stats.pmMax > 0 ? stats.pm / stats.pmMax : 0;

        const barAreaWidth = cellWidth - 4;
        const gapWidth = 1;
        const barWidth = (barAreaWidth - gapWidth) / 2;

        // AM棒
        const amBarHeight = barAreaHeight * amRatio;
        const amBar = new Konva.Rect({
            x: cellLeft + 2,
            y: barAreaTop + barAreaHeight - amBarHeight,
            width: barWidth,
            height: amBarHeight,
            fill: getSlotColor(amRatio),
            cornerRadius: 1
        });
        layers.content.add(amBar);

        // PM棒
        const pmBarHeight = barAreaHeight * pmRatio;
        const pmBar = new Konva.Rect({
            x: cellLeft + 2 + barWidth + gapWidth,
            y: barAreaTop + barAreaHeight - pmBarHeight,
            width: barWidth,
            height: pmBarHeight,
            fill: getSlotColor(pmRatio),
            cornerRadius: 1
        });
        layers.content.add(pmBar);
    }

    // インタラクションエリア
    const hitArea = new Konva.Rect({
        x: cellLeft,
        y: cellTop,
        width: cellWidth,
        height: cellHeight,
        fill: 'transparent'
    });

    hitArea.on('mouseenter', function (e) {
        let tooltipContent = `<strong>${dateStr}</strong><br>`;
        if (slots && slots.length > 0) {
            const totalCount = slots.reduce((sum, s) => sum + s.count, 0);
            const totalMax = slots.reduce((sum, s) => sum + s.max, 0);
            tooltipContent += `予約: ${totalCount}/${totalMax}件<br>`;
            tooltipContent += `<small>`;
            slots.forEach(s => {
                const emoji = s.count >= s.max ? '🔴' : s.count > 0 ? '🟡' : '🔵';
                tooltipContent += `${s.time}: ${emoji} ${s.count}/${s.max}<br>`;
            });
            tooltipContent += `</small>`;
        } else {
            const st = state.dayStats.get(dateStr) || { am: 0, pm: 0 };
            tooltipContent += `午前: ${st.am}件<br>午後: ${st.pm}件`;
        }
        showTooltip(e.evt.clientX, e.evt.clientY, tooltipContent);
        bgRect.stroke(CONFIG.colors.today);
        bgRect.strokeWidth(2);
        layers.content.batchDraw();
    });

    hitArea.on('mouseleave', function () {
        hideTooltip();
        if (!state.isDragging) {
            const isSelected = state.selectedDate === dateStr ||
                (state.selectedDateRange && isDateInRange(dateStr, state.selectedDateRange.start, state.selectedDateRange.end));
            bgRect.stroke(isSelected ? '#3b82f6' : isToday(dateStr) ? CONFIG.colors.today : null);
            bgRect.strokeWidth(isSelected || isToday(dateStr) ? 2 : 0);
        }
        layers.content.batchDraw();
    });

    // クリックハンドラ（ダブルクリック・Shift+クリック対応）
    hitArea.on('click', function (e) {
        const now = Date.now();
        const isDoubleClick = (now - state.lastClickTime < 300) && (state.lastClickDate === dateStr);
        const isShiftClick = e.evt.shiftKey;

        if (isDoubleClick) {
            setState({ lastClickTime: 0, lastClickDate: null });
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateDoubleClicked', dateStr);
            }
        } else if (isShiftClick && state.selectedDate) {
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateRangeSelected', state.selectedDate, dateStr);
            }
            setState({ selectedDateRange: { start: state.selectedDate, end: dateStr } });
        } else {
            setState({
                lastClickTime: now,
                lastClickDate: dateStr,
                selectedDate: dateStr,
                selectedDateRange: null
            });
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
            }
        }
    });

    // ドラッグによる範囲選択
    hitArea.on('mousedown', function (e) {
        setState({
            isDragging: true,
            dragStartDate: dateStr,
            selectedDateRange: { start: dateStr, end: dateStr }
        });
    });

    hitArea.on('mouseup', function () {
        if (state.isDragging && state.dragStartDate) {
            const start = state.dragStartDate;
            const end = dateStr;
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateRangeSelected', start, end);
            }
            setState({ isDragging: false });
        }
    });

    hitArea.on('mousemove', function () {
        if (state.isDragging && state.dragStartDate) {
            setState({ selectedDateRange: { start: state.dragStartDate, end: dateStr } });
        }
    });

    layers.interaction.add(hitArea);
}
