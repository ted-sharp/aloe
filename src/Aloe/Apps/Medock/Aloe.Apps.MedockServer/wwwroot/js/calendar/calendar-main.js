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

/**
 * Initialize the calendar canvas
 * @param {string} containerId - DOM container ID
 * @param {object} data - Initial data { appointments, dayStats, holidays }
 * @param {object} options - Configuration options
 * @param {object} dotNetRef - .NET object reference for callbacks
 */
function init(containerId, data, options, dotNetRef) {
    setState({
        containerId,
        dotNetRef,
        options: { ...getState().options, ...options }
    });

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
        if (state.isDragging && state.selectedDateRange) {
            const range = state.selectedDateRange;
            // 範囲が有効な場合のみコールバック
            if (range.start !== range.end && state.dotNetRef) {
                // 日付の順序を正規化
                const start = parseDate(range.start);
                const end = parseDate(range.end);
                const normalizedRange = start <= end
                    ? { start: range.start, end: range.end }
                    : { start: range.end, end: range.start };
                state.dotNetRef.invokeMethodAsync('OnDateRangeSelected', normalizedRange.start, normalizedRange.end);
            }
        }
        setState({ isDragging: false, dragStartDate: null });
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
 * @param {object} data - { appointments: [], dayStats: {}, holidays: {} }
 */
function updateData(data) {
    if (data.appointments) {
        setState({ appointments: data.appointments });
    }

    if (data.dayStats) {
        setState({ dayStats: new Map(Object.entries(data.dayStats)) });
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
