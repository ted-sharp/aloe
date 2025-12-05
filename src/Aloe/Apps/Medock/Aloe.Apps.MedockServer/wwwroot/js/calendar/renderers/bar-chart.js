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

    // グレーアウト判定: confirmedDateRange がある場合は範囲外をグレーアウト
    let isDateGrayed = false;
    if (state.confirmedDateRange) {
        isDateGrayed = !isDateInRange(dateStr, state.confirmedDateRange.start, state.confirmedDateRange.end);
    } else {
        isDateGrayed = stats?.isGrayedOut || false;
    }

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
        // 業務時間設定を取得
        const startHour = state.options.startHour || 8;
        const endHour = state.options.endHour || 18;
        const totalHours = endHour - startHour;
        const barAreaWidth = cellWidth - 4; // 左右余白2px
        const barWidth = 3; // 固定幅

        // AM/PM境界線を描画（12:00の位置）
        const noonHour = 12;
        if (startHour < noonHour && endHour > noonHour) {
            const noonPosition = (noonHour - startHour) / totalHours;
            const noonX = cellLeft + 2 + noonPosition * barAreaWidth;

            const noonLine = new Konva.Line({
                points: [noonX, barAreaTop, noonX, barAreaTop + barAreaHeight],
                stroke: '#9ca3af',
                strokeWidth: 1,
                dash: [2, 2], // 点線
                opacity: 0.5
            });
            layers.content.add(noonLine);
        }

        slots.forEach((slot, index) => {
            const ratio = slot.max > 0 ? slot.count / slot.max : 0;
            const isSlotGrayed = slot.isGrayedOut || isDateGrayed;
            const slotColor = isSlotGrayed ? '#9ca3af' : getSlotColor(ratio);
            const barHeight = barAreaHeight * ratio;

            // 時刻を解析（例: "08:00-09:00" → 開始時刻 "08:00" → 8.0）
            const startTime = slot.time.split('-')[0]; // "08:00-09:00" → "08:00"
            const timeParts = startTime.split(':');
            const hour = parseInt(timeParts[0], 10);
            const minute = timeParts[1] ? parseInt(timeParts[1], 10) : 0;
            const timeInHours = hour + minute / 60;

            // 業務時間内での相対位置を計算（0.0 ～ 1.0）
            const relativePosition = Math.max(0, Math.min(1,
                (timeInHours - startHour) / totalHours));

            // X座標を時刻に応じて配置
            const barX = cellLeft + 2 + relativePosition * (barAreaWidth - barWidth);
            const barY = barAreaTop + barAreaHeight - barHeight;

            // 通常の棒グラフ
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

            // フィルター条件の重ね表示（filteredCount > 0 の場合）
            if (slot.filteredCount > 0) {
                const filteredRatio = slot.max > 0 ? slot.filteredCount / slot.max : 0;
                const filteredBarHeight = barAreaHeight * filteredRatio;
                const filteredBarY = barAreaTop + barAreaHeight - filteredBarHeight;

                const filterBar = new Konva.Rect({
                    x: barX,
                    y: filteredBarY,
                    width: barWidth,
                    height: filteredBarHeight,
                    fill: '#fb923c', // オレンジ色
                    cornerRadius: 1,
                    opacity: isSlotGrayed ? 0.4 : 0.7  // グレーアウト時は透明度を下げる
                });
                layers.content.add(filterBar);
            }
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

            // Show room filter summary
            const hasRoomFilter = slots.some(s => s.filteredCount > 0);
            if (hasRoomFilter) {
                const totalFiltered = slots.reduce((sum, s) => sum + (s.filteredCount || 0), 0);
                tooltipContent += `<span style="color: #fb923c;">選択部屋: ${totalFiltered}件</span><br>`;
            }
            tooltipContent += '<br>';

            // 横棒グラフで表示
            tooltipContent += '<div style="font-size: 11px;">';
            slots.forEach(s => {
                const ratio = s.max > 0 ? (s.count / s.max) * 100 : 0;
                const color = ratio >= 100 ? '#ef4444' : ratio >= 70 ? '#f97316' : ratio >= 30 ? '#fbbf24' : '#3b82f6';

                // Show room-filtered count in each slot
                let roomInfo = '';
                if (s.filteredCount > 0) {
                    roomInfo = ` <span style="color: #fb923c;">[部屋:${s.filteredCount}]</span>`;
                }

                tooltipContent += `
                    <div style="margin-bottom: 6px;">
                        <div style="display: flex; justify-content: space-between; margin-bottom: 2px;">
                            <span>${s.time}</span>
                            <span>${s.count}/${s.max} (${Math.round(ratio)}%)${roomInfo}</span>
                        </div>
                        <div style="background: rgba(255,255,255,0.2); height: 8px; border-radius: 4px; overflow: hidden;">
                            <div style="background: ${color}; height: 100%; width: ${ratio}%; transition: width 0.2s;"></div>
                        </div>
                    </div>
                `;
            });
            tooltipContent += '</div>';
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
            // ホバー解除後は枠線を削除（今日の日付のみ枠線を残す）
            if (isToday(dateStr)) {
                bgRect.stroke(CONFIG.colors.today);
                bgRect.strokeWidth(2);
            } else {
                // 選択状態に関わらず、ホバー解除後は枠線を削除
                bgRect.stroke(null);
                bgRect.strokeWidth(0);
            }
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
            const range = { start: state.selectedDate, end: dateStr };
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateRangeSelected', range.start, range.end);
            }
            // Shift+クリックで範囲選択（両方の日付が異なる場合のみ confirmedDateRange を設定）
            setState({
                selectedDate: null,  // 範囲選択時は単一選択をクリア
                lastClickTime: 0,    // ダブルクリック判定をリセット
                lastClickDate: null,
                selectedDateRange: range,
                confirmedDateRange: range.start !== range.end ? range : null
            });
        } else if (state.confirmedDateRange &&
                   isDateInRange(dateStr, state.confirmedDateRange.start, state.confirmedDateRange.end)) {
            // 範囲選択内の日付をクリックした場合は範囲選択を解除
            setState({
                lastClickTime: 0,
                lastClickDate: null,
                selectedDate: null,
                selectedDateRange: null,
                confirmedDateRange: null
            });
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', null);
            }
            // カレンダーを再描画してグレーアウトを解除
            if (window.MedockCalendar) {
                window.MedockCalendar.render();
            }
        } else {
            // 同じ日付をクリックした場合は選択解除
            if (state.selectedDate === dateStr) {
                setState({
                    lastClickTime: 0,
                    lastClickDate: null,
                    selectedDate: null,
                    selectedDateRange: null,
                    confirmedDateRange: null
                });
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', null);
                }
            } else {
                setState({
                    lastClickTime: now,
                    lastClickDate: dateStr,
                    selectedDate: dateStr,
                    selectedDateRange: null,
                    confirmedDateRange: null
                });
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
                }
            }
        }
    });

    // ドラッグによる範囲選択
    hitArea.on('mousedown', function (e) {
        // mousedown では isDragging と dragStartDate のみ設定
        // selectedDateRange は mousemove で実際にドラッグが開始された時に設定
        setState({
            isDragging: true,
            dragStartDate: dateStr
        });
    });

    hitArea.on('mouseup', function () {
        // mouseup は処理しない（calendar-main.js の stage mouseup で処理される）
        // ここでは何もしない
    });

    hitArea.on('mousemove', function () {
        if (state.isDragging && state.dragStartDate) {
            // 実際にドラッグが開始された場合のみ selectedDateRange を設定
            setState({ selectedDateRange: { start: state.dragStartDate, end: dateStr } });
        }
    });

    layers.interaction.add(hitArea);
}
