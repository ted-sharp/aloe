/**
 * Shared Element Transition
 *
 * 共有要素トランジション（位置・サイズを主に補間）
 */

/**
 * 共有要素トランジションを適用
 * @param {object} manager - CanvasManager インスタンス
 * @param {number} fadeDuration - フェード時間（ミリ秒）
 * @param {object} options - オプション { sourceBounds, targetBounds, transitionType }
 */
export function applySharedElementTransition(manager, fadeDuration, options = {}) {
    const sourceBounds = options.sourceBounds;
    const targetBounds = options.targetBounds;

    if (!sourceBounds || !targetBounds) {
        // bounds情報がない場合はnullを返す（呼び出し側でフォールバック）
        return null;
    }

    // 1. すべてのレイヤーを合成したスナップショットを作成（古い画面）
    // 各レイヤーごとにスナップショットを取るのではなく、すべてのレイヤーを1つに合成
    const compositeSnapshot = document.createElement('canvas');
    compositeSnapshot.width = manager.width;
    compositeSnapshot.height = manager.height;
    const compositeCtx = compositeSnapshot.getContext('2d');

    // すべてのメインCanvasレイヤーを順番に合成
    manager.canvases.forEach((canvas, layerName) => {
        compositeCtx.drawImage(canvas, 0, 0);
    });

    console.log('Composite snapshot created:', {
        size: `${compositeSnapshot.width}x${compositeSnapshot.height}`
    });

    // 2. アニメーションループ
    const startTime = performance.now();

    const animate = (currentTime) => {
        const elapsed = currentTime - startTime;
        const progress = Math.min(elapsed / fadeDuration, 1);

        // ease-in-out-cubic イージングで開始と終了を滑らかに
        const easeInOutCubic = progress < 0.5
            ? 4 * progress * progress * progress
            : 1 - Math.pow(-2 * progress + 2, 3) / 2;

        manager.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
            const ctx = manager.contexts.get(layerName);
            // 合成スナップショットを使用（各レイヤーの個別スナップショットは使わない）
            const snapshotCanvas = compositeSnapshot;

            if (ctx && snapshotCanvas) {
                ctx.clearRect(0, 0, manager.width, manager.height);

                // 遷移方向を判定
                const transitionType = options.transitionType || '';
                const isExpandingTransition = transitionType.endsWith('-to-year') || transitionType.endsWith('-to-week');

                // 月→年/週の場合: 年/週ビューをすっぱり表示、その上に月ビューを縮小
                if (isExpandingTransition) {
                    // 新しい画面（年/週ビュー）を最初から完全表示
                    ctx.drawImage(offscreenCanvas, 0, 0);

                    // progress が 0.95 未満の場合のみ月ビューを描画（最後は完全に消す）
                    if (progress < 0.95) {
                        // 共有要素の位置・サイズを補間
                        const currentX = sourceBounds.x + (targetBounds.x - sourceBounds.x) * easeInOutCubic;
                        const currentY = sourceBounds.y + (targetBounds.y - sourceBounds.y) * easeInOutCubic;
                        const currentWidth = sourceBounds.width + (targetBounds.width - sourceBounds.width) * easeInOutCubic;
                        const currentHeight = sourceBounds.height + (targetBounds.height - sourceBounds.height) * easeInOutCubic;

                        // 古い画面（月ビュー）を不透明のまま縮小（オーバーレイ）
                        // 縮小比率を計算
                        const scaleX = currentWidth / sourceBounds.width;
                        const scaleY = currentHeight / sourceBounds.height;
                        const offsetX = currentX - sourceBounds.x * scaleX;
                        const offsetY = currentY - sourceBounds.y * scaleY;

                        ctx.save();

                        // クリップマスク：縮小する領域だけ描画
                        ctx.beginPath();
                        ctx.rect(currentX, currentY, currentWidth, currentHeight);
                        ctx.clip();

                        // 変換を適用して月ビューを縮小描画
                        ctx.translate(offsetX, offsetY);
                        ctx.scale(scaleX, scaleY);

                        // まず背景を白で塗りつぶす（変換後の座標系、元のキャンバスサイズ）
                        ctx.fillStyle = '#ffffff';
                        ctx.fillRect(0, 0, sourceBounds.width, sourceBounds.height);

                        // 月ビューを描画
                        ctx.drawImage(snapshotCanvas, 0, 0);
                        ctx.restore();
                    }

                } else {
                    // 年/週→月の場合: 従来の動作を維持
                    // 共有要素の位置・サイズを補間
                    const currentX = sourceBounds.x + (targetBounds.x - sourceBounds.x) * easeInOutCubic;
                    const currentY = sourceBounds.y + (targetBounds.y - sourceBounds.y) * easeInOutCubic;
                    const currentWidth = sourceBounds.width + (targetBounds.width - sourceBounds.width) * easeInOutCubic;
                    const currentHeight = sourceBounds.height + (targetBounds.height - sourceBounds.height) * easeInOutCubic;

                    // 古い画面のフェードアウト
                    const fadeOutSpeed = 1.2;
                    ctx.save();
                    ctx.globalAlpha = Math.max(0, 1 - progress * fadeOutSpeed);
                    ctx.drawImage(snapshotCanvas, 0, 0);
                    ctx.restore();

                    // 共有要素: 位置・サイズのみ補間
                    ctx.save();

                    // クリップマスク：共有要素の領域だけ描画
                    ctx.beginPath();
                    ctx.rect(currentX, currentY, currentWidth, currentHeight);
                    ctx.clip();

                    // 新しい画面の全体を描画（クリップ内のみ表示される）
                    const scaleX = currentWidth / targetBounds.width;
                    const scaleY = currentHeight / targetBounds.height;
                    const offsetX = currentX - targetBounds.x * scaleX;
                    const offsetY = currentY - targetBounds.y * scaleY;

                    ctx.translate(offsetX, offsetY);
                    ctx.scale(scaleX, scaleY);
                    ctx.drawImage(offscreenCanvas, 0, 0);
                    ctx.restore();

                    // 周辺コンテンツ: progress > 0.7 からフェードイン
                    if (progress > 0.7) {
                        const delayedProgress = (progress - 0.7) / 0.3;
                        const contentAlpha = delayedProgress * 0.6;
                        ctx.save();
                        ctx.globalAlpha = contentAlpha;
                        ctx.drawImage(offscreenCanvas, 0, 0);
                        ctx.restore();
                    }
                }
            }
        });

        if (progress < 1) {
            requestAnimationFrame(animate);
        } else {
            // アニメーション完了後の処理
            const transitionType = options.transitionType || '';
            const isExpandingTransition = transitionType.endsWith('-to-year') || transitionType.endsWith('-to-week');

            // 月→年/週の場合、年/週ビューはすでに完全表示されているので何もしない
            // 年/週→月の場合は、月ビューを即座に完全表示
            if (!isExpandingTransition) {
                manager.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
                    const ctx = manager.contexts.get(layerName);
                    if (ctx) {
                        ctx.clearRect(0, 0, manager.width, manager.height);
                        ctx.drawImage(offscreenCanvas, 0, 0);
                    }
                });
            }
        }
    };
    requestAnimationFrame(animate);
}
