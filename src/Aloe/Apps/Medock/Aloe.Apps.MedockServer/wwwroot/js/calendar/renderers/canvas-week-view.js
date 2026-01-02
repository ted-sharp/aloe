/**
 * Canvas Week View Renderer
 * 
 * 週スケジューラー表示（Canvas API版）
 * 時間軸カレンダーで予約ブロックを表示
 */

import { CONFIG } from '../config.js';
import { drawRect, drawLine, drawText, drawCircle } from '../utils/canvas-utils.js';
import { dateToString, getStartOfWeek, isToday, parseDate } from '../utils/date-utils.js';
import { getRenderState, resetRenderState } from './canvas-render-state.js';
import { getWinterColorFromAvailable } from '../utils/winter-colormap.js';
import {
    getSymbolFromVacancyRatio,
    calculateVacancyRatio,
    drawSymbol,
    drawSlotInfoText
} from '../utils/slot-display-utils.js';

/**
 * HH:mm形式の時間文字列を時間.分の数値に変換
 * @param {string} timeStr - "HH:mm"形式の時間文字列（例："09:30"）
 * @returns {number} 時間.分の数値（例：9.5）
 */
function parseTimeToHours(timeStr) {
    if (!timeStr) return 9.0;
    const parts = timeStr.split(':');
    const hours = parseInt(parts[0] || 0, 10);
    const minutes = parseInt(parts[1] || 0, 10);
    return hours + (minutes / 60);
}

/**
 * ステータスコードからテキストを取得
 */
function getStatusText(status) {
    const statusTexts = {
        0: '予約',
        1: '待機中',
        2: '来院済み',
        3: 'キャンセル'
    };
    return statusTexts[status] || '不明';
}

/**
 * 患者名からイニシャルを取得
 */
function getInitial(name) {
    if (!name) return '?';
    const parts = name.split(/[\s　]/);
    return parts[0].charAt(0);
}

/**
 * アバターの色パレット
 */
const avatarColors = [
    '#3b82f6', '#10b981', '#f59e0b', '#ef4444',
    '#8b5cf6', '#ec4899', '#06b6d4', '#f97316'
];

/**
 * 週スケジューラーを描画（Canvas API版）
 * @param {object} canvasManager - CanvasManagerインスタンス
 * @param {object} state - アプリケーション状態
 * @param {string} fadeMode - フェードモード: 'instant', 'crossfade', 'fadethrough', 'sharedelement'
 * @param {number} fadeDuration - フェード時間（ミリ秒）
 * @param {object} transitionInfo - トランジション情報 { sourceBounds, targetDateStr }
 */
export function renderCanvasWeekView(canvasManager, state, fadeMode = 'crossfade', fadeDuration = 200, transitionInfo = null) {
    // オフスクリーンバッファに描画（ダブルバッファリング）
    const contexts = canvasManager.getAllOffscreenContexts();
    const width = canvasManager.width;
    const height = canvasManager.height;

    // オフスクリーンレイヤーをクリア
    canvasManager.clearAllOffscreen();

    // Render Stateをリセット
    resetRenderState();
    const renderState = getRenderState();
    renderState.setViewType('week');

    const gridCtx = contexts.get('grid');
    const contentCtx = contexts.get('content');
    const backgroundCtx = contexts.get('background');

    const { weekDays, startHour, endHour, showSlots } = state.options;
    const hours = endHour - startHour;
    const timeColumnWidth = 60;
    const headerHeight = 65;
    const hourHeight = (height - headerHeight) / hours;

    // startDate: currentDate から始まる週表示
    const startDate = new Date(state.currentDate);
    startDate.setHours(0, 0, 0);

    // 各日付にデータがあるかチェックして、幅を動的に計算
    const dayWidths = [];
    const baseWidth = (width - timeColumnWidth) / weekDays;
    for (let i = 0; i < weekDays; i++) {
        const date = new Date(startDate);
        date.setDate(date.getDate() + i);
        const dateStr = dateToString(date);
        const hasData = state.appointments && state.appointments.some(appt => appt.date === dateStr);
        dayWidths.push(hasData ? baseWidth : baseWidth * 0.5);
    }

    // ヘッダー背景
    drawRect(gridCtx, {
        x: 0,
        y: 0,
        width: width,
        height: headerHeight,
        fill: '#f9fafb',
        stroke: CONFIG.colors.grid,
        strokeWidth: 1
    });

    // 時間列ヘッダー
    drawRect(gridCtx, {
        x: 0,
        y: 0,
        width: timeColumnWidth,
        height: headerHeight,
        fill: '#f3f4f6',
        stroke: CONFIG.colors.grid,
        strokeWidth: 1
    });

    // 曜日ヘッダー
    const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
    let xOffset = timeColumnWidth;
    for (let i = 0; i < weekDays; i++) {
        const date = new Date(startDate);
        date.setDate(date.getDate() + i);
        const dateStr = dateToString(date);
        const dayOfWeek = date.getDay();
        const dayWidth = dayWidths[i];
        const x = xOffset;
        const isHoliday = state.holidays && state.holidays.has(dateStr);

        // ヘッダーセル背景
        const isTodayCell = isToday(dateStr);
        drawRect(gridCtx, {
            x: x,
            y: 0,
            width: dayWidth,
            height: headerHeight,
            fill: isTodayCell ? 'rgba(16, 185, 129, 0.1)' : 'transparent',
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });

        // 曜日名
        const dayTextColor = isHoliday || dayOfWeek === 0 ? CONFIG.colors.weekend.sun : dayOfWeek === 6 ? CONFIG.colors.weekend.sat : '#374151';
        drawText(gridCtx, {
            text: dayNames[dayOfWeek],
            x: x,
            y: 10,
            width: dayWidth,
            fill: dayTextColor,
            fontSize: CONFIG.font.sizeMedium,
            fontStyle: 'bold',
            align: 'center'
        });

        // 日付
        drawText(gridCtx, {
            text: String(date.getDate()),
            x: x,
            y: 28,
            width: dayWidth,
            fill: dayTextColor,
            fontSize: CONFIG.font.sizeMedium,
            align: 'center'
        });

        // 祝日名を表示
        const holidayName = state.holidays?.get(dateStr);
        if (holidayName) {
            const maxHolidayChars = Math.floor(dayWidth / (9 * 0.6)); // 文字幅の推定
            const displayHolidayName = holidayName.length > maxHolidayChars
                ? holidayName.substring(0, Math.max(1, maxHolidayChars - 1)) + '…'
                : holidayName;

            drawText(gridCtx, {
                text: displayHolidayName,
                x: x,
                y: 41,
                width: dayWidth,
                fill: dayTextColor,
                fontSize: 9,
                align: 'center',
                opacity: 0.8
            });
        }

        xOffset += dayWidth;
    }

    // 時間軸グリッド
    for (let hour = 0; hour <= hours; hour++) {
        const y = headerHeight + hour * hourHeight;

        // 時間ラベル
        if (hour < hours) {
            const hourValue = startHour + hour;
            drawText(gridCtx, {
                text: `${String(hourValue).padStart(2, '0')}:00`,
                x: 5,
                y: y + hourHeight / 2 - 6,
                width: timeColumnWidth - 10,
                fill: '#6b7280',
                fontSize: CONFIG.font.sizeSmall,
                align: 'left'
            });
        }

        // 横線（時間の区切り）
        drawLine(gridCtx, {
            points: [timeColumnWidth, y, width, y],
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });

        // 30分スロットの区切り線を描画（簡易表示・詳細表示両方）
        if (hour < hours) {
            const halfHourY = y + hourHeight / 2;
            drawLine(gridCtx, {
                points: [timeColumnWidth, halfHourY, width, halfHourY],
                stroke: CONFIG.colors.grid,
                strokeWidth: 0.5,
                opacity: 0.5
            });
        }

        // 背景色（交互）
        if (hour < hours && hour % 2 === 0) {
            drawRect(backgroundCtx, {
                x: timeColumnWidth,
                y: y,
                width: width - timeColumnWidth,
                height: hourHeight,
                fill: '#f9fafb',
                opacity: 0.5
            });
        }
    }

    // 縦線（曜日の区切り）
    let verticalLineX = timeColumnWidth;
    for (let i = 0; i <= weekDays; i++) {
        drawLine(gridCtx, {
            points: [verticalLineX, headerHeight, verticalLineX, height],
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });
        if (i < weekDays) {
            verticalLineX += dayWidths[i];
        }
    }

    // 各日付のスロット統計を描画
    for (let i = 0; i < weekDays; i++) {
        const date = new Date(startDate);
        date.setDate(date.getDate() + i);
        const dateStr = dateToString(date);

        // dayIndまでの幅の合計を計算
        let xStart = timeColumnWidth;
        for (let j = 0; j < i; j++) {
            xStart += dayWidths[j];
        }
        const dayWidth = dayWidths[i];
        const cellLeft = xStart;
        const cellTop = headerHeight;
        const cellWidth = dayWidth;
        const cellHeight = height - headerHeight;

        const isHoliday = state.holidays && state.holidays.has(dateStr);

        if (showSlots) {
            // 簡易表示モード: mainStatsの各スロットごとに記号を描画（月間ビューの棒グラフと同じロジック）
            const stats = state.mainStats?.get(dateStr);
            if (stats && stats.slotStarts && stats.slotCaps && stats.slotCounts) {
                const slotStarts = stats.slotStarts || [];
                const slotEnds = stats.slotEnds || [];
                const slotCounts = stats.slotCounts || [];
                const slotCaps = stats.slotCaps || [];
                const slotFlags = stats.slotFlags || null;
                const slotCount = slotStarts.length;

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

                // 表示対象のスロットを判定
                const validIndices = [];
                for (let idx = 0; idx < slotCount; idx++) {
                    const cap = (slotCaps[idx] !== undefined && slotCaps[idx] !== null && slotCaps[idx] > 0) ? slotCaps[idx] : 0;
                    const count = (slotCounts[idx] !== undefined && slotCounts[idx] !== null) ? slotCounts[idx] : 0;
                    if (cap > 0 || count > 0) {
                        validIndices.push(idx);
                    }
                }

                // 時間範囲内のスロットを取得
                const slotsInRange = [];
                for (const idx of validIndices) {
                    const slotStartMinutes = slotStarts[idx];
                    const slotEndMinutes = slotEnds[idx];
                    const slotStartHour = slotStartMinutes / 60;
                    const slotEndHour = slotEndMinutes / 60;

                    // 昼休み時間帯かどうかを判定
                    const isInLunchTime = lunchStartHour !== null && lunchEndHour !== null &&
                        ((slotStartHour >= lunchStartHour && slotStartHour < lunchEndHour) ||
                            (slotEndHour > lunchStartHour && slotEndHour <= lunchEndHour) ||
                            (slotStartHour <= lunchStartHour && slotEndHour >= lunchEndHour));

                    if (slotEndHour > startHour && slotStartHour < endHour && !isInLunchTime) {
                        slotsInRange.push(idx);
                    }
                }

                // 各スロットごとに記号を描画（時刻軸に沿って縦に配置）
                slotsInRange.forEach((idx) => {
                    const slotStartMinutes = slotStarts[idx];
                    const slotEndMinutes = slotEnds[idx];
                    const slotStartHour = Math.max(startHour, slotStartMinutes / 60);
                    const slotEndHour = Math.min(endHour, slotEndMinutes / 60);

                    const count = (slotCounts[idx] !== undefined && slotCounts[idx] !== null) ? slotCounts[idx] : 0;
                    const cap = (slotCaps[idx] !== undefined && slotCaps[idx] !== null && slotCaps[idx] > 0) ? slotCaps[idx] : 0;
                    const available = cap - count;

                    const { vacancyRatio } = calculateVacancyRatio([cap], [count], null, false);
                    const symbolType = getSymbolFromVacancyRatio(vacancyRatio);

                    // スロットの描画位置を計算（日の列内、時刻軸に沿って）
                    const slotTop = headerHeight + (slotStartHour - startHour) * hourHeight;
                    const slotHeight = (slotEndHour - slotStartHour) * hourHeight;
                    const slotLeft = cellLeft + 2;
                    const slotWidth = cellWidth - 4;

                    // スロットブロックを描画（薄い背景 + 枠線で範囲を表示）
                    const slotColor = getWinterColorFromAvailable(available, cap);
                    drawRect(contentCtx, {
                        x: slotLeft,
                        y: slotTop,
                        width: slotWidth,
                        height: slotHeight,
                        fill: slotColor,
                        cornerRadius: 4,
                        opacity: 0.15,  // 背景は薄く
                        stroke: slotColor,  // 枠線は濃い色
                        strokeWidth: 2
                    });

                    // シンボルをブロック内に配置
                    const symbolCenterX = slotLeft + slotWidth / 2;
                    const symbolCenterY = slotTop + slotHeight / 2;
                    const symbolSize = Math.min(slotWidth * 0.4, slotHeight * 0.5, 20);

                    drawSymbol(contentCtx, symbolCenterX, symbolCenterY, symbolSize, symbolType, 1.0);

                    // 「n/m」テキストも表示（スロット高さが十分な場合）
                    if (slotHeight >= 40 && cap > 0) {
                        const textY = symbolCenterY + symbolSize / 2 + 4;
                        drawSlotInfoText(contentCtx, slotLeft + 2, textY, slotWidth - 4, available, cap, false, 1.0);
                    }

                    // Hit Test用に登録
                    renderState.addWeekSlot({
                        bounds: { x: slotLeft, y: slotTop, width: slotWidth, height: slotHeight },
                        dateStr: dateStr,
                        slotIndex: idx,
                        stats: stats
                    });
                });
            }
        } else {
            // 詳細表示モード: アバター表示（30分スロット単位）
            // 該当日のAppointmentsを取得
            const dayAppointments = state.appointments?.filter(appt => appt.date === dateStr) || [];

            // 時間範囲内のAppointmentsをスロット別にグループ化
            const slotMap = new Map(); // key: "hour-half" -> appointments[]

            dayAppointments.forEach(appt => {
                const startTime = parseTimeToHours(appt.startTime || '09:00');
                if (startTime < startHour || startTime >= endHour) return;

                const hourNum = Math.floor(startTime);
                const minNum = Math.round((startTime - hourNum) * 60);
                const half = minNum >= 30 ? 1 : 0;
                const slotKey = `${hourNum}-${half}`;

                if (!slotMap.has(slotKey)) {
                    slotMap.set(slotKey, []);
                }
                slotMap.get(slotKey).push(appt);
            });

            // 各スロットにアバターを描画
            slotMap.forEach((slotAppts, slotKey) => {
                const [hourNum, half] = slotKey.split('-').map(Number);
                const slotHeight = hourHeight / 2;
                const maxAvatars = 4; // 1スロット最大4人まで表示
                const avatarSize = Math.min(28, slotHeight - 4, (cellWidth - 8) / Math.min(slotAppts.length, maxAvatars) - 4);

                const baseX = cellLeft + 4;
                const baseY = headerHeight + (hourNum - startHour) * hourHeight + half * slotHeight + (slotHeight - avatarSize) / 2;

                slotAppts.slice(0, maxAvatars).forEach((appt, idx) => {
                    const avatarX = baseX + idx * (avatarSize + 4);
                    const avatarY = baseY;
                    const initial = getInitial(appt.patientName);
                    const colorIdx = appt.patientName ? appt.patientName.charCodeAt(0) % avatarColors.length : 0;
                    const avatarColor = avatarColors[colorIdx];

                    // アバター円
                    drawCircle(contentCtx, {
                        x: avatarX + avatarSize / 2,
                        y: avatarY + avatarSize / 2,
                        radius: avatarSize / 2,
                        fill: avatarColor,
                        stroke: '#ffffff',
                        strokeWidth: 2
                    });

                    // イニシャル文字
                    drawText(contentCtx, {
                        text: initial,
                        x: avatarX,
                        y: avatarY + avatarSize / 2 - 7,
                        width: avatarSize,
                        fill: '#ffffff',
                        fontSize: 14,
                        fontStyle: 'bold',
                        align: 'center'
                    });

                    // Hit Test用に登録
                    renderState.addWeekSlot({
                        bounds: {
                            x: avatarX,
                            y: avatarY,
                            width: avatarSize,
                            height: avatarSize
                        },
                        appointment: appt
                    });
                });
            });
        }
    }

    // すべての描画が完了したら、オフスクリーンバッファをメインCanvasに一括転送
    let commitOptions = {};

    if (fadeMode === 'sharedelement' && transitionInfo) {
        // 月 → 週: 月間ビューの週行bounds → 週ビュー全体
        if (transitionInfo.transitionType === 'month-to-week') {
            // 遷移元: calendar-main.jsで保存された月間ビューの週行bounds
            // 遷移先: 週ビュー全体（canvas全体）
            if (transitionInfo.sourceBounds) {
                commitOptions = {
                    sourceBounds: transitionInfo.sourceBounds,
                    targetBounds: { x: 0, y: 0, width, height },
                    transitionType: transitionInfo.transitionType
                };
                // console.log('Week View: Month-to-Week transition', commitOptions);
            } else {
                // console.warn('Week View: sourceBounds not found in transitionInfo, falling back to scalefade');
                fadeMode = 'scalefade';
            }
        }
        // 週 → 月: この遷移は月ビューで処理されるため、ここには来ない
        else if (transitionInfo.transitionType === 'week-to-month') {
            // console.warn('Week View: Unexpected week-to-month transition (should be handled by month view)');
            fadeMode = 'scalefade';
        }
    }

    canvasManager.commitAll(fadeMode, fadeDuration, commitOptions);
}


