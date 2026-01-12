/**
 * Crossfade Transition
 *
 * クロスフェードトランジション（古い画面と新しい画面をブレンド）
 */

import { CONFIG } from '../../config.js';

/**
 * クロスフェードトランジションを適用
 * @param {object} manager - CanvasManager インスタンス
 * @param {number} fadeDuration - フェード時間（ミリ秒）
 */
export function applyCrossfadeTransition(manager, fadeDuration) {
    // 1. 現在のメインCanvasの内容をスナップショット（古い画面）
    manager.canvases.forEach((canvas, layerName) => {
        const snapshotCanvas = manager.snapshotCanvases.get(layerName);
        if (snapshotCanvas) {
            const snapshotCtx = snapshotCanvas.getContext('2d');
            snapshotCtx.clearRect(0, 0, manager.width, manager.height);
            snapshotCtx.drawImage(canvas, 0, 0);
        }
    });

    // 2. アニメーションループでクロスフェード
    const startTime = performance.now();
    const animate = (currentTime) => {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / fadeDuration, 1);

        manager.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
            const ctx = manager.contexts.get(layerName);
            const snapshotCanvas = manager.snapshotCanvases.get(layerName);

            if (ctx && snapshotCanvas) {
                ctx.clearRect(0, 0, manager.width, manager.height);

                // 背景色を描画（テーマに対応した背景色）
                if (layerName === 'background' || layerName === 'grid' || layerName === 'content') {
                    ctx.fillStyle = CONFIG.colors.background;
                    ctx.fillRect(0, 0, manager.width, manager.height);
                }

                // 古い画面をフェードアウト（アルファ: 1 → 0）
                ctx.globalAlpha = 1 - progress;
                ctx.drawImage(snapshotCanvas, 0, 0);

                // 新しい画面をフェードイン（アルファ: 0 → 1）
                ctx.globalAlpha = progress;
                ctx.drawImage(offscreenCanvas, 0, 0);

                ctx.globalAlpha = 1.0; // 元に戻す
            }
        });

        if (progress < 1) {
            requestAnimationFrame(animate);
        }
    };
    requestAnimationFrame(animate);
}
