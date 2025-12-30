/**
 * Canvas Transition Controller
 *
 * Canvas Manager のトランジション処理を管理
 */

import { applyCrossfadeTransition } from './transitions/crossfade.js';
import { applyFadethroughTransition } from './transitions/fadethrough.js';
import { applySlidefadeTransition } from './transitions/slidefade.js';
import { applyScalefadeTransition } from './transitions/scalefade.js';
import { applySharedElementTransition } from './transitions/sharedelement.js';

/**
 * すべてのオフスクリーンレイヤーをメインCanvasに転送（トランジション付き）
 * @param {object} manager - CanvasManager インスタンス
 * @param {string} fadeMode - フェードモード: 'instant', 'crossfade', 'fadethrough', 'slidefade', 'scalefade', 'sharedelement'（デフォルト: 'instant'）
 * @param {number} fadeDuration - フェード時間（ミリ秒、デフォルト: 150）
 * @param {object} options - 追加オプション { sourceBounds, targetBounds, transitionType }
 */
export function commitAll(manager, fadeMode = 'instant', fadeDuration = 150, options = {}) {
    // 下位互換性のため、古いAPI（boolean）もサポート
    if (typeof fadeMode === 'boolean') {
        fadeMode = fadeMode ? 'crossfade' : 'instant';
    }

    if (fadeMode === 'instant') {
        // 通常の一括転送（瞬時に切り替え）
        manager.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
            const ctx = manager.contexts.get(layerName);
            if (ctx) {
                ctx.clearRect(0, 0, manager.width, manager.height);
                ctx.drawImage(offscreenCanvas, 0, 0);
            }
        });

    } else if (fadeMode === 'crossfade') {
        applyCrossfadeTransition(manager, fadeDuration);

    } else if (fadeMode === 'fadethrough') {
        applyFadethroughTransition(manager, fadeDuration);

    } else if (fadeMode === 'slidefade') {
        applySlidefadeTransition(manager, fadeDuration);

    } else if (fadeMode === 'scalefade') {
        applyScalefadeTransition(manager, fadeDuration);

    } else if (fadeMode === 'sharedelement') {
        const result = applySharedElementTransition(manager, fadeDuration, options);

        // bounds情報がない場合はscalefadeにフォールバック
        if (result === null) {
            applyScalefadeTransition(manager, fadeDuration);
        }
    }
}
