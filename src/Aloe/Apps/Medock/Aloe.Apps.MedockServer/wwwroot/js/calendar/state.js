/**
 * Centralized State Management
 *
 * カレンダーの状態を管理する単一のstateオブジェクト
 * 全モジュールから参照・更新される共有状態
 */

import { CONFIG } from './config.js';

/**
 * カレンダーの状態オブジェクト
 */
let state = {
    canvasManager: null,              // Canvas Manager インスタンス
    stage: null,                      // 互換性のため残す（Konva.Stageの代わり）
    layers: {},                       // 互換性のため残す（Konvaレイヤーの代わり）
    dotNetRef: null,                  // .NET オブジェクト参照（コールバック用）
    containerId: null,                // コンテナ要素のID
    currentView: 'month',             // 現在のビュー: 'year', 'month', 'week'
    previousView: null,               // 前回のビュー（トランジション判定用）
    currentDate: new Date(),          // 現在表示中の日付
    appointments: [],                 // 予約データの配列
    mainStats: new Map(),              // 日付文字列 -> Mainリソース統計 { am, pm, amMax, pmMax, slots[], isDayGrayedOut }
    equipmentStats: new Map(),         // 日付文字列 -> Equipmentリソース統計 { resources: { resourceId: { resourceName, totalAvailable, totalCapacity, slots[] } } }
    holidays: new Map(),              // 日付文字列 -> 祝日名
    options: {
        weekDays: 7,                  // 週表示で表示する日数: 1, 3, 7, 14, 31
        showSlots: true,              // スロット表示モード（true）かアバター表示（false）か
        showSimpleView: false,         // 簡易表示モード（記号表示）を表示するかどうか（月間・年間表示用）
        startHour: 8,                 // 週表示の開始時刻
        endHour: 18                   // 週表示の終了時刻
    },
    tooltip: null,                    // ツールチップのDOM要素
    hoveredElement: null,             // 現在ホバー中の要素
    // インタラクション用の追加状態
    selectedDate: null,               // 選択中の日付
    selectedDateRange: null,          // 範囲選択 { start: string, end: string }
    confirmedDateRange: null,         // 確定した範囲選択 { start: string, end: string } - グレーアウト判定に使用
    lastClickTime: 0,                 // ダブルクリック検出用（最後のクリック時刻）
    lastClickDate: null,              // ダブルクリック検出用（最後にクリックした日付）
    isDragging: false,                // ドラッグ中フラグ
    dragStartDate: null,              // ドラッグ開始日
    resizeObserver: null              // ResizeObserver インスタンス（クリーンアップ用）
};

/**
 * 状態オブジェクトを取得
 * @returns {Object} 現在の状態オブジェクト
 */
export function getState() {
    return state;
}

/**
 * 状態を更新
 * @param {Object} updates - 更新する状態のプロパティ
 */
export function setState(updates) {
    Object.assign(state, updates);
}

/**
 * 状態をリセット（クリーンアップ）
 */
export function resetState() {
    // Canvas Managerを破棄
    if (state.canvasManager) {
        state.canvasManager.destroy();
    }

    // 互換性のためのKonva Stageを破棄（存在する場合）
    if (state.stage && state.stage.destroy) {
        state.stage.destroy();
    }

    // ResizeObserverを切断
    if (state.resizeObserver) {
        state.resizeObserver.disconnect();
    }

    // 状態をリセット（必要なプロパティのみクリア）
    state = {
        ...state,
        canvasManager: null,
        stage: null,
        layers: {},
        appointments: [],
        mainStats: new Map(),
        equipmentStats: new Map(),
        holidays: new Map(),
        resizeObserver: null,
        selectedDate: null,
        selectedDateRange: null,
        confirmedDateRange: null,
        isDragging: false,
        dragStartDate: null
    };
}
