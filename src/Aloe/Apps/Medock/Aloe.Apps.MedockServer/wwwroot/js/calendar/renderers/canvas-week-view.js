/**
 * Canvas Week View Renderer
 * 
 * 週スケジューラー表示（Canvas API版）
 * 時間軸カレンダーで予約ブロックを表示
 */

import { CONFIG } from '../config.js';
import { drawRect, drawLine, drawText, drawCircle } from '../utils/canvas-utils.js';
import { dateToString, getStartOfWeek, isToday } from '../utils/date-utils.js';
import { getRenderState, resetRenderState } from './canvas-render-state.js';

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
 */
export function renderCanvasWeekView(canvasManager, state) {
    const contexts = canvasManager.getAllContexts();
    const width = canvasManager.width;
    const height = canvasManager.height;

    // レイヤーをクリア
    canvasManager.clearAll();

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

        // 横線
        drawLine(gridCtx, {
            points: [timeColumnWidth, y, width, y],
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });

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

    // 予約ブロックを描画
    if (state.appointments && state.appointments.length > 0) {
        const filteredAppointments = state.appointments.filter(appt => {
            const apptDate = new Date(appt.appointmentDate);
            const daysDiff = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));
            return daysDiff >= 0 && daysDiff < weekDays;
        });

        filteredAppointments.forEach((appt, index) => {
            const apptDate = new Date(appt.appointmentDate);
            const daysDiff = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));
            const startTime = parseFloat(appt.startTime || '9.0');
            const endTime = parseFloat(appt.endTime || '10.0');

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
    }
}


