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
import { renderYearView } from './renderers/year-view.js';
import { renderMonthView } from './renderers/month-view.js';
import { renderWeekView } from './renderers/week-view.js';
import { renderDayDetailBarChart } from './renderers/bar-chart/index.js';

// Blazor DayDetailPopup用のステージを管理
const dayDetailPopupStages = new Map();

/**
 * Initialize the calendar canvas
 * @param {string} containerId - DOM container ID
 * @param {object} data - Initial data { appointments, mainStats, holidays }
 * @param {object} options - Configuration options
 * @param {object} dotNetRef - .NET object reference for callbacks
 */
function init(containerId, data, options, dotNetRef) {
    const state = getState();
    
    // dotNetRefは常に最新のものを保持（複数のCalendarCanvasインスタンスが存在する場合に対応）
    console.log('MedockCalendar: init called', { 
        containerId, 
        hasDotNetRef: !!dotNetRef,
        hasExistingStage: !!state.stage,
        existingContainerId: state.containerId
    });
    
    setState({
        containerId,
        dotNetRef,  // 常に最新のdotNetRefを設定
        options: { ...state.options, ...options }
    });
    
    // 既に初期化されている場合は、データとビューの更新のみ
    if (state.stage) {
        console.log('MedockCalendar: Already initialized, updating data and view');
        // 既存のstageのコンテナを更新（必要に応じて）
        const container = document.getElementById(containerId);
        if (container && state.stage.container()?.id !== containerId) {
            // 新しいコンテナにstageを移動
            const oldContainer = state.stage.container();
            if (oldContainer) {
                oldContainer.innerHTML = '';
            }
            state.stage.container(containerId);
        }
        // データとビューを更新
        if (data) {
            updateData(data);
        }
        return;
    }

    const container = document.getElementById(containerId);
    if (!container) {
        console.error('MedockCalendar: Container not found:', containerId);
        return;
    }

    // Create stage
    const stage = new Konva.Stage({
        container: containerId,
        width: container.clientWidth,
        height: container.clientHeight || 600
    });

    setState({ stage });
    createLayers();
    createTooltip();

    // Set initial data
    if (data) {
        updateData(data);
    }

    // Handle resize
    const resizeObserver = new ResizeObserver(entries => {
        for (let entry of entries) {
            const state = getState();
            state.stage.width(entry.contentRect.width);
            state.stage.height(entry.contentRect.height || 600);
            render();
        }
    });
    resizeObserver.observe(container);
    setState({ resizeObserver });

    // ドラッグ終了イベント（mouseup）をステージに追加
    stage.on('mouseup', function () {
        const state = getState();
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
                    // 確定した範囲を設定（グレーアウト判定に使用）、単一選択をクリア
                    setState({
                        confirmedDateRange: normalizedRange,
                        selectedDate: null,
                        lastClickTime: 0,
                        lastClickDate: null
                    });
                }
                // range.start === range.end の場合は何もしない（単一クリックとして扱い、既存の confirmedDateRange を保持）
            }
            // 常に isDragging と selectedDateRange をリセット
            setState({ isDragging: false, dragStartDate: null, selectedDateRange: null });
        }
    });

    // コンテナ外でのmouseupも処理
    document.addEventListener('mouseup', function () {
        const state = getState();
        if (state.isDragging) {
            setState({ isDragging: false, dragStartDate: null });
        }
    });

    // Initial render
    render();
}

/**
 * Update calendar data
 * @param {object} data - { appointments: [], mainStats: {}, holidays: {} }
 */
function updateData(data) {
    if (data.appointments) {
        setState({ appointments: data.appointments });
    }

    if (data.mainStats) {
        setState({ mainStats: new Map(Object.entries(data.mainStats)) });
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
    if (!state.stage) return;

    // Ensure stage size matches container
    const container = document.getElementById(state.containerId);
    if (container) {
        const width = container.clientWidth;
        const height = container.clientHeight || 600;
        if (state.stage.width() !== width || state.stage.height() !== height) {
            state.stage.width(width);
            state.stage.height(height);
        }
    }

    switch (state.currentView) {
        case 'year':
            renderYearView();
            break;
        case 'month':
            renderMonthView();
            break;
        case 'week':
            renderWeekView();
            break;
        default:
            console.error('Unknown view type:', state.currentView);
    }
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

    resetState();
}

/**
 * Blazor DayDetailPopup用: 指定コンテナにKonva.jsグラフを描画
 * @param {string} containerId - DOM container ID
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 */
function renderDayDetailPopup(containerId, dateStr) {
    const container = document.getElementById(containerId);
    if (!container) {
        console.error('MedockCalendar: DayDetailPopup container not found:', containerId);
        return;
    }

    // 既存のステージがあれば破棄
    if (dayDetailPopupStages.has(containerId)) {
        dayDetailPopupStages.get(containerId).destroy();
        dayDetailPopupStages.delete(containerId);
    }

    // コンテナのサイズを取得
    const rect = container.getBoundingClientRect();
    const width = Math.max(400, rect.width);
    const height = Math.max(300, rect.height);

    // Konva Stageを作成
    const popupStage = new Konva.Stage({
        container: containerId,
        width: width,
        height: height
    });

    // レイヤーを作成
    const popupLayers = {
        background: new Konva.Layer(),
        grid: new Konva.Layer(),
        content: new Konva.Layer(),
        interaction: new Konva.Layer()
    };

    popupStage.add(popupLayers.background);
    popupStage.add(popupLayers.grid);
    popupStage.add(popupLayers.content);
    popupStage.add(popupLayers.interaction);

    // 現在のstateを保存
    const state = getState();
    const originalLayers = state.layers;
    const originalStage = state.stage;

    try {
        // 一時的にstateをポップアップ用に設定
        setState({
            layers: popupLayers,
            stage: popupStage
        });

        // 日付から情報を取得
        const date = new Date(dateStr);
        const dayNumber = date.getDate();
        const isHoliday = state.holidays.has(dateStr);

        // グラフを描画
        renderDayDetailBarChart(0, 0, width, height, dateStr, dayNumber, isHoliday);

        // レイヤーを描画
        popupLayers.background.batchDraw();
        popupLayers.grid.batchDraw();
        popupLayers.content.batchDraw();
        popupLayers.interaction.batchDraw();

        // ステージを保存
        dayDetailPopupStages.set(containerId, popupStage);

        // リサイズ対応
        const resizeObserver = new ResizeObserver(entries => {
            for (let entry of entries) {
                const newWidth = Math.max(400, entry.contentRect.width);
                const newHeight = Math.max(300, entry.contentRect.height);

                if (popupStage.width() !== newWidth || popupStage.height() !== newHeight) {
                    popupStage.width(newWidth);
                    popupStage.height(newHeight);

                    // 現在のstateを再び保存
                    const currentOriginalLayers = getState().layers;
                    const currentOriginalStage = getState().stage;

                    try {
                        setState({
                            layers: popupLayers,
                            stage: popupStage
                        });

                        // レイヤーをクリアして再描画
                        popupLayers.background.destroyChildren();
                        popupLayers.grid.destroyChildren();
                        popupLayers.content.destroyChildren();
                        popupLayers.interaction.destroyChildren();

                        renderDayDetailBarChart(0, 0, newWidth, newHeight, dateStr, dayNumber, isHoliday);

                        popupLayers.background.batchDraw();
                        popupLayers.grid.batchDraw();
                        popupLayers.content.batchDraw();
                        popupLayers.interaction.batchDraw();
                    } finally {
                        setState({
                            layers: currentOriginalLayers,
                            stage: currentOriginalStage
                        });
                    }
                }
            }
        });
        resizeObserver.observe(container);

        // クリーンアップ用にResizeObserverを保存
        popupStage._resizeObserver = resizeObserver;

    } finally {
        // 元のstateを復元
        setState({
            layers: originalLayers,
            stage: originalStage
        });
    }
}

/**
 * Blazor DayDetailPopup用: 指定コンテナのKonva.jsステージを破棄
 * @param {string} containerId - DOM container ID
 */
function destroyDayDetailPopup(containerId) {
    if (dayDetailPopupStages.has(containerId)) {
        const stage = dayDetailPopupStages.get(containerId);
        if (stage._resizeObserver) {
            stage._resizeObserver.disconnect();
        }
        stage.destroy();
        dayDetailPopupStages.delete(containerId);
    }
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
    destroyDayDetailPopup
};
