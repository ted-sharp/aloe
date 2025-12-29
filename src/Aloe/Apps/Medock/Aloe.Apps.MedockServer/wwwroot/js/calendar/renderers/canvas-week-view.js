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
    const headerHeight = 50;
    const dayWidth = (width - timeColumnWidth) / weekDays;
    const hourHeight = (height - headerHeight) / hours;

    const startDate = getStartOfWeek(state.currentDate);

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
    for (let i = 0; i < weekDays; i++) {
        const date = new Date(startDate);
        date.setDate(date.getDate() + i);
        const dateStr = dateToString(date);
        const dayOfWeek = date.getDay();
        const x = timeColumnWidth + i * dayWidth;

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
        drawText(gridCtx, {
            text: dayNames[dayOfWeek],
            x: x,
            y: 10,
            width: dayWidth,
            fill: dayOfWeek === 0 ? CONFIG.colors.weekend.sun : dayOfWeek === 6 ? CONFIG.colors.weekend.sat : '#374151',
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
            fill: '#6b7280',
            fontSize: CONFIG.font.sizeSmall,
            align: 'center'
        });
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

        // 詳細表示モードの場合、30分スロットの区切り線も描画
        if (!showSlots && hour < hours) {
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
    for (let i = 0; i <= weekDays; i++) {
        const x = timeColumnWidth + i * dayWidth;
        drawLine(gridCtx, {
            points: [x, headerHeight, x, height],
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });
    }

    // 予約を描画
    if (state.appointments && state.appointments.length > 0) {
        const filteredAppointments = state.appointments.filter(appt => {
            const apptDate = parseDate(appt.date);
            const daysDiff = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));
            return daysDiff >= 0 && daysDiff < weekDays;
        });

        if (showSlots) {
            // 簡易表示モード: ブロック表示
            filteredAppointments.forEach((appt, index) => {
                const apptDate = parseDate(appt.date);
                const daysDiff = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));
                const startTime = parseTimeToHours(appt.startTime || '09:00');
                const endTime = parseTimeToHours(appt.endTime || '10:00');

                if (startTime < startHour || endTime > endHour) {
                    return; // 範囲外
                }

                const x = timeColumnWidth + daysDiff * dayWidth + 2;
                const y = headerHeight + (startTime - startHour) * hourHeight;
                const blockWidth = dayWidth - 4;
                const blockHeight = (endTime - startTime) * hourHeight;

                const statusColor = CONFIG.colors.status[appt.status] || '#9ca3af';

                // ブロック
                drawRect(contentCtx, {
                    x: x,
                    y: y,
                    width: blockWidth,
                    height: blockHeight,
                    fill: statusColor,
                    cornerRadius: 4,
                    opacity: 0.9
                });

                // テキスト
                if (blockHeight > 20) {
                    drawText(contentCtx, {
                        text: appt.patientName || '患者',
                        x: x + 5,
                        y: y + 5,
                        width: blockWidth - 10,
                        fill: '#ffffff',
                        fontSize: CONFIG.font.sizeSmall,
                        fontStyle: 'bold'
                    });

                    if (blockHeight > 40 && appt.organizationName) {
                        drawText(contentCtx, {
                            text: appt.organizationName,
                            x: x + 5,
                            y: y + 20,
                            width: blockWidth - 10,
                            fill: '#ffffff',
                            fontSize: CONFIG.font.sizeSmall - 1
                        });
                    }
                }

                // Hit Test用に登録
                renderState.addWeekSlot({
                    bounds: { x, y, width: blockWidth, height: blockHeight },
                    appointment: appt
                });
            });
        } else {
            // 詳細表示モード: アバター表示（30分スロット単位）
            const slotMap = new Map(); // key: "dayIndex-hour-half" -> appointments[]

            filteredAppointments.forEach(appt => {
                const apptDate = parseDate(appt.date);
                const daysDiff = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));

                if (daysDiff < 0 || daysDiff >= weekDays) return;

                const startTime = parseTimeToHours(appt.startTime || '09:00');
                if (startTime < startHour || startTime >= endHour) return;

                const hourNum = Math.floor(startTime);
                const minNum = Math.round((startTime - hourNum) * 60);
                const half = minNum >= 30 ? 1 : 0;
                const slotKey = `${daysDiff}-${hourNum}-${half}`;

                if (!slotMap.has(slotKey)) {
                    slotMap.set(slotKey, []);
                }
                slotMap.get(slotKey).push(appt);
            });

            // 各スロットにアバターを描画
            slotMap.forEach((slotAppts, slotKey) => {
                const [dayIndex, hourNum, half] = slotKey.split('-').map(Number);
                const slotHeight = hourHeight / 2;
                const maxAvatars = 4; // 1スロット最大4人まで表示
                const avatarSize = Math.min(28, slotHeight - 4, (dayWidth - 8) / Math.min(slotAppts.length, maxAvatars) - 4);

                const baseX = timeColumnWidth + dayIndex * dayWidth + 4;
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
    // 週間ビューは日付セルの概念がないため、共有要素トランジションは通常のフェードにフォールバック
    if (fadeMode === 'sharedelement') {
        fadeMode = 'scalefade';
    }

    canvasManager.commitAll(fadeMode, fadeDuration);
}


