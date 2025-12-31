/**
 * Slot Display Utilities
 *
 * スロット表示ロジックの統一管理
 * 記号の決定・描画を一箇所で管理し、
 * 週間・月間・年間・日詳細ビューで共通で使用
 */

import { CONFIG } from '../config.js';
import { drawText, drawCircle, drawPolygon } from './canvas-utils.js';

/**
 * 空き率から記号を決定
 * @param {number} vacancyRatio - 空き率（0-1、マイナス値も許容）
 * @returns {string} 記号の種類: 'x', 'triangle', 'circle', 'double-circle'
 *
 * 判定基準:
 * - vacancyRatio <= 0: × (バツ)
 * - 0 < vacancyRatio < 0.3: △ (三角)
 * - 0.3 <= vacancyRatio < 0.6: ○ (丸)
 * - vacancyRatio >= 0.6: ◎ (二重丸)
 */
export function getSymbolFromVacancyRatio(vacancyRatio) {
    if (vacancyRatio <= 0) {
        return 'x'; // ×（バツ）
    } else if (vacancyRatio < 0.3) {
        return 'triangle'; // △（三角）
    } else if (vacancyRatio < 0.6) {
        return 'circle'; // ○（丸）
    } else {
        return 'double-circle'; // ◎（二重丸）
    }
}

/**
 * 複数のスロット情報から全体の空き率を計算
 * グレーアウト対象のスロットは除外
 *
 * @param {array} slotCaps - 容量配列
 * @param {array} slotCounts - 使用数配列
 * @param {Uint8Array|null} slotFlags - フラグ配列（ビット0: IsGrayedOut）
 * @param {boolean} isDateGrayed - 日付全体がグレーアウトしているか
 * @returns {{vacancyRatio: number, available: number, capacity: number}}
 */
export function calculateVacancyRatio(slotCaps, slotCounts, slotFlags, isDateGrayed) {
    let totalAvailable = 0;
    let totalCapacity = 0;

    if (slotCaps && slotCounts) {
        for (let i = 0; i < slotCaps.length; i++) {
            // フラグからisSlotGrayedを取得（ビット0: IsGrayedOut）
            const isSlotGrayed = slotFlags ? (slotFlags[i] & 0b001) !== 0 : false;
            if (isSlotGrayed || isDateGrayed) {
                continue;
            }

            const cap = (slotCaps[i] !== undefined && slotCaps[i] !== null && slotCaps[i] > 0) ? slotCaps[i] : 0;
            const count = (slotCounts[i] !== undefined && slotCounts[i] !== null) ? slotCounts[i] : 0;

            if (cap > 0) {
                totalCapacity += cap;
                totalAvailable += (cap - count);
            }
        }
    }

    const vacancyRatio = totalCapacity > 0 ? totalAvailable / totalCapacity : 0;

    return {
        vacancyRatio,
        available: totalAvailable,
        capacity: totalCapacity
    };
}

/**
 * 記号を描画
 * @param {CanvasRenderingContext2D} ctx - キャンバスコンテキスト
 * @param {number} centerX - 記号の中心X座標
 * @param {number} centerY - 記号の中心Y座標
 * @param {number} symbolSize - 記号のサイズ
 * @param {string} symbolType - 記号の種類: 'x', 'triangle', 'circle', 'double-circle'
 * @param {number} opacity - 透明度（0-1）
 */
export function drawSymbol(ctx, centerX, centerY, symbolSize, symbolType, opacity = 1) {
    switch (symbolType) {
        case 'x':
            // ×（バツ）- 赤色
            const xFontSize = symbolSize * 1.2;
            drawText(ctx, {
                text: '×',
                x: centerX - symbolSize * 0.6,
                y: centerY - xFontSize * 0.35,
                width: symbolSize * 1.2,
                fill: '#ef4444',
                fontSize: xFontSize,
                align: 'center',
                opacity: opacity
            });
            break;

        case 'triangle':
            // △（三角）- 黄色
            drawPolygon(ctx, {
                x: centerX,
                y: centerY,
                sides: 3,
                radius: symbolSize / 2,
                rotation: 90,
                fill: '#fbbf24',
                opacity: opacity
            });
            break;

        case 'circle':
            // ○（丸）- 緑色
            drawCircle(ctx, {
                x: centerX,
                y: centerY,
                radius: symbolSize / 2,
                fill: '#10b981',
                opacity: opacity
            });
            break;

        case 'double-circle':
            // ◎（二重丸）- 緑色
            drawCircle(ctx, {
                x: centerX,
                y: centerY,
                radius: symbolSize / 2,
                fill: '#10b981',
                opacity: opacity
            });
            drawCircle(ctx, {
                x: centerX,
                y: centerY,
                radius: symbolSize / 3,
                fill: '#10b981',
                opacity: opacity
            });
            break;
    }
}

/**
 * スロット情報テキストを描画
 * @param {CanvasRenderingContext2D} ctx - キャンバスコンテキスト
 * @param {number} x - テキストのX座標
 * @param {number} y - テキストのY座標
 * @param {number} width - テキスト領域の幅
 * @param {number} available - 空き数
 * @param {number} capacity - 容量
 * @param {boolean} isDateGrayed - グレーアウト中か
 * @param {number} opacity - 透明度
 */
export function drawSlotInfoText(ctx, x, y, width, available, capacity, isDateGrayed = false, opacity = 1) {
    if (capacity <= 0) {
        return;
    }

    const textFontSize = Math.max(8, Math.min(width * 0.12, 12));
    drawText(ctx, {
        text: `${Math.round(available)}/${Math.round(capacity)}`,
        x: x,
        y: y,
        width: width,
        fill: isDateGrayed ? '#9ca3af' : '#6b7280',
        fontSize: textFontSize,
        fontFamily: CONFIG.font.numberFamily,
        align: 'center',
        opacity: opacity
    });
}

/**
 * データがない場合の表示
 * @param {CanvasRenderingContext2D} ctx - キャンバスコンテキスト
 * @param {number} x - テキストのX座標
 * @param {number} y - テキストのY座標
 * @param {number} width - テキスト領域の幅
 * @param {boolean} isDateGrayed - グレーアウト中か
 * @param {number} opacity - 透明度
 */
export function drawNoDataDash(ctx, x, y, width, isDateGrayed = false, opacity = 1) {
    drawText(ctx, {
        text: '–',
        x: x,
        y: y - 8,
        width: width,
        fill: isDateGrayed ? '#9ca3af' : '#6b7280',
        fontSize: 16,
        align: 'center',
        opacity: opacity
    });
}
