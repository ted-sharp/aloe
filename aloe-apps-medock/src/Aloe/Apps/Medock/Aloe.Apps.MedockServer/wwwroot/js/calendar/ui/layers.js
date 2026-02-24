/**
 * Canvas Layer Management
 *
 * Canvas APIのレイヤー初期化とz-order管理
 * レイヤー構造（下から順）：
 *   1. background - 週末背景、祝日色
 *   2. grid - グリッド線、曜日ヘッダー
 *   3. content - 視覚要素（棒グラフ、テキスト等）
 *   4. interaction - インタラクション用ヒットエリア（透明）
 */

import { getState, setState } from '../state.js';

/**
 * Canvas Managerを使用してレイヤーを初期化
 * この関数は互換性のために残されていますが、実際にはCanvasManagerが処理します
 */
export function createLayers() {
    const state = getState();

    // CanvasManagerが既に存在する場合は、コンテキストをlayers互換形式で提供
    if (state.canvasManager) {
        const contexts = state.canvasManager.getAllContexts();
        
        // 互換性のためのlayers形式（Konvaとは異なる構造）
        const layers = {
            background: {
                destroyChildren: () => state.canvasManager.clearLayer('background'),
                batchDraw: () => {}, // Canvas APIではbatchDrawは不要
                add: () => {} // 互換性のためのダミー
            },
            grid: {
                destroyChildren: () => state.canvasManager.clearLayer('grid'),
                batchDraw: () => {},
                add: () => {}
            },
            content: {
                destroyChildren: () => state.canvasManager.clearLayer('content'),
                batchDraw: () => {},
                add: () => {}
            },
            interaction: {
                destroyChildren: () => state.canvasManager.clearLayer('interaction'),
                batchDraw: () => {},
                add: () => {}
            }
        };

        // layers プロパティは削除されたため、setState は呼ばない
    }
}
