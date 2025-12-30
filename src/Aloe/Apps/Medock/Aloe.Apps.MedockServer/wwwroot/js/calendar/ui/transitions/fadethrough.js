/**
 * Fadethrough Transition
 *
 * フェードスルートランジション（フェードアウト → フェードイン）
 */

/**
 * フェードスルートランジションを適用
 * @param {object} manager - CanvasManager インスタンス
 * @param {number} fadeDuration - フェード時間（ミリ秒）
 */
export function applyFadethroughTransition(manager, fadeDuration) {
    // 1. 現在のメインCanvasの内容をスナップショット（古い画面）
    manager.canvases.forEach((canvas, layerName) => {
        const snapshotCanvas = manager.snapshotCanvases.get(layerName);
        if (snapshotCanvas) {
            const snapshotCtx = snapshotCanvas.getContext('2d');
            snapshotCtx.clearRect(0, 0, manager.width, manager.height);
            snapshotCtx.drawImage(canvas, 0, 0);
        }
    });

    // 2. アニメーションループでフェードスルー
    const halfDuration = fadeDuration / 2;
    const startTime = performance.now();

    const animate = (currentTime) => {
        const elapsed = currentTime - startTime;
        const totalProgress = Math.min(elapsed / fadeDuration, 1);

        manager.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
            const ctx = manager.contexts.get(layerName);
            const snapshotCanvas = manager.snapshotCanvases.get(layerName);

            if (ctx && snapshotCanvas) {
                ctx.clearRect(0, 0, manager.width, manager.height);

                if (elapsed < halfDuration) {
                    // 前半: 古い画面をフェードアウト（アルファ: 1 → 0）
                    const fadeOutProgress = elapsed / halfDuration;
                    ctx.globalAlpha = 1 - fadeOutProgress;
                    ctx.drawImage(snapshotCanvas, 0, 0);
                    ctx.globalAlpha = 1.0;
                } else {
                    // 後半: 新しい画面をフェードイン（アルファ: 0 → 1）
                    const fadeInProgress = (elapsed - halfDuration) / halfDuration;
                    ctx.globalAlpha = fadeInProgress;
                    ctx.drawImage(offscreenCanvas, 0, 0);
                    ctx.globalAlpha = 1.0;
                }
            }
        });

        if (totalProgress < 1) {
            requestAnimationFrame(animate);
        }
    };
    requestAnimationFrame(animate);
}
