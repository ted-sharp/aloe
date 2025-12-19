/**
 * Medock Calendar Main Entry Point
 *
 * ES Module統合とPublic API公開
 * window.MedockCalendar として公開され、Blazor JSInterop から呼び出される
 */

import { CONFIG } from './config.js';
import { getState, setState, resetState } from './state.js';
import { dateToString, parseDate } from './utils/date-utils.js';
import { createModal, hideModal } from './ui/modal.js';
import { createTooltip, hideTooltip } from './ui/tooltip.js';
import { createLayers } from './ui/layers.js';
import { renderYearView } from './renderers/year-view.js';
import { renderMonthView } from './renderers/month-view.js';
import { renderWeekView } from './renderers/week-view.js';

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
    createModal();

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
    hideModal();
    hideTooltip();

    // モーダルをDOMから削除
    const modal = document.getElementById('medock-day-modal');
    if (modal && modal.parentNode) {
        modal.parentNode.removeChild(modal);
    }

    // ツールチップをDOMから削除
    if (state.tooltip && state.tooltip.parentNode) {
        state.tooltip.parentNode.removeChild(state.tooltip);
    }

    resetState();
}

// Export Public API to global scope
window.MedockCalendar = {
    init,
    updateData,
    changeView,
    setOptions,
    navigateTo,
    render,
    destroy
};
