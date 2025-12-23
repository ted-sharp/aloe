/**
 * Modal Content Builder
 *
 * モーダル表示用のコンテンツHTML生成
 */

import { getState, setState } from '../../state.js';
import { renderDayDetailBarChart } from './index.js';

/**
 * モーダル内にKonva.jsのStageを作成して棒グラフを描画
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {Array|null} slots - 時間帯枠データ
 * @param {object} state - カレンダーの状態
 * @returns {Konva.Stage|null} 作成したStageインスタンス（エラー時はnull）
 */
export function buildModalContent(dateStr, slots, state) {
    const container = document.getElementById('medock-modal-chart-container');
    if (!container) {
        console.error('MedockCalendar: Modal chart container not found');
        return null;
    }

    // 既存のStageがあれば破棄
    const existingStage = container.querySelector('canvas');
    if (existingStage) {
        const stage = existingStage.getStage ? existingStage.getStage() : null;
        if (stage) {
            stage.destroy();
        }
    }

    // コンテナの実際のサイズを取得（画面サイズに応じて動的に計算）
    // モーダルが表示された後にサイズを取得するため、少し待機
    const getContainerSize = () => {
        const rect = container.getBoundingClientRect();
        // パディングやマージンを考慮してサイズを計算
        const padding = 32; // py-4のパディング + マージン
        const headerHeight = 40; // タイトルとボタンの高さ
        const footerHeight = 60; // モーダルアクションの高さ
        const availableWidth = Math.max(800, rect.width - padding);
        const availableHeight = Math.max(500, rect.height - padding - headerHeight - footerHeight);
        return {
            width: availableWidth,
            height: availableHeight
        };
    };

    // サイズを取得（モーダルが表示された直後はサイズが0の場合があるため、少し待機）
    let containerSize = getContainerSize();
    if (containerSize.width === 0 || containerSize.height === 0) {
        // サイズが取得できない場合は、デフォルト値を使用
        containerSize = {
            width: Math.max(800, window.innerWidth * 0.85),
            height: Math.max(500, window.innerHeight * 0.75)
        };
    }

    const containerWidth = containerSize.width;
    const containerHeight = containerSize.height;

    // モーダル用のKonva Stageを作成
    const modalStage = new Konva.Stage({
        container: 'medock-modal-chart-container',
        width: containerWidth,
        height: containerHeight
    });

    // モーダル用のlayersを作成
    const modalLayers = {
        background: new Konva.Layer(),
        grid: new Konva.Layer(),
        content: new Konva.Layer(),
        interaction: new Konva.Layer()
    };

    // Stageにlayersを追加
    modalStage.add(modalLayers.background);
    modalStage.add(modalLayers.grid);
    modalStage.add(modalLayers.content);
    modalStage.add(modalLayers.interaction);

    // 元のstate.layersを保存
    const originalLayers = state.layers;
    const originalStage = state.stage;

    try {
        // 一時的にstate.layersをモーダル用のlayersに置き換え
        setState({
            layers: modalLayers,
            stage: modalStage
        });

        // 日付から日数を取得
        const date = new Date(dateStr);
        const dayNumber = date.getDate();
        const isHoliday = state.holidays.has(dateStr);

        // セルのサイズを設定（モーダル内の描画エリア全体を使用）
        const cellLeft = 0;
        const cellTop = 0;
        const cellWidth = containerWidth;
        const cellHeight = containerHeight;

        // renderDayDetailBarChartを呼び出して棒グラフを描画
        renderDayDetailBarChart(cellLeft, cellTop, cellWidth, cellHeight, dateStr, dayNumber, isHoliday);

        // レイヤーを描画
        modalLayers.background.batchDraw();
        modalLayers.grid.batchDraw();
        modalLayers.content.batchDraw();
        modalLayers.interaction.batchDraw();

        return modalStage;
    } catch (error) {
        console.error('MedockCalendar: Failed to render bar chart in modal', error);
        // エラー時はStageを破棄
        modalStage.destroy();
        return null;
    } finally {
        // 元のstate.layersを復元
        setState({
            layers: originalLayers,
            stage: originalStage
        });
    }
}

