/**
 * Week View Renderer
 *
 * 週スケジューラー表示（時間軸カレンダー）
 * - スロット表示モード: 予約ブロックを表示
 * - 詳細表示モード: アバター（患者イニシャル）を表示してドラッグ&ドロップ可能
 */

import { getState } from '../state.js';
import { CONFIG } from '../config.js';
import { dateToString, parseDate, isToday, getStartOfWeek } from '../utils/date-utils.js';
import { showTooltip, hideTooltip } from '../ui/tooltip.js';

/**
 * ステータスコードからテキストを取得
 * @param {number} status - ステータスコード
 * @returns {string} ステータステキスト
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
 * @param {string} name - 患者名
 * @returns {string} イニシャル
 */
function getInitial(name) {
    if (!name) return '?';
    // スペースで分割して苗字を取得
    const parts = name.split(/[\s　]/);
    return parts[0].charAt(0);
}

/**
 * アバターの色パレット
 */
const avatarColors = [
    '#3b82f6', // blue
    '#10b981', // green
    '#f59e0b', // amber
    '#ef4444', // red
    '#8b5cf6', // purple
    '#ec4899', // pink
    '#06b6d4', // cyan
    '#f97316', // orange
];

/**
 * 週スケジューラーを描画
 */
export function renderWeekView() {
    const state = getState();
    const { stage, layers, currentDate, appointments, options } = state;
    const width = stage.width();
    const height = stage.height();

    layers.grid.destroyChildren();
    layers.content.destroyChildren();
    layers.interaction.destroyChildren();

    const { weekDays, startHour, endHour } = options;
    const hours = endHour - startHour;
    const timeColumnWidth = 60;
    const headerHeight = 50;
    const dayWidth = (width - timeColumnWidth) / weekDays;
    const hourHeight = (height - headerHeight) / hours;

    const startDate = getStartOfWeek(currentDate);

    // Header background
    const headerBg = new Konva.Rect({
        x: 0,
        y: 0,
        width: width,
        height: headerHeight,
        fill: '#f9fafb',
        stroke: CONFIG.colors.grid,
        strokeWidth: 1
    });
    layers.grid.add(headerBg);

    // Time column header
    const timeHeader = new Konva.Rect({
        x: 0,
        y: 0,
        width: timeColumnWidth,
        height: headerHeight,
        fill: '#f3f4f6',
        stroke: CONFIG.colors.grid,
        strokeWidth: 1
    });
    layers.grid.add(timeHeader);

    // Day headers
    const dayNames = ['日', '月', '火', '水', '木', '金', '土'];
    for (let i = 0; i < weekDays; i++) {
        const date = new Date(startDate);
        date.setDate(date.getDate() + i);
        const dayOfWeek = date.getDay();
        const dateStr = dateToString(date);
        const x = timeColumnWidth + i * dayWidth;

        // Header cell
        const headerCell = new Konva.Rect({
            x: x,
            y: 0,
            width: dayWidth,
            height: headerHeight,
            fill: isToday(date) ? 'rgba(16, 185, 129, 0.1)' : '#f9fafb',
            stroke: CONFIG.colors.grid,
            strokeWidth: 1
        });
        layers.grid.add(headerCell);

        // Day name
        const dayText = new Konva.Text({
            x: x,
            y: 8,
            width: dayWidth,
            text: dayNames[dayOfWeek],
            fontSize: CONFIG.font.sizeMedium,
            fontFamily: CONFIG.font.family,
            fill: dayOfWeek === 0 ? CONFIG.colors.weekend.sun :
                dayOfWeek === 6 ? CONFIG.colors.weekend.sat : '#6b7280',
            align: 'center'
        });
        layers.grid.add(dayText);

        // Date number
        const dateText = new Konva.Text({
            x: x,
            y: 26,
            width: dayWidth,
            text: `${date.getMonth() + 1}/${date.getDate()}`,
            fontSize: CONFIG.font.sizeLarge,
            fontFamily: CONFIG.font.family,
            fontStyle: isToday(date) ? 'bold' : 'normal',
            fill: dayOfWeek === 0 ? CONFIG.colors.weekend.sun :
                dayOfWeek === 6 ? CONFIG.colors.weekend.sat : '#374151',
            align: 'center'
        });
        layers.grid.add(dateText);
    }

    // Time slots and grid
    for (let h = 0; h < hours; h++) {
        const hour = startHour + h;
        const y = headerHeight + h * hourHeight;

        // Time label
        const timeLabel = new Konva.Text({
            x: 5,
            y: y + 5,
            text: `${String(hour).padStart(2, '0')}:00`,
            fontSize: CONFIG.font.sizeMedium,
            fontFamily: CONFIG.font.family,
            fill: '#6b7280'
        });
        layers.grid.add(timeLabel);

        // Hour row background
        const rowBg = new Konva.Rect({
            x: timeColumnWidth,
            y: y,
            width: width - timeColumnWidth,
            height: hourHeight,
            fill: h % 2 === 0 ? 'white' : '#fafafa',
            stroke: CONFIG.colors.grid,
            strokeWidth: 0.5
        });
        layers.grid.add(rowBg);

        // Day column separators and interaction areas
        for (let i = 0; i < weekDays; i++) {
            const x = timeColumnWidth + i * dayWidth;
            const date = new Date(startDate);
            date.setDate(date.getDate() + i);
            const dateStr = dateToString(date);

            // Column separator
            const colSep = new Konva.Line({
                points: [x, y, x, y + hourHeight],
                stroke: CONFIG.colors.grid,
                strokeWidth: 1
            });
            layers.grid.add(colSep);

            // Interaction area
            const hitArea = new Konva.Rect({
                x: x,
                y: y,
                width: dayWidth,
                height: hourHeight,
                fill: 'transparent'
            });
            hitArea.on('mouseenter', function () {
                hitArea.fill(CONFIG.colors.hover);
                layers.interaction.batchDraw();
            });
            hitArea.on('mouseleave', function () {
                hitArea.fill('transparent');
                layers.interaction.batchDraw();
            });
            hitArea.on('click', function () {
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnCreateRequested', dateStr, `${String(hour).padStart(2, '0')}:00`);
                }
            });
            layers.interaction.add(hitArea);
        }
    }

    // Render appointments
    renderAppointments(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate);

    layers.grid.batchDraw();
    layers.content.batchDraw();
    layers.interaction.batchDraw();
}

/**
 * 予約を描画（モードに応じて分岐）
 * @param {number} timeColumnWidth - 時間列の幅
 * @param {number} headerHeight - ヘッダーの高さ
 * @param {number} dayWidth - 日列の幅
 * @param {number} hourHeight - 時間行の高さ
 * @param {Date} startDate - 表示開始日
 */
function renderAppointments(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate) {
    const state = getState();
    const { layers, appointments, options } = state;
    const { startHour, weekDays, showSlots } = options;

    if (showSlots) {
        // スロット表示モード: タイムスロット枠を表示
        renderSlotMode(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate);
    } else {
        // 詳細表示モード: アバター表示
        renderDetailMode(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate);
    }
}

/**
 * スロット表示モード（予約ブロックを表示）
 * @param {number} timeColumnWidth - 時間列の幅
 * @param {number} headerHeight - ヘッダーの高さ
 * @param {number} dayWidth - 日列の幅
 * @param {number} hourHeight - 時間行の高さ
 * @param {Date} startDate - 表示開始日
 */
function renderSlotMode(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate) {
    const state = getState();
    const { layers, appointments, options } = state;
    const { startHour, weekDays } = options;

    // 30分単位のスロットを表示
    const slotMinutes = 30;
    const slotHeight = hourHeight / 2;
    const maxPerSlot = 4; // 1スロットあたり最大4人

    appointments.forEach(appt => {
        const apptDate = parseDate(appt.date);
        const dayIndex = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));

        if (dayIndex < 0 || dayIndex >= weekDays) return;

        const startParts = appt.startTime.split(':');
        const endParts = appt.endTime.split(':');
        const startHourNum = parseInt(startParts[0]);
        const startMin = parseInt(startParts[1]) || 0;
        const endHourNum = parseInt(endParts[0]);
        const endMin = parseInt(endParts[1]) || 0;

        const startOffset = (startHourNum - startHour) + (startMin / 60);
        const duration = (endHourNum - startHourNum) + ((endMin - startMin) / 60);

        const x = timeColumnWidth + dayIndex * dayWidth + 2;
        const y = headerHeight + startOffset * hourHeight;
        const w = Math.max(0, dayWidth - 4);
        const h = Math.max(0, duration * hourHeight - 2);

        // サイズが小さすぎる場合はスキップ
        if (w < 4 || h < 4) return;

        const statusColor = CONFIG.colors.status[appt.status] || CONFIG.colors.status[0];
        const blockCornerRadius = Math.max(0, Math.min(4, w / 2, h / 2));

        // Appointment block
        const block = new Konva.Rect({
            x: x,
            y: y,
            width: w,
            height: h,
            fill: statusColor,
            opacity: 0.9,
            cornerRadius: blockCornerRadius,
            shadowColor: 'black',
            shadowBlur: 2,
            shadowOpacity: 0.2,
            shadowOffsetY: 1
        });
        layers.content.add(block);

        // Patient name
        const nameText = new Konva.Text({
            x: x + 4,
            y: y + 4,
            width: w - 8,
            text: appt.patientName || '未設定',
            fontSize: CONFIG.font.sizeMedium,
            fontFamily: CONFIG.font.family,
            fontStyle: 'bold',
            fill: '#1f2937',
            ellipsis: true
        });
        layers.content.add(nameText);

        // Organization name
        if (appt.orgName && h > 35) {
            const orgText = new Konva.Text({
                x: x + 4,
                y: y + 20,
                width: w - 8,
                text: appt.orgName,
                fontSize: CONFIG.font.sizeSmall,
                fontFamily: CONFIG.font.family,
                fill: '#4b5563',
                ellipsis: true
            });
            layers.content.add(orgText);
        }

        // Appointment interaction
        const apptHitArea = new Konva.Rect({
            x: x,
            y: y,
            width: w,
            height: h,
            fill: 'transparent'
        });
        apptHitArea.on('mouseenter', function (e) {
            block.opacity(1);
            block.shadowBlur(4);
            layers.content.batchDraw();
            showTooltip(e.evt.clientX, e.evt.clientY,
                `<strong>${appt.patientName || '未設定'}</strong><br>` +
                `${appt.orgName || ''}<br>` +
                `${appt.startTime} - ${appt.endTime}<br>` +
                `ステータス: ${getStatusText(appt.status)}`);
        });
        apptHitArea.on('mouseleave', function () {
            block.opacity(0.9);
            block.shadowBlur(2);
            layers.content.batchDraw();
            hideTooltip();
        });
        apptHitArea.on('click', function () {
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnAppointmentClicked', appt.id);
            }
        });
        layers.interaction.add(apptHitArea);
    });
}

/**
 * 詳細表示モード（アバター表示でドラッグ&ドロップ可能）
 * @param {number} timeColumnWidth - 時間列の幅
 * @param {number} headerHeight - ヘッダーの高さ
 * @param {number} dayWidth - 日列の幅
 * @param {number} hourHeight - 時間行の高さ
 * @param {Date} startDate - 表示開始日
 */
function renderDetailMode(timeColumnWidth, headerHeight, dayWidth, hourHeight, startDate) {
    const state = getState();
    const { layers, appointments, options } = state;
    const { startHour, weekDays } = options;

    // スロットごとに予約をグループ化（30分単位）
    const slotMap = new Map(); // key: "dayIndex-hour-half" -> appointments[]

    appointments.forEach(appt => {
        const apptDate = parseDate(appt.date);
        const dayIndex = Math.floor((apptDate - startDate) / (1000 * 60 * 60 * 24));

        if (dayIndex < 0 || dayIndex >= weekDays) return;

        const startParts = appt.startTime.split(':');
        const hourNum = parseInt(startParts[0]);
        const minNum = parseInt(startParts[1]) || 0;
        const half = minNum >= 30 ? 1 : 0;
        const slotKey = `${dayIndex}-${hourNum}-${half}`;

        if (!slotMap.has(slotKey)) {
            slotMap.set(slotKey, []);
        }
        slotMap.get(slotKey).push(appt);
    });

    // 各スロットにアバターを描画
    slotMap.forEach((slotAppts, slotKey) => {
        const [dayIndex, hourNum, half] = slotKey.split('-').map(Number);
        const slotHeight = hourHeight / 2;
        const avatarSize = Math.min(28, slotHeight - 4, (dayWidth - 8) / Math.min(slotAppts.length, 4) - 4);

        const baseX = timeColumnWidth + dayIndex * dayWidth + 4;
        const baseY = headerHeight + (hourNum - startHour) * hourHeight + half * slotHeight + (slotHeight - avatarSize) / 2;

        slotAppts.forEach((appt, idx) => {
            if (idx >= 4) return; // 1スロット最大4人まで表示

            const avatarX = baseX + idx * (avatarSize + 4);
            const avatarY = baseY;
            const initial = getInitial(appt.patientName);
            const colorIdx = appt.patientName ? appt.patientName.charCodeAt(0) % avatarColors.length : 0;
            const avatarColor = avatarColors[colorIdx];

            // アバター円
            const avatar = new Konva.Circle({
                x: avatarX + avatarSize / 2,
                y: avatarY + avatarSize / 2,
                radius: avatarSize / 2,
                fill: avatarColor,
                stroke: 'white',
                strokeWidth: 2,
                shadowColor: 'rgba(0,0,0,0.3)',
                shadowBlur: 3,
                shadowOffsetY: 1,
                draggable: true
            });
            avatar.id(appt.id);

            // イニシャル文字
            const initialText = new Konva.Text({
                x: avatarX,
                y: avatarY + avatarSize / 2 - 7,
                width: avatarSize,
                text: initial,
                fontSize: 14,
                fontFamily: CONFIG.font.family,
                fontStyle: 'bold',
                fill: 'white',
                align: 'center',
                listening: false
            });

            // グループにまとめてドラッグ可能に
            const group = new Konva.Group({
                x: 0,
                y: 0,
                draggable: true
            });
            group.add(avatar);
            group.add(initialText);
            group.id(appt.id);

            // ホバー時のハイライト
            group.on('mouseenter', function (e) {
                avatar.stroke('#fbbf24');
                avatar.strokeWidth(3);
                avatar.shadowBlur(6);
                layers.content.batchDraw();
                document.body.style.cursor = 'pointer';
                showTooltip(e.evt.clientX, e.evt.clientY,
                    `<strong>${appt.patientName || '未設定'}</strong><br>` +
                    `${appt.orgName || ''}<br>` +
                    `${appt.startTime} - ${appt.endTime}<br>` +
                    `ステータス: ${getStatusText(appt.status)}`);
            });

            group.on('mouseleave', function () {
                avatar.stroke('white');
                avatar.strokeWidth(2);
                avatar.shadowBlur(3);
                layers.content.batchDraw();
                document.body.style.cursor = 'default';
                hideTooltip();
            });

            // クリックで選択
            group.on('click', function () {
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnAppointmentClicked', appt.id);
                }
            });

            // ドラッグ開始時
            group.on('dragstart', function () {
                avatar.shadowBlur(8);
                avatar.shadowColor('rgba(0,0,0,0.5)');
                group.moveToTop();
                hideTooltip();
            });

            // ドラッグ終了時（ドロップ位置から新しい日時を計算）
            group.on('dragend', function () {
                avatar.shadowBlur(3);
                avatar.shadowColor('rgba(0,0,0,0.3)');

                const pos = group.position();
                const dropX = avatarX + avatarSize / 2 + pos.x;
                const dropY = avatarY + avatarSize / 2 + pos.y;

                // ドロップ位置から日付と時間を計算
                const newDayIndex = Math.floor((dropX - timeColumnWidth) / dayWidth);
                const newHourOffset = (dropY - headerHeight) / hourHeight;
                const newHour = Math.floor(startHour + newHourOffset);
                const newMin = (newHourOffset % 1) >= 0.5 ? 30 : 0;

                if (newDayIndex >= 0 && newDayIndex < weekDays && newHour >= startHour && newHour < options.endHour) {
                    const newDate = new Date(startDate);
                    newDate.setDate(newDate.getDate() + newDayIndex);
                    const newDateStr = dateToString(newDate);
                    const newTimeStr = `${String(newHour).padStart(2, '0')}:${String(newMin).padStart(2, '0')}`;

                    // ドラッグ＆ドロップ完了をサーバーに通知
                    if (state.dotNetRef) {
                        state.dotNetRef.invokeMethodAsync('OnAppointmentMoved', appt.id, newDateStr, newTimeStr);
                    }
                }

                // 元の位置に戻す（サーバー側で更新後にリフレッシュされる）
                group.position({ x: 0, y: 0 });
                layers.content.batchDraw();
            });

            layers.content.add(group);
        });
    });
}
