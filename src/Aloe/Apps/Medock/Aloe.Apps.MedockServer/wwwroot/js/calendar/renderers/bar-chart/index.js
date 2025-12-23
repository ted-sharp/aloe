/**
 * Bar Chart Renderer
 *
 * 棒グラフ形式での日付セル描画
 * 年表示・月表示で使用される
 */

import { getState } from '../../state.js';
import { CONFIG } from '../../config.js';
import { isDateInRange } from '../../utils/date-utils.js';
import { getSymbolFromVacancyRatio, renderSimpleViewSymbol } from './simple-view.js';
import { renderBarChart } from './bar-rendering.js';
import { createInteractionArea } from './interactions.js';

/**
 * 棒グラフ形式で日付セルを描画
 * @param {number} cellLeft - セルの左端X座標
 * @param {number} cellTop - セルの上端Y座標
 * @param {number} cellWidth - セル幅
 * @param {number} cellHeight - セル高さ
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {number} dayNumber - 日にち
 * @param {boolean} isHoliday - 祝日フラグ
 */
export function renderDayBarChart(cellLeft, cellTop, cellWidth, cellHeight, dateStr, dayNumber, isHoliday = false) {
    const state = getState();
    const { layers } = state;

    // セルサイズが小さすぎる場合は描画をスキップ（ただし、hitAreaは作成する）
    const isTooSmall = cellWidth < 10 || cellHeight < 10;

    // 時間帯枠データを取得
    const stats = state.mainStats.get(dateStr);
    const slots = stats?.slots || null;

    // 営業時間情報を取得（昼休み時間帯の縦ライン描画用、関数全体で使用）
    const businessHours = state.options?.businessHours;
    
    // 昼休み時間を解析（関数全体で使用するため、最初に初期化）
    let lunchStartHour = null;
    let lunchEndHour = null;
    if (businessHours && businessHours.lunchStartTime && businessHours.lunchEndTime) {
        const parseTime = (timeStr) => {
            const parts = timeStr.split(':');
            return parseInt(parts[0], 10) + (parseInt(parts[1] || 0, 10) / 60);
        };
        lunchStartHour = parseTime(businessHours.lunchStartTime);
        lunchEndHour = parseTime(businessHours.lunchEndTime);
    }

    // グレーアウト判定: confirmedDateRange がある場合は範囲外をグレーアウト
    let isDateGrayed = false;
    if (state.confirmedDateRange) {
        isDateGrayed = !isDateInRange(dateStr, state.confirmedDateRange.start, state.confirmedDateRange.end);
    } else {
        isDateGrayed = stats?.isGrayedOut || false;
    }

    let bgRect = null; // 小さなセルの場合でもhitAreaで参照できるようにする

    // ラベル表示用のスペースを確保（年間ビューはより厳しい条件、月間ビューは標準）
    const isYearView = state.currentView === 'year';
    // 日付数字のフォントサイズをビューに応じて決定
    const dateFontSize = isYearView ? CONFIG.font.sizeDateYear : CONFIG.font.sizeDateMonth;
    // 日付テキスト表示エリア（小さなセルでも計算しておく）
    const dayTextHeight = dateFontSize + 4;
    const barAreaTop = cellTop + dayTextHeight;
    const labelAreaHeight = (cellWidth >= 40 && cellHeight >= 50) ? (isYearView ? 10 : 12) : 0;
    const barAreaHeight = Math.max(0, cellHeight - dayTextHeight - 4 - labelAreaHeight); // 下部余白4px + ラベルエリア、負の値を防止

    // 小さなセルの場合は描画をスキップ
    if (!isTooSmall) {
        // 背景矩形（サイズは最小0を保証）
        const bgWidth = Math.max(0, cellWidth - 2);
        const bgHeight = Math.max(0, cellHeight - 2);
        const bgCornerRadius = Math.min(2, bgWidth / 2, bgHeight / 2); // cornerRadiusは幅/高さの半分以下

        bgRect = new Konva.Rect({
            x: cellLeft + 1,
            y: cellTop + 1,
            width: bgWidth,
            height: bgHeight,
            fill: isDateGrayed ? '#f3f4f6' : CONFIG.colors.slot.background,
            cornerRadius: Math.max(0, bgCornerRadius),
            opacity: isDateGrayed ? 0.6 : 1
        });
        layers.content.add(bgRect);

        // 日付テキスト
        const dayOfWeek = new Date(dateStr).getDay();
        let textColor;
        if (isDateGrayed) {
            textColor = '#9ca3af';
        } else if (isHoliday || dayOfWeek === 0) {
            textColor = CONFIG.colors.weekend.sun;
        } else if (dayOfWeek === 6) {
            textColor = CONFIG.colors.weekend.sat;
        } else {
            textColor = '#374151';
        }

        const dayText = new Konva.Text({
            x: cellLeft + 1,
            y: cellTop + 2,
            width: cellWidth - 2,
            text: String(dayNumber),
            fontSize: dateFontSize,
            fontFamily: CONFIG.font.numberFamily,
            fill: textColor,
            align: 'center',
            wrap: 'none'
        });
        layers.content.add(dayText);
    }

    // 簡易表示モードの判定
    const showSimpleView = state.options?.showSimpleView ?? false;

    // 簡易表示モードの場合
    if (!isTooSmall && showSimpleView) {
        // 時間帯枠データから空き率を計算（フィルタ条件を考慮）
        let overallVacancyRatio = 0;
        
        if (slots && slots.length > 0) {
            // 全スロットの平均空き率を計算
            let totalVacancy = 0;
            let slotCount = 0;
            
            slots.forEach(slot => {
                // グレーアウトされたスロットは除外
                if (slot.isGrayedOut || isDateGrayed) {
                    return;
                }
                
                const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 1;
                const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
                
                const vacancyRatio = Math.max(0, Math.min(1, 1 - (count / cap)));
                totalVacancy += vacancyRatio;
                slotCount++;
            });
            
            if (slotCount > 0) {
                overallVacancyRatio = totalVacancy / slotCount;
            }
        }
        
        // 記号を決定して描画
        const symbolType = getSymbolFromVacancyRatio(overallVacancyRatio);
        renderSimpleViewSymbol(cellLeft, cellTop, cellWidth, cellHeight, dateStr, symbolType, isDateGrayed, overallVacancyRatio);
    }
    // 詳細表示モード（棒グラフ描画）
    else if (!isTooSmall && slots && slots.length > 0) {
        // 業務時間設定を取得
        const startHour = state.options.startHour || 8;
        const endHour = state.options.endHour || 18;

        const rendered = renderBarChart({
            cellLeft,
            cellTop,
            cellWidth,
            cellHeight,
            dateStr,
            barAreaTop,
            barAreaHeight,
            labelAreaHeight,
            isDateGrayed,
            slots,
            startHour,
            endHour,
            lunchStartHour,
            lunchEndHour,
            isYearView
        });

        if (!rendered) {
            // 描画されなかった場合はhitAreaのみ追加
            const hitArea = createInteractionArea({
                cellLeft,
                cellTop,
                cellWidth,
                cellHeight,
                dateStr,
                slots,
                bgRect
            });
            layers.interaction.add(hitArea);
            return;
        }
    }

    // インタラクションエリア
    const hitArea = createInteractionArea({
        cellLeft,
        cellTop,
        cellWidth,
        cellHeight,
        dateStr,
        slots,
        bgRect
    });

    layers.interaction.add(hitArea);
}

