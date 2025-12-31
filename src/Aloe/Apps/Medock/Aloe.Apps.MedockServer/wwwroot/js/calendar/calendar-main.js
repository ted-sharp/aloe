/**
 * Medock Calendar Main Entry Point
 *
 * ES Module統合とPublic API公開
 * window.MedockCalendar として公開され、Blazor JSInterop から呼び出される
 */

import { CONFIG } from './config.js';
import { getState, setState, resetState } from './state.js';
import { dateToString, parseDate } from './utils/date-utils.js';
import { createTooltip, hideTooltip } from './ui/tooltip.js';
import { createLayers } from './ui/layers.js';
import { createCanvasManager } from './ui/canvas-manager.js';
import { renderCanvasMonthView } from './renderers/canvas-month-view.js';
import { renderCanvasYearView } from './renderers/canvas-year-view.js';
import { renderCanvasWeekView } from './renderers/canvas-week-view.js';
import { renderCanvasDayDetail } from './renderers/canvas-day-detail.js';
import { setupCanvasInteractions, drawSelectionBorder } from './renderers/canvas-interactions.js';
import { getRenderState } from './renderers/canvas-render-state.js';
import { startConnection, stopConnection } from './realtime/signalr-client.js';

// Blazor DayDetailPopup用のステージを管理
const dayDetailPopupStages = new Map();

/**
 * Initialize the calendar canvas
 * @param {string} containerId - DOM container ID
 * @param {object} data - Initial data { appointments, mainStats, holidays }
 * @param {object} options - Configuration options
 * @param {object} dotNetRef - .NET object reference for callbacks
 * @param {string} currentDateStr - Initial date string 'YYYY-MM-DD' (from Blazor)
 */
function init(containerId, data, options, dotNetRef, currentDateStr) {
    const state = getState();

    // dotNetRefは常に最新のものを保持（複数のCalendarCanvasインスタンスが存在する場合に対応）
    // console.log('MedockCalendar: init called', {
    //     containerId,
    //     hasDotNetRef: !!dotNetRef,
    //     hasExistingCanvasManager: !!state.canvasManager,
    //     existingContainerId: state.containerId,
    //     currentDateStr
    // });

    // Blazor側から渡された日付を使用（渡されなければ今日）
    const initialDate = currentDateStr ? parseDate(currentDateStr) : new Date();
    const initialDateStr = currentDateStr || dateToString(new Date());

    setState({
        containerId,
        dotNetRef,  // 常に最新のdotNetRefを設定
        options: { ...state.options, ...options },
        currentDate: initialDate,
        selectedDate: initialDateStr
    });
    
    // 既に初期化されている場合は、データとビューの更新のみ
    if (state.canvasManager) {
        // console.log('MedockCalendar: Already initialized, updating data and view');
        // 既存のコンテナを更新（必要に応じて）
        const container = document.getElementById(containerId);
        if (container && state.containerId !== containerId) {
            // 新しいコンテナにcanvasを移動
            const oldContainer = document.getElementById(state.containerId);
            if (oldContainer) {
                oldContainer.innerHTML = '';
            }
            // 新しいコンテナでCanvas Managerを再初期化
            destroy();
            // 次の処理で新しいCanvas Managerを作成
        } else {
            // データとビューを更新
            if (data) {
                updateData(data);
            }
            return;
        }
    }

    const container = document.getElementById(containerId);
    if (!container) {
        console.error('MedockCalendar: Container not found:', containerId);
        return;
    }

    // Create Canvas Manager
    const canvasManager = createCanvasManager(
        containerId,
        container.clientWidth,
        container.clientHeight || 600
    );

    setState({ 
        canvasManager
    });
    createLayers();
    createTooltip();
    
    // インタラクションハンドラを設定
    setupCanvasInteractions(canvasManager, getState(), setState, render);

    // Initialize SignalR connection for real-time updates
    startConnection(async (updatedDate, updatedResourceIds, diffData) => {
        // console.log('Stats updated, refreshing calendar:', { updatedDate, updatedResourceIds });
        // データが更新されたら、カレンダーを再描画
        // 実際の実装では、diffDataを使用して部分更新を行う
        const state = getState();
        if (state.mainStats) {
            // 差分データを既存のdataにマージ
            // ここでは簡易的に全体を再描画
            render();
        }
    });

    // Set initial data
    if (data) {
        updateData(data);
    }

    // リサイズはCanvasManagerが自動的に処理するため、renderのみ呼び出す
    // CanvasManager内のResizeObserverがresize()を呼び出すので、ここで再描画だけ実行
    // 注: CanvasManager内のResizeObserverとは別に、render呼び出し用のResizeObserverを設定
    const resizeObserver = new ResizeObserver(entries => {
        for (let entry of entries) {
            render();
        }
    });
    resizeObserver.observe(container);
    setState({ resizeObserver });

    // イベントハンドラはsetupCanvasInteractionsで設定されるため、ここでは不要

    // Initial render
    render();
}

/**
 * Update calendar data
 * @param {object} data - { appointments: [], mainStats: {}, equipmentStats: {}, holidays: {} }
 */
function updateData(data) {
    if (data.appointments) {
        setState({ appointments: data.appointments });
    }

    if (data.mainStats) {
        setState({ mainStats: new Map(Object.entries(data.mainStats)) });
    }

    if (data.equipmentStats) {
        setState({ equipmentStats: new Map(Object.entries(data.equipmentStats)) });
    }

    if (data.holidays) {
        setState({ holidays: new Map(Object.entries(data.holidays)) });
    }

    render();
}

/**
 * Change the current view
 * @param {string} viewType - 'year', 'month', 'week'
 * @param {string} dateStr - Optional date string 'YYYY-MM-DD'
 */
function changeView(viewType, dateStr) {
    const updates = { currentView: viewType };
    if (dateStr) {
        updates.currentDate = parseDate(dateStr);
    }
    setState(updates);
    render();
}

/**
 * Set week scheduler options
 * @param {object} options - { weekDays: number, showSlots: boolean, startHour: number, endHour: number }
 */
function setOptions(options) {
    const state = getState();
    setState({ options: { ...state.options, ...options } });
    render();
}

/**
 * Navigate to a specific date
 * @param {string} dateStr - Date string 'YYYY-MM-DD'
 */
function navigateTo(dateStr) {
    setState({ currentDate: parseDate(dateStr) });
    render();
}

/**
 * Render the current view
 */
function render() {
    const state = getState();
    if (!state.canvasManager) return;

    const currentRenderState = getRenderState();

    // ビュータイプが変わったかどうかを判定
    const viewChanged = state.previousView && state.previousView !== state.currentView;
    let fadeMode = viewChanged ? 'scalefade' : 'crossfade';
    let fadeDuration = viewChanged ? 500 : 200;

    // 共有要素トランジションの判定（currentDate から自動計算）
    let transitionInfo = null;
    if (viewChanged) {
        const currentDateStr = dateToString(state.currentDate);
        const currentYear = state.currentDate.getFullYear();
        const currentMonth = state.currentDate.getMonth();

        // console.log('SharedElement Debug:', {
        //     viewChanged,
        //     currentDateStr,
        //     currentView: state.currentView,
        //     previousView: state.previousView,
        //     currentYear,
        //     currentMonth
        // });

        // 年 ↔ 月 のトランジション
        if ((state.previousView === 'year' && state.currentView === 'month') ||
            (state.previousView === 'month' && state.currentView === 'year')) {

            fadeMode = 'sharedelement';
            transitionInfo = {
                targetYear: currentYear,
                targetMonth: currentMonth,
                transitionType: `${state.previousView}-to-${state.currentView}`
            };

            // 遷移前のビューからsourceBoundsを取得
            if (state.previousView === 'year' && currentRenderState.months[currentMonth]) {
                // 年 → 月: 年間ビューの月boundsを保存
                transitionInfo.sourceBounds = currentRenderState.months[currentMonth].bounds;
                // console.log('Captured Year month bounds:', transitionInfo.sourceBounds);
            } else if (state.previousView === 'month') {
                // 月 → 年: 月間ビュー全体のboundsを保存
                transitionInfo.sourceBounds = {
                    x: 0,
                    y: 0,
                    width: state.canvasManager.width,
                    height: state.canvasManager.height
                };
                // console.log('Captured Month full bounds:', transitionInfo.sourceBounds);
            }

            // console.log('SharedElement Transition ENABLED (Year-Month)', transitionInfo);
        }
        // 月 ↔ 週 のトランジション（フェードスルー）
        else if ((state.previousView === 'month' && state.currentView === 'week') ||
                 (state.previousView === 'week' && state.currentView === 'month')) {

            // 週表示はゆっくりめのフェードスルー
            fadeMode = 'fadethrough';
            fadeDuration = 400;
            // console.log('Fadethrough Transition ENABLED (Month-Week)');
        }
        // 年 ↔ 週 のトランジション（フェードスルー）
        else if ((state.previousView === 'year' && state.currentView === 'week') ||
                 (state.previousView === 'week' && state.currentView === 'year')) {

            // 週表示はゆっくりめのフェードスルー
            fadeMode = 'fadethrough';
            fadeDuration = 400;
            // console.log('Fadethrough Transition ENABLED (Year-Week)');
        } else {
            // console.log('SharedElement Transition SKIPPED - not Year-Month or Month-Week transition');
        }
    } else {
        // console.log('SharedElement Transition SKIPPED - no view change');
    }

    // 前回のビューを記憶
    setState({ previousView: state.currentView });

    // CanvasManagerがリサイズを自動処理するため、サイズ調整は不要
    // 直接描画関数を呼び出す（フェードモード、時間、トランジション情報を渡す）
    switch (state.currentView) {
        case 'year':
            renderCanvasYearView(state.canvasManager, state, fadeMode, fadeDuration, transitionInfo);
            break;
        case 'month':
            renderCanvasMonthView(state.canvasManager, state, fadeMode, fadeDuration, transitionInfo);
            break;
        case 'week':
            renderCanvasWeekView(state.canvasManager, state, fadeMode, fadeDuration, transitionInfo);
            break;
        default:
            console.error('Unknown view type:', state.currentView);
    }

    // トランジション完了後に選択中の枠線を描画
    // fadeDuration + バッファで確実に完了後に描画
    // 長めの待ち時間を設定してトランジションの完全終了を待つ
    setTimeout(() => {
        drawSelectionBorder(state.canvasManager, state);
    }, fadeDuration + 100);
}

/**
 * Destroy the calendar instance
 */
function destroy() {
    const state = getState();
    hideTooltip();

    // ツールチップをDOMから削除
    if (state.tooltip && state.tooltip.parentNode) {
        state.tooltip.parentNode.removeChild(state.tooltip);
    }

    // SignalR接続を停止
    stopConnection();

    resetState();
}

/**
 * Blazor DayDetailPopup用: 指定コンテナにCanvas APIグラフを描画
 * @param {string} containerId - DOM container ID
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 */
function renderDayDetailPopup(containerId, dateStr) {
    const container = document.getElementById(containerId);
    if (!container) {
        console.error('MedockCalendar: DayDetailPopup container not found:', containerId);
        return;
    }

    // 既存のCanvas Managerがあれば破棄
    if (dayDetailPopupStages.has(containerId)) {
        const existingManager = dayDetailPopupStages.get(containerId);
        if (existingManager && existingManager.destroy) {
            existingManager.destroy();
        }
        dayDetailPopupStages.delete(containerId);
    }

    // コンテナのサイズを取得
    const rect = container.getBoundingClientRect();
    const width = Math.max(400, rect.width);
    const height = Math.max(300, rect.height);

    // Canvas Managerを作成
    const popupCanvasManager = createCanvasManager(containerId, width, height);

    // 現在のstateを取得
    const state = getState();

    // 日付から情報を取得
    const date = new Date(dateStr);
    const dayNumber = date.getDate();
    const isHoliday = state.holidays.has(dateStr);

    // Equipment統計データを取得
    const equipmentStats = state.equipmentStats.get(dateStr) || null;

    // グラフを描画（日詳細ポップアップでは日付テキストを表示しない）
    renderCanvasDayDetail(popupCanvasManager, state, dateStr, dayNumber, isHoliday, false, equipmentStats);

    // Canvas Managerを保存
    dayDetailPopupStages.set(containerId, popupCanvasManager);

    // リサイズ対応（CanvasManagerが自動でリサイズするため、再描画のみ）
    const resizeObserver = new ResizeObserver(entries => {
        for (let entry of entries) {
            const newWidth = Math.max(400, entry.contentRect.width);
            const newHeight = Math.max(300, entry.contentRect.height);

            if (popupCanvasManager.width !== newWidth || popupCanvasManager.height !== newHeight) {
                // Canvas Managerがリサイズを処理
                popupCanvasManager.resize(newWidth, newHeight);
                
                // Equipment統計データを再取得
                const equipmentStats = state.equipmentStats.get(dateStr) || null;
                
                // 再描画
                renderCanvasDayDetail(popupCanvasManager, state, dateStr, dayNumber, isHoliday, false, equipmentStats);
            }
        }
    });
    resizeObserver.observe(container);

    // クリーンアップ用にResizeObserverを保存
    popupCanvasManager._resizeObserver = resizeObserver;
}

/**
 * Blazor DayDetailPopup用: 指定コンテナのCanvas Managerを破棄
 * @param {string} containerId - DOM container ID
 */
function destroyDayDetailPopup(containerId) {
    if (dayDetailPopupStages.has(containerId)) {
        const canvasManager = dayDetailPopupStages.get(containerId);
        if (canvasManager._resizeObserver) {
            canvasManager._resizeObserver.disconnect();
        }
        canvasManager.destroy();
        dayDetailPopupStages.delete(containerId);
    }
}

/**
 * 指定日付の祝日名を取得
 * @param {string} dateStr - 日付文字列（YYYY-MM-DD形式）
 * @returns {string|null} 祝日名、祝日でない場合はnull
 */
function getHolidayName(dateStr) {
    const state = getState();
    return state.holidays?.get(dateStr) || null;
}

// Export Public API to global scope
window.MedockCalendar = {
    init,
    updateData,
    changeView,
    setOptions,
    navigateTo,
    render,
    destroy,
    renderDayDetailPopup,
    destroyDayDetailPopup,
    getHolidayName
};
