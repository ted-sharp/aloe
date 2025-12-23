/**
 * Interaction Handlers
 *
 * 日付セルのインタラクション処理（クリック、ドラッグ、ホバー）
 */

import { getState, setState } from '../../state.js';
import { CONFIG } from '../../config.js';
import { isToday, isDateInRange } from '../../utils/date-utils.js';
import { showDayModal } from '../../ui/modal.js';
import { buildModalContent } from './modal-content.js';

/**
 * インタラクションエリア（hitArea）を作成してイベントハンドラを設定
 * @param {object} params - パラメータ
 * @param {number} params.cellLeft - セルの左端X座標
 * @param {number} params.cellTop - セルの上端Y座標
 * @param {number} params.cellWidth - セル幅
 * @param {number} params.cellHeight - セル高さ
 * @param {string} params.dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {Array|null} params.slots - 時間帯枠データ
 * @param {object|null} params.bgRect - 背景矩形（ホバー時の枠線用）
 * @returns {Konva.Rect} hitAreaオブジェクト
 */
export function createInteractionArea({ cellLeft, cellTop, cellWidth, cellHeight, dateStr, slots, bgRect }) {
    const state = getState();
    const { layers } = state;

    // インタラクションエリア
    const hitArea = new Konva.Rect({
        x: cellLeft,
        y: cellTop,
        width: cellWidth,
        height: cellHeight,
        fill: 'transparent'
    });

    hitArea.on('mouseenter', function (e) {
        // ホバー時は枠線ハイライトのみ（ツールチップは廃止）
        if (bgRect) {
            bgRect.stroke(CONFIG.colors.today);
            bgRect.strokeWidth(2);
            layers.content.batchDraw();
        }
    });

    hitArea.on('mouseleave', function () {
        if (!state.isDragging && bgRect) {
            // ホバー解除後は枠線を削除（今日の日付のみ枠線を残す）
            if (isToday(dateStr)) {
                bgRect.stroke(CONFIG.colors.today);
                bgRect.strokeWidth(2);
            } else {
                // 選択状態に関わらず、ホバー解除後は枠線を削除
                bgRect.stroke(null);
                bgRect.strokeWidth(0);
            }
            layers.content.batchDraw();
        }
    });

    // クリックハンドラ（ダブルクリック・Shift+クリック対応）
    hitArea.on('click', function (e) {
        console.log('MedockCalendar: hitArea click event fired', { dateStr, cellWidth, cellHeight });
        const now = Date.now();
        const isDoubleClick = (now - state.lastClickTime < 300) && (state.lastClickDate === dateStr);
        const isShiftClick = e.evt.shiftKey;
        const isYearView = state.currentView === 'year';

        // 年間カレンダーの場合は日付選択と範囲選択をスキップし、ダブルクリックのみ処理
        if (isYearView) {
            if (isDoubleClick) {
                console.log('MedockCalendar: Double click detected', { dateStr, hasDotNetRef: !!state.dotNetRef });
                setState({ lastClickTime: 0, lastClickDate: null });
                // モーダルダイアログを表示
                const modalContent = buildModalContent(dateStr, slots, state);
                showDayModal(dateStr, modalContent, state.dotNetRef);
            } else {
                // ダブルクリック判定用に時刻と日付を記録
                setState({
                    lastClickTime: now,
                    lastClickDate: dateStr
                });
            }
            return;
        }

        // 月間カレンダー・週間カレンダーの既存処理
        if (isDoubleClick) {
            console.log('MedockCalendar: Double click detected', { dateStr, hasDotNetRef: !!state.dotNetRef });
            setState({ lastClickTime: 0, lastClickDate: null });
            // モーダルダイアログを表示
            const modalContent = buildModalContent(dateStr, slots, state);
            showDayModal(dateStr, modalContent, state.dotNetRef);
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
            // 範囲選択内の日付をクリックした場合は範囲選択を解除し、その日付を選択
            setState({
                lastClickTime: now,      // ダブルクリック判定用に時刻を記録
                lastClickDate: dateStr,
                selectedDate: dateStr,   // クリックした日付を選択状態にする
                selectedDateRange: null,
                confirmedDateRange: null
            });
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
            }
            // カレンダーを再描画してグレーアウトを解除
            if (window.MedockCalendar) {
                window.MedockCalendar.render();
            }
        } else {
            // 同じ日付をクリックした場合は選択解除（だがダブルクリック判定用に時刻は記録）
            if (state.selectedDate === dateStr) {
                setState({
                    lastClickTime: now,  // ダブルクリック判定用に時刻を記録
                    lastClickDate: dateStr,
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
        // 年間カレンダーの場合はドラッグ処理をスキップ
        if (state.currentView === 'year') {
            return;
        }
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
        // 年間カレンダーの場合はドラッグ処理をスキップ
        if (state.currentView === 'year') {
            return;
        }
        if (state.isDragging && state.dragStartDate) {
            // 実際にドラッグが開始された場合のみ selectedDateRange を設定
            setState({ selectedDateRange: { start: state.dragStartDate, end: dateStr } });
        }
    });

    return hitArea;
}

