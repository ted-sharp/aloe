/**
 * Slidefade Transition
 *
 * スライドフェードトランジション（弱ディゾルブ＋微小スライド）
 */

/**
 * スライドフェードトランジションを適用
 * @param {object} manager - CanvasManager インスタンス
 * @param {number} fadeDuration - フェード時間（ミリ秒）
 */
export function applySlidefadeTransition(manager, fadeDuration) {
    // 1. 現在のメインCanvasの内容をスナップショット（古い画面）
    manager.canvases.forEach((canvas, layerName) => {
        const snapshotCanvas = manager.snapshotCanvases.get(layerName);
        if (snapshotCanvas) {
            const snapshotCtx = snapshotCanvas.getContext('2d');
            snapshotCtx.clearRect(0, 0, manager.width, manager.height);
            snapshotCtx.drawImage(canvas, 0, 0);
        }
    });

    // 2. アニメーションループでスライドフェード
    const slideDistance = 30; // スライド距離（ピクセル）
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

                // 古い画面: フェードアウト＋微小左スライド（アルファ: 1 → 0、X: 0 → -15px）
                ctx.save();
                ctx.globalAlpha = 1 - progress; // 完全に消える
                ctx.translate(-slideDistance * 0.5 * easeOut, 0);
                ctx.drawImage(snapshotCanvas, 0, 0);
                ctx.restore();

                // 新しい画面: フェードイン＋右からスライド（アルファ: 0 → 1、X: 30px → 0）
                ctx.save();
                ctx.globalAlpha = progress;
                ctx.translate(slideDistance * (1 - easeOut), 0);
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
