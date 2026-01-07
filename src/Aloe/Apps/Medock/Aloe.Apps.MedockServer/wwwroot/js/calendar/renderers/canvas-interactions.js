/**
 * Canvas Interactions Handler
 * 
 * Canvas API用のインタラクション処理
 * クリック検出、ホバー、ドラッグ範囲選択
 */

import { getRenderState } from './canvas-render-state.js';
import { isToday, isDateInRange, parseDate, dateToString } from '../utils/date-utils.js';
import { CONFIG } from '../config.js';
import { drawRect } from '../utils/canvas-utils.js';

/**
 * 選択中のセルの枠線を描画（外部から呼び出し可能）
 * @param {object} canvasManager - CanvasManagerインスタンス
 * @param {object} state - アプリケーション状態
 */
export function drawSelectionBorder(canvasManager, state) {
    // canvasManagerが利用可能かチェック
    if (!canvasManager) {
        return;
    }

    const interactionCtx = canvasManager.getContext('interaction');
    if (!interactionCtx) {
        // トランジション中はコンテキストが取得できないことがあるのでスキップ
        return;
    }

    const renderState = getRenderState();
    const currentDateStr = `${state.currentDate.getFullYear()}-${String(state.currentDate.getMonth() + 1).padStart(2, '0')}-${String(state.currentDate.getDate()).padStart(2, '0')}`;

    if (state.currentView === 'year') {
        // 年間ビュー: 選択中の月の日付グリッド部分の枠線
        const currentMonth = state.currentDate.getMonth();
        const monthInfo = renderState.months ? renderState.months[currentMonth] : null;

        if (monthInfo && monthInfo.dayGridBounds) {
            drawRect(interactionCtx, {
                x: monthInfo.dayGridBounds.x,
                y: monthInfo.dayGridBounds.y,
                width: monthInfo.dayGridBounds.width,
                height: monthInfo.dayGridBounds.height,
                stroke: CONFIG.colors.today,
                strokeWidth: 2,
                cornerRadius: Math.min(2, monthInfo.dayGridBounds.width / 20, monthInfo.dayGridBounds.height / 20)
            });
        }
    } else {
        // 月間・週間ビュー: 選択中の日の枠線
        const cell = renderState.getCell(currentDateStr);

        if (cell) {
            drawRect(interactionCtx, {
                x: cell.x,
                y: cell.y,
                width: cell.width,
                height: cell.height,
                stroke: CONFIG.colors.today,
                strokeWidth: 2
            });
        }
    }
}

/**
 * インタラクションハンドラーを設定
 * @param {object} canvasManager - CanvasManagerインスタンス
 * @param {object} state - アプリケーション状態
 * @param {function} setState - 状態更新関数
 * @param {function} render - 再描画関数
 */
export function setupCanvasInteractions(canvasManager, state, setState, render) {
    const interactionCanvas = canvasManager.getCanvas('interaction');
    const contentCtx = canvasManager.getContext('content');
    const renderState = getRenderState();

    // ホバー中のセル情報
    let hoveredCell = null;
    let hoveredSlot = null;
    let selectedSlot = null;
    let slotDrawTimer = null;

    /**
     * マウス座標からHit Testを実行
     */
    function hitTest(x, y) {
        return renderState.hierarchicalHitTest(x, y);
    }

    /**
     * ホバーハイライトを描画
     */
    function drawHoverHighlight(cell) {
        if (!cell) return;

        // 一時的な描画なので、interactionレイヤーに描画
        const interactionCtx = canvasManager.getContext('interaction');
        canvasManager.clearLayer('interaction');

        // 選択中のセルの枠線を再描画
        drawSelectedBorder(interactionCtx);

        // ホバー中のセルが選択中でない場合のみホバーハイライトを表示
        const currentDateStr = `${state.currentDate.getFullYear()}-${String(state.currentDate.getMonth() + 1).padStart(2, '0')}-${String(state.currentDate.getDate()).padStart(2, '0')}`;
        if (cell.dateStr !== currentDateStr) {
            drawRect(interactionCtx, {
                x: cell.x,
                y: cell.y,
                width: cell.width,
                height: cell.height,
                stroke: CONFIG.colors.today,
                strokeWidth: 1,
                strokeDashArray: [4, 4],
                fill: 'transparent'
            });
        }
    }

    /**
     * 選択中のセルの枠線を描画（内部関数）
     */
    function drawSelectedBorder(ctx) {
        const renderState = getRenderState();
        const currentDateStr = `${state.currentDate.getFullYear()}-${String(state.currentDate.getMonth() + 1).padStart(2, '0')}-${String(state.currentDate.getDate()).padStart(2, '0')}`;

        if (state.currentView === 'year') {
            // 年間ビュー: 選択中の月の日付グリッド部分の枠線
            const currentMonth = state.currentDate.getMonth();
            const monthInfo = renderState.months ? renderState.months[currentMonth] : null;
            if (monthInfo && monthInfo.dayGridBounds) {
                drawRect(ctx, {
                    x: monthInfo.dayGridBounds.x,
                    y: monthInfo.dayGridBounds.y,
                    width: monthInfo.dayGridBounds.width,
                    height: monthInfo.dayGridBounds.height,
                    stroke: CONFIG.colors.today,
                    strokeWidth: 2,
                    cornerRadius: Math.min(2, monthInfo.dayGridBounds.width / 20, monthInfo.dayGridBounds.height / 20)
                });
            }
        } else {
            // 月間・週間ビュー: 選択中の日の枠線
            const cell = renderState.getCell(currentDateStr);
            if (cell) {
                drawRect(ctx, {
                    x: cell.x,
                    y: cell.y,
                    width: cell.width,
                    height: cell.height,
                    stroke: CONFIG.colors.today,
                    strokeWidth: 2
                });
            }
        }
    }

    /**
     * スロット描画タイマーを開始（選択状態が消えるのを防ぐ）
     */
    function startSlotDrawTimer() {
        if (slotDrawTimer) {
            clearInterval(slotDrawTimer);
        }
        slotDrawTimer = setInterval(() => {
            if (hoveredSlot || selectedSlot) {
                drawSlotHighlightAndSelection(hoveredSlot, selectedSlot);
            } else {
                clearInterval(slotDrawTimer);
                slotDrawTimer = null;
            }
        }, 100); // 100ms ごとに再描画
    }

    /**
     * スロット描画タイマーを停止
     */
    function stopSlotDrawTimer() {
        if (slotDrawTimer) {
            clearInterval(slotDrawTimer);
            slotDrawTimer = null;
        }
    }

    /**
     * スロットのホバー・選択状態を描画
     */
    function drawSlotHighlightAndSelection(hoveredSlot, selectedSlot) {
        const interactionCtx = canvasManager.getContext('interaction');
        canvasManager.clearLayer('interaction');

        // 選択状態を描画（実線）
        if (selectedSlot && selectedSlot.bounds) {
            const bounds = selectedSlot.bounds;
            interactionCtx.strokeStyle = '#3b82f6';
            interactionCtx.lineWidth = 3;
            interactionCtx.setLineDash([]);
            interactionCtx.strokeRect(bounds.x - 2, bounds.y - 2, bounds.width + 4, bounds.height + 4);
        }

        // ホバー状態を描画（点線、選択状態でなければ）
        if (hoveredSlot && hoveredSlot.bounds && (!selectedSlot || (selectedSlot.dateStr !== hoveredSlot.dateStr || selectedSlot.slotIndex !== hoveredSlot.slotIndex))) {
            const bounds = hoveredSlot.bounds;
            interactionCtx.strokeStyle = '#3b82f6';
            interactionCtx.lineWidth = 2;
            interactionCtx.setLineDash([4, 4]);
            interactionCtx.strokeRect(bounds.x - 1, bounds.y - 1, bounds.width + 2, bounds.height + 2);
            interactionCtx.setLineDash([]);
        }
    }

    /**
     * マウス移動ハンドラ
     */
    interactionCanvas.addEventListener('mousemove', (e) => {
        const coords = canvasManager.getCanvasCoordinates(e);
        const hitResult = hitTest(coords.x, coords.y);

        if (hitResult && hitResult.type === 'cell') {
            const cell = hitResult.data;
            
            // セルが変わった場合のみ更新
            if (!hoveredCell || hoveredCell.dateStr !== cell.dateStr) {
                hoveredCell = cell;
                drawHoverHighlight(cell);
                interactionCanvas.style.cursor = 'pointer';
            }

            // ドラッグ中の処理
            if (state.isDragging && state.dragStartDate) {
                const newRange = { start: state.dragStartDate, end: cell.dateStr };
                setState({ selectedDateRange: newRange });
            }
        } else if (hitResult && hitResult.type === 'bar') {
            // バーの上
            interactionCanvas.style.cursor = 'pointer';
        } else if (hitResult && hitResult.type === 'slot') {
            // 週間ビュー: スロットの上
            const slot = hitResult.data;

            // スロットが変わった場合のみカーソルを更新
            if (!hoveredSlot || hoveredSlot.dateStr !== slot.dateStr || hoveredSlot.slotIndex !== slot.slotIndex) {
                hoveredSlot = slot;
                // グレーアウトフラグを確認
                const flags = slot.stats.slotFlags ? slot.stats.slotFlags[slot.slotIndex] : 0;
                const isGrayed = (flags & 0b001) !== 0;
                interactionCanvas.style.cursor = isGrayed ? 'not-allowed' : 'pointer';
            }

            // 常に描画を更新（interaction レイヤーのクリア対策）
            drawSlotHighlightAndSelection(hoveredSlot, selectedSlot);
        } else {
            // 何もない場所
            if (hoveredCell) {
                hoveredCell = null;
                const interactionCtx = canvasManager.getContext('interaction');
                canvasManager.clearLayer('interaction');
                // 選択中の枠線を再描画
                drawSelectedBorder(interactionCtx);
                interactionCanvas.style.cursor = 'default';
            } else if (hoveredSlot) {
                hoveredSlot = null;
                drawSlotHighlightAndSelection(null, selectedSlot);
                interactionCanvas.style.cursor = 'default';
            }
        }
    });

    /**
     * マウスリーブハンドラ
     */
    interactionCanvas.addEventListener('mouseleave', () => {
        if (hoveredCell) {
            hoveredCell = null;
            const interactionCtx = canvasManager.getContext('interaction');
            canvasManager.clearLayer('interaction');
            // 選択中の枠線を再描画
            drawSelectedBorder(interactionCtx);
            interactionCanvas.style.cursor = 'default';
        } else if (hoveredSlot) {
            hoveredSlot = null;
            startSlotDrawTimer(); // timer を続ける（selectedSlot がある場合）
            drawSlotHighlightAndSelection(null, selectedSlot);
            interactionCanvas.style.cursor = 'default';
        }
    });

    /**
     * マウスダウンハンドラ
     */
    interactionCanvas.addEventListener('mousedown', (e) => {
        const coords = canvasManager.getCanvasCoordinates(e);
        const hitResult = hitTest(coords.x, coords.y);

        if (hitResult && hitResult.type === 'cell') {
            const cell = hitResult.data;
            const dateStr = cell.dateStr;

            // 年間カレンダーの場合はドラッグ処理をスキップ
            if (state.currentView === 'year') {
                return;
            }

            // ドラッグ開始
            setState({
                isDragging: true,
                dragStartDate: dateStr
            });
        }
    });

    /**
     * マウスアップハンドラ
     */
    interactionCanvas.addEventListener('mouseup', (e) => {
        if (state.isDragging) {
            if (state.selectedDateRange) {
                const range = state.selectedDateRange;
                // 範囲が有効な場合のみコールバックと confirmedDateRange を設定
                if (range.start !== range.end && state.dotNetRef) {
                    // 日付の順序を正規化
                    const start = parseDate(range.start);
                    const end = parseDate(range.end);
                    const normalizedRange = start <= end
                        ? { start: range.start, end: range.end }
                        : { start: range.end, end: range.start };
                    state.dotNetRef.invokeMethodAsync('OnDateRangeSelected', normalizedRange.start, normalizedRange.end);
                    // 確定した範囲を設定
                    setState({
                        confirmedDateRange: normalizedRange,
                        selectedDate: null,
                        lastClickTime: 0,
                        lastClickDate: null
                    });
                }
            }
            // 常に isDragging をリセット
            setState({ isDragging: false, dragStartDate: null, selectedDateRange: null });
        }
    });

    /**
     * クリックハンドラ
     */
    interactionCanvas.addEventListener('click', (e) => {
        const coords = canvasManager.getCanvasCoordinates(e);
        const hitResult = hitTest(coords.x, coords.y);

        if (!hitResult) return;

        if (hitResult.type === 'bar') {
            // バーがクリックされた
            const bar = hitResult.data;
            console.log('Bar clicked:', bar);
            if (state.dotNetRef && bar.data && bar.data.id) {
                state.dotNetRef.invokeMethodAsync('OnAppointmentClicked', bar.data.id);
            }
            return;
        }

        if (hitResult.type === 'slot') {
            // 週間ビュー: スロットがクリックされた
            const slot = hitResult.data;
            const { dateStr, slotIndex, stats, appointment } = slot;

            // スロットデータを取得
            const startMinutes = stats.slotStarts[slotIndex];
            const endMinutes = stats.slotEnds[slotIndex];
            const cap = stats.slotCaps[slotIndex];
            const count = stats.slotCounts[slotIndex];
            const flags = stats.slotFlags ? stats.slotFlags[slotIndex] : 0;
            const isGrayed = (flags & 0b001) !== 0;

            // 検証: グレーアウトされたスロットはスキップ
            if (isGrayed) {
                console.log('Slot is grayed out, ignoring click');
                return;
            }

            // ダブルクリック検出（同じスロットを300ms以内に2回クリック）
            const now = Date.now();
            const slotId = `${dateStr}-${slotIndex}`;
            const isDoubleClick = (now - state.lastClickTime < 300) && (state.lastClickDate === slotId);

            if (isDoubleClick) {
                console.log('Week slot double-clicked');
                setState({ lastClickTime: 0, lastClickDate: null });
                selectedSlot = null;
                stopSlotDrawTimer();
                drawSlotHighlightAndSelection(null, null);

                // Blazor側に通知
                if (state.dotNetRef) {
                    // 既存の予約がある場合は編集モード
                    if (appointment && appointment.id) {
                        console.log('Opening appointment for edit:', appointment.id);
                        state.dotNetRef.invokeMethodAsync('OnAppointmentClicked', appointment.id);
                    } else {
                        // 新規作成モード（既存フロー維持）
                        console.log('Opening new appointment dialog');
                        state.dotNetRef.invokeMethodAsync('OnWeekSlotClicked',
                            dateStr, startMinutes, endMinutes, cap, count);
                    }
                }
            } else {
                // シングルクリック：選択状態のみ
                console.log('Week slot selected:', { dateStr, startMinutes, endMinutes, cap, count });
                selectedSlot = slot;
                startSlotDrawTimer(); // 定期的に描画を続ける
                drawSlotHighlightAndSelection(hoveredSlot, selectedSlot);
                setState({
                    lastClickTime: now,
                    lastClickDate: slotId
                });
            }
            return;
        }

        if (hitResult.type === 'cell') {
            const cell = hitResult.data;
            const dateStr = cell.dateStr;
            const now = Date.now();
            const isDoubleClick = (now - state.lastClickTime < 300) && (state.lastClickDate === dateStr);
            const isShiftClick = e.shiftKey;
            const isYearView = state.currentView === 'year';

            // クリック時に日付を選択（共有要素トランジションは currentDate から自動計算）
            // 位置情報の保存は不要

            // 年間カレンダーの場合
            if (isYearView) {
                if (isDoubleClick) {
                    console.log('MedockCalendar: Double click detected', { dateStr, hasDotNetRef: !!state.dotNetRef });
                    setState({ lastClickTime: 0, lastClickDate: null });
                    // Blazor側で日詳細ダイアログを表示
                    if (state.dotNetRef) {
                        state.dotNetRef.invokeMethodAsync('ShowDayDetail', dateStr);
                    }
                } else {
                    // ワンクリックで日付を選択（月の枠線を更新）
                    const clickedDate = parseDate(dateStr);
                    setState({
                        lastClickTime: now,
                        lastClickDate: dateStr,
                        currentDate: clickedDate
                    });
                    if (state.dotNetRef) {
                        state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
                    }
                    render();
                }
                return;
            }

            // 月間カレンダー・週間カレンダーの処理
            if (isDoubleClick) {
                console.log('MedockCalendar: Double click detected', { dateStr, hasDotNetRef: !!state.dotNetRef });
                setState({ lastClickTime: 0, lastClickDate: null });
                // Blazor側で日詳細ダイアログを表示
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('ShowDayDetail', dateStr);
                }
            } else if (isShiftClick && state.selectedDate) {
                const range = { start: state.selectedDate, end: dateStr };
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateRangeSelected', range.start, range.end);
                }
                // Shift+クリックで範囲選択
                setState({
                    selectedDate: null,
                    lastClickTime: 0,
                    lastClickDate: null,
                    selectedDateRange: range,
                    confirmedDateRange: range.start !== range.end ? range : null
                });
                render();
            } else if (state.confirmedDateRange &&
                isDateInRange(dateStr, state.confirmedDateRange.start, state.confirmedDateRange.end)) {
                // 範囲選択内の日付をクリックした場合は範囲選択を解除し、その日付を選択
                const clickedDate = parseDate(dateStr);
                setState({
                    lastClickTime: now,
                    lastClickDate: dateStr,
                    selectedDate: dateStr,
                    currentDate: clickedDate,
                    selectedDateRange: null,
                    confirmedDateRange: null
                });
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
                }
                render();
            } else {
                // 日付をクリックして選択
                const clickedDate = parseDate(dateStr);
                setState({
                    lastClickTime: now,
                    lastClickDate: dateStr,
                    selectedDate: dateStr,
                    currentDate: clickedDate,
                    selectedDateRange: null,
                    confirmedDateRange: null
                });
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
                }
                render();
            }
        } else if (hitResult.type === 'month') {
            // 月ヘッダーがクリックされた（年間ビュー用）
            const month = hitResult.data;
            console.log('Month header clicked:', month);
            if (state.dotNetRef) {
                const year = state.currentDate.getFullYear();
                const monthIndex = typeof month === 'object' ? month.index : month;
                state.dotNetRef.invokeMethodAsync('OnMonthClicked', year, monthIndex + 1);
            }
        }
    });

    // ドキュメント全体のマウスアップ（ドラッグが canvas 外で終了した場合）
    document.addEventListener('mouseup', () => {
        if (state.isDragging) {
            setState({ isDragging: false, dragStartDate: null });
        }
    });
}


