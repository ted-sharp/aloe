/**
 * Day Bar Chart Rendering (Aggregated)
 * 
 * 年間/月間カレンダー用の棒グラフ描画（集約版）
 * - 使用ビュー: 年間ビュー、月間ビュー
 * - 特徴: スロットを集約して表示（aggregateSlotsを使用）
 * - 関数: renderBarChart()
 * 
 * 注意: 日詳細ビューでは使用しない（day-detail-bar-rendering.jsを使用）
 */

import { getState } from '../../state.js';
import { CONFIG } from '../../config.js';
import { parseSlotTimeRange } from './slot-time-utils.js';
import { aggregateSlots } from './slot-aggregation.js';
import { getWinterColorFromAvailable } from '../../utils/winter-colormap.js';

/**
 * 棒グラフを描画
 * @param {object} params - 描画パラメータ
 * @param {number} params.cellLeft - セルの左端X座標
 * @param {number} params.cellTop - セルの上端Y座標
 * @param {number} params.cellWidth - セル幅
 * @param {number} params.cellHeight - セル高さ
 * @param {string} params.dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {number} params.barAreaTop - 棒グラフエリアの上端Y座標
 * @param {number} params.barAreaHeight - 棒グラフエリアの高さ
 * @param {number} params.labelAreaHeight - ラベルエリアの高さ
 * @param {boolean} params.isDateGrayed - グレーアウトフラグ
 * @param {Array} params.slots - スロットデータ
 * @param {number} params.startHour - 業務開始時刻
 * @param {number} params.endHour - 業務終了時刻
 * @param {number|null} params.lunchStartHour - 昼休み開始時刻
 * @param {number|null} params.lunchEndHour - 昼休み終了時刻
 * @param {boolean} params.isYearView - 年間ビューかどうか
 * @returns {boolean} 描画が成功したかどうか
 */
export function renderBarChart({
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
}) {
    const state = getState();
    const { layers } = state;

    // データがない場合は描画しない
    const validSlots = slots.filter(slot => {
        const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
        const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
        return cap > 0 || count > 0;
    });

    if (validSlots.length === 0) {
        return false;
    }

    const totalHours = endHour - startHour;
    const barAreaWidth = cellWidth - 4; // 左右余白2px
    
    // ビジネスアワーの開始・終了位置を固定X座標として計算
    const businessStartX = cellLeft + 2; // 開始位置
    const businessEndX = cellLeft + 2 + barAreaWidth; // 終了位置
    
    // 昼休み時間帯の長さを計算（昼休みを除外した実効時間を計算するため）
    const lunchDuration = (lunchStartHour !== null && lunchEndHour !== null) 
        ? (lunchEndHour - lunchStartHour) 
        : 0;
    const effectiveTotalHours = totalHours - lunchDuration; // 昼休みを除外した実効時間
    
    // 時刻をX座標に変換する関数（昼休み時間帯を除外して配置）
    const timeToX = (timeInHours) => {
        let relativePosition;
        if (lunchStartHour !== null && lunchEndHour !== null && effectiveTotalHours > 0) {
            // 昼休み時間帯を除外した配置
            const morningHours = lunchStartHour - startHour; // 昼休み前の時間
            const afternoonHours = endHour - lunchEndHour; // 昼休み後の時間
            
            if (timeInHours < lunchStartHour) {
                // 昼休み前：開始時刻から昼休み開始時刻までの範囲で配置
                const morningRatio = (timeInHours - startHour) / morningHours;
                relativePosition = morningRatio * (morningHours / effectiveTotalHours);
            } else if (timeInHours >= lunchEndHour) {
                // 昼休み後：昼休み終了時刻から終了時刻までの範囲で配置
                const afternoonRatio = (timeInHours - lunchEndHour) / afternoonHours;
                const morningWidth = morningHours / effectiveTotalHours;
                relativePosition = morningWidth + afternoonRatio * (afternoonHours / effectiveTotalHours);
            } else {
                // 昼休み時間帯内：昼休み開始位置に配置（縦ライン用）
                relativePosition = morningHours / effectiveTotalHours;
            }
        } else {
            // 昼休み時間帯がない場合の通常の配置
            relativePosition = Math.max(0, Math.min(1, (timeInHours - startHour) / totalHours));
        }
        return businessStartX + relativePosition * barAreaWidth;
    };

    // スロットを分類（ビジネスアワー前/中/後/昼休み/時間外）
    // 時間外スロット（isOutsideHours = true）はグラフには描画せず、赤い縦ラインで存在の有無のみ表示
    const slotsBefore = []; // ビジネスアワー前
    const slotsInBusiness = []; // ビジネスアワー内（昼休み以外）
    const slotsAfter = []; // ビジネスアワー後
    const slotsLunch = []; // 昼休み時間帯
    const slotsOutsideHours = []; // 時間外スロット（早朝・昼休み・夕方）- グラフには描画しない
    
    // 時間外スロットの存在を追跡（縦ライン描画用のみ、グラフには描画しない）
    let hasOutsideHoursBefore = false; // 早朝スロット（businessStart以前）に件数がある
    let hasOutsideHoursAfter = false; // 夕方スロット（businessEnd以降）に件数がある
    let hasOutsideHoursLunch = false; // 昼休みスロット（lunchStart-lunchEnd）に件数がある

    validSlots.forEach(slot => {
        const timeRange = parseSlotTimeRange(slot, startHour, endHour);
        const slotStart = timeRange.start;
        const slotEnd = timeRange.end;
        const isOutsideHours = slot.isOutsideHours || false; // 時間外スロットフラグ
        const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;

        // 時間外スロットの判定（グラフには描画せず、赤い縦ラインで存在の有無のみ表示）
        if (isOutsideHours) {
            slotsOutsideHours.push(slot);
            // 時間外スロットに件数がある場合、対応する縦ラインを赤くする（グラフには描画しない）
            if (count > 0) {
                // 早朝スロット（businessStart以前）
                if (slotEnd <= startHour) {
                    hasOutsideHoursBefore = true;
                }
                // 夕方スロット（businessEnd以降）
                else if (slotStart >= endHour) {
                    hasOutsideHoursAfter = true;
                }
                // 昼休みスロット（lunchStart-lunchEnd）
                else if (lunchStartHour !== null && lunchEndHour !== null &&
                         slotStart >= lunchStartHour && slotEnd <= lunchEndHour) {
                    hasOutsideHoursLunch = true;
                }
            }
            return; // 時間外スロットはここで処理を終了（グラフには描画しない）
        }

        // 通常のスロットの分類
        // 昼休み時間帯かどうかを判定
        const isInLunchTime = lunchStartHour !== null && lunchEndHour !== null &&
            ((slotStart >= lunchStartHour && slotStart < lunchEndHour) ||
             (slotEnd > lunchStartHour && slotEnd <= lunchEndHour) ||
             (slotStart <= lunchStartHour && slotEnd >= lunchEndHour));

        if (slotEnd <= startHour) {
            // ビジネスアワー開始時刻より前
            slotsBefore.push(slot);
        } else if (slotStart >= endHour) {
            // ビジネスアワー終了時刻より後
            slotsAfter.push(slot);
        } else if (isInLunchTime) {
            // 昼休み時間帯（ビジネスアワー内だが昼休みに含まれる）
            slotsLunch.push(slot);
        } else {
            // ビジネスアワー内（昼休み以外、一部でも重なっていれば含める）
            slotsInBusiness.push(slot);
        }
    });

    // ビジネスアワー外と昼休みのスロットを集約（ただし描画はしない）
    const aggregatedBefore = aggregateSlots(slotsBefore);
    const aggregatedAfter = aggregateSlots(slotsAfter);
    const aggregatedLunch = aggregateSlots(slotsLunch);

    // 時間外スロットはグラフには描画しない（赤い縦ラインで存在の有無のみ表示）
    // フィルタリング処理は不要（縦ライン描画用のフラグで判定）

    // 描画対象のスロットリストを構築（集約されたスロットも含む、ただし昼休みは除外）
    const slotsToRender = [];
    if (aggregatedBefore) {
        slotsToRender.push({ ...aggregatedBefore, position: 'before' });
    }
    slotsInBusiness.forEach(slot => {
        slotsToRender.push({ ...slot, position: 'in' });
    });
    if (aggregatedAfter) {
        slotsToRender.push({ ...aggregatedAfter, position: 'after' });
    }
    // 時間外スロットはグラフには描画しない（赤い縦ラインで存在の有無のみ表示）
    // 昼休みのスロットは集約するが、描画はしない（グラフ幅を取らない）

    // 全スロットの最大値（max(cap, count)）を計算してスケーリングに使用
    let maxValue = 0;
    slotsToRender.forEach(slot => {
        const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
        const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
        maxValue = Math.max(maxValue, cap, count);
    });

    // 最大値が0の場合は描画しない
    if (maxValue <= 0) {
        return false;
    }

    // セルの下端（基準線）のY座標
    const baselineY = barAreaTop + barAreaHeight;

    // 集約スロット用の固定幅（ビジネスアワー外のスロット用）
    const aggregatedBarWidth = Math.max(2, Math.min(8, barAreaWidth * 0.05)); // 幅の5%、最小2px、最大8px

    slotsToRender.forEach((slot) => {
        // データの値検証とデフォルト値設定
        const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
        const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
        const available = cap - count; // 空き数
        const isSlotGrayed = slot.isGrayedOut || isDateGrayed;

        let barX, barWidth;

        if (slot.position === 'before') {
            // ビジネスアワー前の集約スロット：開始位置に配置
            barX = businessStartX;
            barWidth = aggregatedBarWidth;
        } else if (slot.position === 'after') {
            // ビジネスアワー後の集約スロット：終了位置に配置
            barX = businessEndX - aggregatedBarWidth;
            barWidth = aggregatedBarWidth;
        } else if (slot.position === 'outside-before') {
            // 早朝時間外スロット（07:00-09:00）：開始位置に配置
            barX = businessStartX;
            barWidth = aggregatedBarWidth;
        } else if (slot.position === 'outside-after') {
            // 夕方時間外スロット（17:00-18:00）：終了位置に配置
            barX = businessEndX - aggregatedBarWidth;
            barWidth = aggregatedBarWidth;
        } else if (slot.position === 'outside-lunch') {
            // 昼休み時間外スロット（12:00-13:00）：昼休み位置に配置
            if (lunchStartHour !== null) {
                const lunchStartX = timeToX(lunchStartHour);
                barX = lunchStartX;
                barWidth = aggregatedBarWidth;
            } else {
                return; // 昼休み時間が定義されていない場合は描画しない
            }
        } else {
            // ビジネスアワー内のスロット：時刻に基づいて配置（昼休みを除外）
            const timeRange = parseSlotTimeRange(slot, startHour, endHour);
            let slotStart = Math.max(startHour, timeRange.start);
            let slotEnd = Math.min(endHour, timeRange.end);
            
            // 昼休み時間帯と重なる場合は分割または調整
            if (lunchStartHour !== null && lunchEndHour !== null) {
                if (slotStart < lunchStartHour && slotEnd > lunchEndHour) {
                    // 昼休みをまたぐ場合：昼休み前と後の2つに分割（ここでは開始位置のみ使用）
                    slotStart = Math.max(startHour, slotStart);
                    slotEnd = Math.min(lunchStartHour, slotEnd);
                } else if (slotStart >= lunchStartHour && slotEnd <= lunchEndHour) {
                    // 完全に昼休み内：描画しない（既に除外されているはず）
                    return;
                } else if (slotStart < lunchStartHour && slotEnd > lunchStartHour) {
                    // 昼休み開始と重なる：昼休み開始まで
                    slotEnd = lunchStartHour;
                } else if (slotStart < lunchEndHour && slotEnd > lunchEndHour) {
                    // 昼休み終了と重なる：昼休み終了から
                    slotStart = lunchEndHour;
                }
            }
            
            const slotStartX = timeToX(slotStart);
            const slotEndX = timeToX(slotEnd);
            barX = slotStartX;
            barWidth = Math.max(1, slotEndX - slotStartX);
        }

        // キャパシティの位置に青いバーを描画
        if (cap > 0) {
            const capacityY = baselineY - (cap / maxValue) * barAreaHeight;
            const capacityBar = new Konva.Line({
                points: [barX, capacityY, barX + barWidth, capacityY],
                stroke: '#3b82f6', // 青色
                strokeWidth: 1,
                opacity: isSlotGrayed ? 0.4 : 0.8
            });
            layers.content.add(capacityBar);
        }

        // 空き数に基づいて棒グラフを描画
        if (available > 0) {
            // 空き数が正の場合: WinterColormapに基づく色の棒グラフを上方向に描画
            const barHeight = (available / maxValue) * barAreaHeight;
            const barY = baselineY - barHeight;
            const slotColor = isSlotGrayed ? '#9ca3af' : getWinterColorFromAvailable(available, cap);

            const bar = new Konva.Rect({
                x: barX,
                y: barY,
                width: Math.max(1, barWidth),
                height: barHeight,
                fill: slotColor,
                cornerRadius: Math.min(1, barHeight / 2),
                opacity: isSlotGrayed ? 0.4 : 1
            });
            layers.content.add(bar);
        } else if (available < 0) {
            // 空き数が負（オーバー）の場合: 赤色の棒グラフを下方向に描画
            const overflowAmount = Math.abs(available);
            const barHeight = (overflowAmount / maxValue) * barAreaHeight;
            const barY = baselineY; // セルの下端から開始

            const overflowBar = new Konva.Rect({
                x: barX,
                y: barY,
                width: Math.max(1, barWidth),
                height: barHeight,
                fill: '#ef4444', // 赤色
                cornerRadius: Math.min(1, barHeight / 2),
                opacity: isSlotGrayed ? 0.4 : 1
            });
            layers.content.add(overflowBar);
        }

        // 各スロットの開始時刻ラベルを描画（セルが十分大きい場合のみ）
        if (labelAreaHeight > 0 && slot.position === 'in') {
            // ビジネスアワー内のスロットのみラベルを表示
            const timeRange = parseSlotTimeRange(slot, startHour, endHour);
            const hourValue = Math.floor(timeRange.start);
            
            // ラベルのフォントサイズを開始・終了時刻ラベルと同じくらい小さく（6-7px程度）
            const labelFontSize = isYearView ? 6 : 7;
            
            // スロットの幅が狭い場合（二桁が入らない場合）は一の位のみを表示
            // 二桁が入るには最低でも12-14px程度の幅が必要なので、15px未満の場合は一の位のみ
            const canShowTwoDigits = barWidth >= 15;
            const labelText = canShowTwoDigits ? String(hourValue) : String(hourValue % 10);
            
            const labelY = baselineY + 2; // バーの下に2pxの余白を設ける
            const labelColor = isSlotGrayed ? '#d1d5db' : '#9ca3af';

            const slotLabel = new Konva.Text({
                x: barX,
                y: labelY,
                width: barWidth,
                text: labelText,
                fontSize: labelFontSize,
                fontFamily: CONFIG.font.numberFamily,
                fill: labelColor,
                align: 'center',
                wrap: 'none', // 改行を防ぐ
                opacity: isSlotGrayed ? 0.6 : 1
            });
            layers.content.add(slotLabel);
        }
    });

    // 業務時間の開始・終了位置に縦ラインを描画（昼休みの縦ラインと同様に常に表示）
    // 時間外スロットに件数がある場合は赤、ない場合はグレー
    {
        // 開始位置に縦ライン
        const beforeLineColor = hasOutsideHoursBefore ? '#ef4444' : '#d1d5db'; // 赤またはグレー
        const beforeLineWidth = hasOutsideHoursBefore ? 2 : 1;
        const beforeLine = new Konva.Line({
            points: [businessStartX, barAreaTop, businessStartX, barAreaTop + barAreaHeight],
            stroke: beforeLineColor,
            strokeWidth: beforeLineWidth,
            opacity: 0.8
        });
        layers.content.add(beforeLine);
    }

    {
        // 終了位置に縦ライン
        const afterLineColor = hasOutsideHoursAfter ? '#ef4444' : '#d1d5db'; // 赤またはグレー
        const afterLineWidth = hasOutsideHoursAfter ? 2 : 1;
        const afterLine = new Konva.Line({
            points: [businessEndX, barAreaTop, businessEndX, barAreaTop + barAreaHeight],
            stroke: afterLineColor,
            strokeWidth: afterLineWidth,
            opacity: 0.8
        });
        layers.content.add(afterLine);
    }

    // 昼休み時間帯の縦ラインを描画（月間・年間ビューの場合、1本だけ）
    if ((lunchStartHour !== null && lunchEndHour !== null) && (state.currentView === 'month' || state.currentView === 'year')) {
        // 昼休み開始時刻をX座標に変換（固定位置に合わせる）
        const lunchStartX = timeToX(lunchStartHour);

        // 昼休み時間帯にスロットがある場合、または時間外スロットがある場合は赤、ない場合はグレー
        const hasLunchSlots = slotsLunch.length > 0 || (aggregatedLunch && (aggregatedLunch.count > 0 || aggregatedLunch.cap > 0));
        const lunchLineColor = (hasLunchSlots || hasOutsideHoursLunch) ? '#ef4444' : '#d1d5db'; // 赤またはグレー
        const lunchLineWidth = (hasLunchSlots || hasOutsideHoursLunch) ? 2 : 1; // スロットがある場合は太く

        // 昼休み開始時刻の縦ライン（1本だけ）
        const lunchLine = new Konva.Line({
            points: [lunchStartX, barAreaTop, lunchStartX, barAreaTop + barAreaHeight],
            stroke: lunchLineColor,
            strokeWidth: lunchLineWidth,
            opacity: 0.8
        });
        layers.content.add(lunchLine);
    }

    return true;
}

