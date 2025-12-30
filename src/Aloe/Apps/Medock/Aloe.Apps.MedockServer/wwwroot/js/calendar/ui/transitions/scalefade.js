/**
 * Scalefade Transition
 *
 * スケールフェードトランジション（弱ディゾルブ＋微小スケール）
 */

/**
 * スケールフェードトランジションを適用
 * @param {object} manager - CanvasManager インスタンス
 * @param {number} fadeDuration - フェード時間（ミリ秒）
 */
export function applyScalefadeTransition(manager, fadeDuration) {
    // 1. 現在のメインCanvasの内容をスナップショット（古い画面）
    manager.canvases.forEach((canvas, layerName) => {
        const snapshotCanvas = manager.snapshotCanvases.get(layerName);
        if (snapshotCanvas) {
            const snapshotCtx = snapshotCanvas.getContext('2d');
            snapshotCtx.clearRect(0, 0, manager.width, manager.height);
            snapshotCtx.drawImage(canvas, 0, 0);
        }
    });

    // 2. アニメーションループでスケールフェード
    const startTime = performance.now();

    const animate = (currentTime) => {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / fadeDuration, 1);

        // イージング関数（ease-out）
        const easeOut = 1 - Math.pow(1 - progress, 3);

        manager.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
            const ctx = manager.contexts.get(layerName);
            const snapshotCanvas = manager.snapshotCanvases.get(layerName);

            if (ctx && snapshotCanvas) {
                ctx.clearRect(0, 0, manager.width, manager.height);

                // 古い画面: フェードアウト＋微小縮小（アルファ: 1 → 0、スケール: 1.0 → 0.95）
                const oldScale = 1.0 - easeOut * 0.05;
                const oldOffsetX = (manager.width - manager.width * oldScale) / 2;
                const oldOffsetY = (manager.height - manager.height * oldScale) / 2;

                ctx.save();
                ctx.globalAlpha = 1 - progress; // 完全に消える
                ctx.translate(oldOffsetX, oldOffsetY);
                ctx.scale(oldScale, oldScale);
                ctx.drawImage(snapshotCanvas, 0, 0);
                ctx.restore();

                // 新しい画面: フェードイン＋拡大（アルファ: 0 → 1、スケール: 0.95 → 1.0）
                const newScale = 0.95 + easeOut * 0.05;
                const newOffsetX = (manager.width - manager.width * newScale) / 2;
                const newOffsetY = (manager.height - manager.height * newScale) / 2;

                ctx.save();
                ctx.globalAlpha = progress;
                ctx.translate(newOffsetX, newOffsetY);
                ctx.scale(newScale, newScale);
                ctx.drawImage(offscreenCanvas, 0, 0);
                ctx.restore();
            }
        });

        if (progress < 1) {
            requestAnimationFrame(animate);
        }
    };
    requestAnimationFrame(animate);
}
