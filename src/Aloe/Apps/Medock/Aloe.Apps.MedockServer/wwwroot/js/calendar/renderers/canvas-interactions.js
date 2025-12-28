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

        drawRect(interactionCtx, {
            x: cell.x,
            y: cell.y,
            width: cell.width,
            height: cell.height,
            stroke: CONFIG.colors.today,
            strokeWidth: 2,
            fill: 'transparent'
        });
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
        } else {
            // 何もない場所
            if (hoveredCell) {
                hoveredCell = null;
                canvasManager.clearLayer('interaction');
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
            canvasManager.clearLayer('interaction');
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
            // TODO: バーの詳細を表示
            return;
        }

        if (hitResult.type === 'cell') {
            const cell = hitResult.data;
            const dateStr = cell.dateStr;
            const now = Date.now();
            const isDoubleClick = (now - state.lastClickTime < 300) && (state.lastClickDate === dateStr);
            const isShiftClick = e.shiftKey;
            const isYearView = state.currentView === 'year';

            // 年間カレンダーの場合は日付選択と範囲選択をスキップし、ダブルクリックのみ処理
            if (isYearView) {
                if (isDoubleClick) {
                    console.log('MedockCalendar: Double click detected', { dateStr, hasDotNetRef: !!state.dotNetRef });
                    setState({ lastClickTime: 0, lastClickDate: null });
                    // Blazor側で日詳細ダイアログを表示
                    if (state.dotNetRef) {
                        state.dotNetRef.invokeMethodAsync('ShowDayDetail', dateStr);
                    }
                } else {
                    // ダブルクリック判定用に時刻と日付を記録
                    setState({
                        lastClickTime: now,
                        lastClickDate: dateStr
                    });
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
                render();
            } else {
                // 同じ日付をクリックした場合は選択解除
                if (state.selectedDate === dateStr) {
                    setState({
                        lastClickTime: now,
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
        } else if (hitResult.type === 'month') {
            // 月ヘッダーがクリックされた（年間ビュー用）
            const month = hitResult.data;
            console.log('Month header clicked:', month);
            // TODO: 月ヘッダーのクリック処理
        }
    });

    // ドキュメント全体のマウスアップ（ドラッグが canvas 外で終了した場合）
    document.addEventListener('mouseup', () => {
        if (state.isDragging) {
            setState({ isDragging: false, dragStartDate: null });
        }
    });
}


