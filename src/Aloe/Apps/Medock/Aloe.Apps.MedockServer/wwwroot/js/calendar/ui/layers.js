/**
 * Konva Layer Management
 *
 * Konva.jsのレイヤー初期化とz-order管理
 * レイヤー構造（下から順）：
 *   1. background - 週末背景、祝日色
 *   2. grid - グリッド線、曜日ヘッダー
 *   3. content - 視覚要素（棒グラフ、テキスト等）
 *   4. interaction - インタラクション用ヒットエリア（透明）
 */

import { getState, setState } from '../state.js';

/**
 * 4つのKonvaレイヤーを作成してStageに追加
 */
export function createLayers() {
    const state = getState();

    // Clear existing layers
    if (state.layers.background) state.layers.background.destroy();
    if (state.layers.grid) state.layers.grid.destroy();
    if (state.layers.content) state.layers.content.destroy();
    if (state.layers.interaction) state.layers.interaction.destroy();

    const layers = {
        background: new Konva.Layer(),
        grid: new Konva.Layer(),
        content: new Konva.Layer(),
        interaction: new Konva.Layer()
    };

    state.stage.add(layers.background);
    state.stage.add(layers.grid);
    state.stage.add(layers.content);
    state.stage.add(layers.interaction);

    setState({ layers });
}
