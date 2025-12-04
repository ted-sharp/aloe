/**
 * Tooltip Management
 *
 * ホバー時に表示されるツールチップのDOM要素管理
 */

import { getState, setState } from '../state.js';

/**
 * ツールチップのDOM要素を作成してコンテナに追加
 */
export function createTooltip() {
    const state = getState();
    const container = document.getElementById(state.containerId);
    if (!container) return;

    let tooltip = container.querySelector('.medock-tooltip');
    if (!tooltip) {
        tooltip = document.createElement('div');
        tooltip.className = 'medock-tooltip';
        tooltip.style.cssText = `
            position: absolute;
            background: rgba(0, 0, 0, 0.85);
            color: white;
            padding: 8px 12px;
            border-radius: 6px;
            font-size: 12px;
            pointer-events: none;
            z-index: 1000;
            opacity: 0;
            transition: opacity 0.15s ease;
            max-width: 250px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
        `;
        container.appendChild(tooltip);
    }
    setState({ tooltip });
}

/**
 * ツールチップを指定座標に表示
 * @param {number} x - クライアント座標のX位置
 * @param {number} y - クライアント座標のY位置
 * @param {string} content - 表示するHTML文字列
 */
export function showTooltip(x, y, content) {
    const state = getState();
    if (!state.tooltip) return;

    // Convert client coordinates to container-relative coordinates
    const container = document.getElementById(state.containerId);
    if (container) {
        const rect = container.getBoundingClientRect();
        x = x - rect.left;
        y = y - rect.top;
    }

    state.tooltip.innerHTML = content;
    state.tooltip.style.left = `${x + 10}px`;
    state.tooltip.style.top = `${y + 10}px`;
    state.tooltip.style.opacity = '1';
}

/**
 * ツールチップを非表示にする
 */
export function hideTooltip() {
    const state = getState();
    if (!state.tooltip) return;
    state.tooltip.style.opacity = '0';
}
