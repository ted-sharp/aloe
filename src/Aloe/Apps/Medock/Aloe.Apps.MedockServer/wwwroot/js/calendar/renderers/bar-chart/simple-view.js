/**
 * Simple View Renderer
 *
 * 簡易表示モード（記号表示）の描画処理
 */

import { getState } from '../../state.js';
import { CONFIG } from '../../config.js';

/**
 * 空き率から記号を決定
 * @param {number} vacancyRatio - 空き率（0.0 ～ 1.0）
 * @returns {string} 記号の種類: 'x', 'triangle', 'circle', 'double-circle'
 */
export function getSymbolFromVacancyRatio(vacancyRatio) {
    if (vacancyRatio === 0) {
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
 * 簡易表示モードで記号を描画
 * @param {number} cellLeft - セルの左端X座標
 * @param {number} cellTop - セルの上端Y座標
 * @param {number} cellWidth - セル幅
 * @param {number} cellHeight - セル高さ
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {string} symbolType - 記号の種類: 'x', 'triangle', 'circle', 'double-circle'
 * @param {boolean} isDateGrayed - グレーアウトフラグ
 * @param {number} vacancyRatio - 空き率（0.0 ～ 1.0）
 */
export function renderSimpleViewSymbol(cellLeft, cellTop, cellWidth, cellHeight, dateStr, symbolType, isDateGrayed, vacancyRatio = 0) {
    const state = getState();
    const { layers } = state;

    // 日付テキストの高さ
    const dayTextHeight = CONFIG.font.sizeSmall + 4;
    
    // 大きいセルで空き率を数字で表示するかどうか（先に判定）
    const shouldShowVacancyText = cellWidth >= 80 && cellHeight >= 60;
    const vacancyTextHeight = shouldShowVacancyText ? 14 : 0; // 空き率テキストの高さ
    
    // 利用可能な領域を計算（日付テキストと空き率テキストの領域を考慮）
    const availableWidth = cellWidth - 4; // 左右余白
    const availableHeight = cellHeight - dayTextHeight - (shouldShowVacancyText ? vacancyTextHeight + 4 : 4); // 日付テキストと空き率テキスト分を考慮
    
    // 記号サイズを計算（はみ出さないように余裕を持たせる）
    const baseSize = Math.min(availableWidth, availableHeight);
    const symbolSize = Math.max(6, baseSize * 0.7); // 最小6px、利用可能領域の70%を使用（はみ出し防止のため余裕を持たせる）
    
    // 記号を利用可能な領域の垂直方向の中央に配置
    const symbolCenterX = cellLeft + cellWidth / 2;
    const symbolCenterY = cellTop + dayTextHeight + (availableHeight / 2);
    
    const opacity = isDateGrayed ? 0.4 : 1;

    switch (symbolType) {
        case 'x':
            // ×（バツ）- 赤色
            const xFontSize = symbolSize * 1.2;
            const xSymbol = new Konva.Text({
                x: cellLeft,
                y: symbolCenterY - xFontSize * 0.35, // 垂直中央に配置（フォントサイズの約35%上にベースラインを配置）
                width: cellWidth,
                text: '×',
                fontSize: xFontSize,
                fontFamily: CONFIG.font.family,
                fill: '#ef4444', // 赤色
                align: 'center',
                opacity: opacity
            });
            layers.content.add(xSymbol);
            break;

        case 'triangle':
            // △（三角）- 黄色
            const triangle = new Konva.RegularPolygon({
                x: symbolCenterX,
                y: symbolCenterY,
                sides: 3,
                radius: symbolSize / 2,
                fill: '#fbbf24', // 黄色
                rotation: 180, // 上向きにする
                opacity: opacity
            });
            layers.content.add(triangle);
            break;

        case 'circle':
            // ○（丸）- 緑色
            const circle = new Konva.Circle({
                x: symbolCenterX,
                y: symbolCenterY,
                radius: symbolSize / 2,
                fill: '#10b981', // 緑色
                opacity: opacity
            });
            layers.content.add(circle);
            break;

        case 'double-circle':
            // ◎（二重丸）- 緑色
            const outerCircle = new Konva.Circle({
                x: symbolCenterX,
                y: symbolCenterY,
                radius: symbolSize / 2,
                fill: '#10b981', // 緑色
                opacity: opacity
            });
            layers.content.add(outerCircle);
            
            const innerCircle = new Konva.Circle({
                x: symbolCenterX,
                y: symbolCenterY,
                radius: symbolSize / 3,
                fill: '#10b981', // 緑色
                opacity: opacity
            });
            layers.content.add(innerCircle);
            break;
    }
    
    // 大きいセルの場合は空き率を数字で表示
    if (shouldShowVacancyText && vacancyRatio !== undefined && vacancyRatio !== null) {
        const vacancyPercent = Math.round(vacancyRatio * 100);
        const textFontSize = Math.max(8, Math.min(cellWidth * 0.12, 12)); // 最小8px、最大12px
        const textY = symbolCenterY + symbolSize / 2 + 4; // 記号の下に配置
        
        const vacancyText = new Konva.Text({
            x: cellLeft,
            y: textY,
            width: cellWidth,
            text: `${vacancyPercent}%`,
            fontSize: textFontSize,
            fontFamily: CONFIG.font.numberFamily,
            fill: isDateGrayed ? '#9ca3af' : '#6b7280',
            align: 'center',
            opacity: opacity
        });
        layers.content.add(vacancyText);
    }
}

