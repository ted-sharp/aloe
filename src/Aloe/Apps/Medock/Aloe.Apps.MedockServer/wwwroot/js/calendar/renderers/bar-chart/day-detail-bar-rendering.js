/**
 * Day Detail Bar Chart Rendering
 *
 * ポップアップ/モーダル用の棒グラフ描画（詳細版）
 * - 使用場面: 日詳細ポップアップ、モーダル
 * - 特徴: スロットを集約せず個別に表示（GraphData通りのスロットを描画）
 * - 関数: renderDetailBarChart()
 *
 * 注意: 年間/月間カレンダーでは使用しない（day-calendar-bar-rendering.jsを使用）
 */

import { getState } from '../../state.js';
import { CONFIG } from '../../config.js';
import { parseSlotTimeRange } from './slot-time-utils.js';
import { getWinterColorFromAvailable } from '../../utils/winter-colormap.js';

/**
 * 時刻（時間単位）をHH:mm形式の文字列に変換
 * @param {number} hours - 時刻（時間単位、例：9.5 = 9:30）
 * @returns {string} HH:mm形式の時刻文字列
 */
function formatTime(hours) {
    const h = Math.floor(hours);
    const m = Math.round((hours - h) * 60);
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`;
}

/**
 * 背景色に応じたテキスト色を取得
 * 暗い色（青系）の場合は白、明るい色（緑系）の場合は黒
 * @param {string} bgColor - 背景色（#RRGGBB形式）
 * @returns {string} テキスト色（#RRGGBB形式）
 */
function getTextColorForBackground(bgColor) {
    // #を除去してRGB値を取得
    const hex = bgColor.replace('#', '');
    if (hex.length !== 6) {
        // 不正な形式の場合はデフォルトの黒を返す
        return '#000000';
    }
    
    const r = parseInt(hex.substring(0, 2), 16);
    const g = parseInt(hex.substring(2, 4), 16);
    const b = parseInt(hex.substring(4, 6), 16);
    
    // 輝度を計算（0.299*R + 0.587*G + 0.114*B）
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
    
    // 輝度が0.5未満（暗い）の場合は白、それ以外は黒
    return luminance < 0.5 ? '#ffffff' : '#000000';
}

/**
 * 詳細棒グラフを描画（集約しないバージョン）
 * GraphData通りのスロットを個別に描画
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
export function renderDetailBarChart({
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
    const yAxisWidth = 30; // Y軸ラベル用スペース
    const barAreaWidth = cellWidth - 4 - yAxisWidth; // 左右余白2px + Y軸スペース

    // ビジネスアワーの開始・終了位置を固定X座標として計算
    const businessStartX = cellLeft + 2 + yAxisWidth; // 開始位置（Y軸スペース分右にオフセット）
    const businessEndX = cellLeft + 2 + yAxisWidth + barAreaWidth; // 終了位置
    
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

        // イレギュラースロット（isOutsideHours）を追跡
        if (isOutsideHours) {
            slotsOutsideHours.push(slot);
            // 時間外スロットの存在を追跡（縦ライン描画用）
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
        }

        // イレギュラースロットも含めて全てのスロットを分類
        // 昼休み時間帯かどうかを判定
        const isInLunchTime = lunchStartHour !== null && lunchEndHour !== null &&
            ((slotStart >= lunchStartHour && slotStart < lunchEndHour) ||
             (slotEnd > lunchStartHour && slotEnd <= lunchEndHour) ||
             (slotStart <= lunchStartHour && slotEnd >= lunchEndHour));

        if (slotEnd < startHour || slotStart < startHour) {
            // ビジネスアワー開始時刻より前（完全に前、または開始時刻より前から始まる）
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

    // 集約せず、各スロットを個別に描画対象に追加（ビジネスアワー前/後のスロットは棒グラフとして描画せず、縦ラインのみで表示）
    const slotsToRender = [];
    // ビジネスアワー前/後のスロットは棒グラフとして描画せず、縦ラインのみで表示
    // ビジネスアワー内のスロットを個別に追加
    slotsInBusiness.forEach(slot => {
        slotsToRender.push({ ...slot, position: 'in' });
    });
    // 時間外スロットはグラフには描画しない（赤い縦ラインで存在の有無のみ表示）
    // 昼休みの通常スロットは描画しない（グラフ幅を取らない）

    // 全スロットの最大値（capの最大値）を計算してスケーリングに使用
    let maxValue = 0;
    slotsToRender.forEach(slot => {
        const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
        maxValue = Math.max(maxValue, cap);
    });

    // 最大値が0の場合は描画しない
    if (maxValue <= 0) {
        return false;
    }

    // セルの下端（基準線）のY座標
    const baselineY = barAreaTop + barAreaHeight;

    // Y軸メモリを描画（セル内に配置）
    const yAxisLabelWidth = 28; // Y軸ラベルの幅
    const yAxisLeft = cellLeft + 2; // Y軸の左端位置（セル内左側）
    const numTicks = 5; // メモリの数
    const tickFontSize = isYearView ? 8 : 10;

    for (let i = 0; i <= numTicks; i++) {
        const value = (maxValue / numTicks) * i;
        const y = baselineY - (value / maxValue) * barAreaHeight;

        // メモリ線を描画（グラフエリアの左端に）
        const tickLine = new Konva.Line({
            points: [businessStartX - 6, y, businessStartX, y],
            stroke: '#9ca3af',
            strokeWidth: 1,
            opacity: 0.8
        });
        layers.content.add(tickLine);

        // Y軸メモリ位置に横線を描画（グラフエリア全体に）
        const gridLine = new Konva.Line({
            points: [businessStartX, y, businessEndX, y],
            stroke: '#d1d5db',
            strokeWidth: 1,
            opacity: 0.8
        });
        layers.content.add(gridLine);

        // 値のラベルを描画
        if (i > 0 || value === 0) { // 0または値がある場合のみ表示
            const tickLabel = new Konva.Text({
                x: yAxisLeft,
                y: y - tickFontSize / 2,
                width: yAxisLabelWidth - 4,
                text: String(Math.round(value)),
                fontSize: tickFontSize,
                fontFamily: CONFIG.font.numberFamily,
                fill: isDateGrayed ? '#9ca3af' : '#6b7280',
                align: 'right',
                verticalAlign: 'middle',
                wrap: 'none',
                opacity: isDateGrayed ? 0.6 : 0.9
            });
            layers.content.add(tickLabel);
        }
    }

    // Y=0の位置に基準線を描画（他のメモリ線と同じスタイル）
    const baselineGridLine = new Konva.Line({
        points: [businessStartX, baselineY, businessEndX, baselineY],
        stroke: '#d1d5db',
        strokeWidth: 1,
        opacity: 0.8
    });
    layers.content.add(baselineGridLine);

    // ビジネスアワー外のスロット用の固定幅
    const outsideBarWidth = Math.max(2, Math.min(8, barAreaWidth * 0.05)); // 幅の5%、最小2px、最大8px

    slotsToRender.forEach((slot) => {
        // データの値検証とデフォルト値設定
        const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
        const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
        const available = cap - count; // 空き数
        const isSlotGrayed = slot.isGrayedOut || isDateGrayed;

        let barX, barWidth;

        if (slot.position === 'before') {
            // ビジネスアワー前のスロット：開始位置に配置
            barX = businessStartX;
            barWidth = outsideBarWidth;
        } else if (slot.position === 'after') {
            // ビジネスアワー後のスロット：終了位置に配置
            barX = businessEndX - outsideBarWidth;
            barWidth = outsideBarWidth;
        } else if (slot.position === 'lunch') {
            // 昼休み時間帯のイレギュラースロット：昼休み開始位置に配置
            if (lunchStartHour !== null) {
                const lunchStartX = timeToX(lunchStartHour);
                barX = lunchStartX;
                barWidth = outsideBarWidth;
            } else {
                return; // 昼休み時間が定義されていない場合は描画しない
            }
        } else {
            // ビジネスアワー内のスロット：時刻に基づいて配置（昼休みを除外）
            const timeRange = parseSlotTimeRange(slot, startHour, endHour);
            let slotStart = timeRange.start; // イレギュラースロットも含めて実際の開始時刻を使用
            let slotEnd = timeRange.end; // イレギュラースロットも含めて実際の終了時刻を使用
            
            // ビジネスアワー外のスロットも描画するため、範囲制限を緩和
            // ただし、グラフエリア内に収まるように調整
            const actualSlotStart = slotStart;
            const actualSlotEnd = slotEnd;
            
            // 昼休み時間帯と重なる場合は分割または調整
            if (lunchStartHour !== null && lunchEndHour !== null) {
                if (actualSlotStart < lunchStartHour && actualSlotEnd > lunchEndHour) {
                    // 昼休みをまたぐ場合：昼休み前と後の2つに分割（ここでは開始位置のみ使用）
                    slotStart = Math.max(startHour, actualSlotStart);
                    slotEnd = Math.min(lunchStartHour, actualSlotEnd);
                } else if (actualSlotStart >= lunchStartHour && actualSlotEnd <= lunchEndHour) {
                    // 完全に昼休み内：描画しない（既に除外されているはず）
                    return;
                } else if (actualSlotStart < lunchStartHour && actualSlotEnd > lunchStartHour) {
                    // 昼休み開始と重なる：昼休み開始まで
                    slotStart = Math.max(startHour, actualSlotStart);
                    slotEnd = lunchStartHour;
                } else if (actualSlotStart < lunchEndHour && actualSlotEnd > lunchEndHour) {
                    // 昼休み終了と重なる：昼休み終了から
                    slotStart = lunchEndHour;
                    slotEnd = Math.min(endHour, actualSlotEnd);
                } else {
                    // 昼休みと重ならない場合
                    slotStart = Math.max(startHour, Math.min(endHour, actualSlotStart));
                    slotEnd = Math.max(startHour, Math.min(endHour, actualSlotEnd));
                }
            } else {
                // 昼休み時間帯がない場合
                slotStart = Math.max(startHour, Math.min(endHour, actualSlotStart));
                slotEnd = Math.max(startHour, Math.min(endHour, actualSlotEnd));
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
        let barTopY = baselineY; // バーの上端Y座標（デフォルトは基準線）
        if (available > 0) {
            // 空き数が正の場合: WinterColormapに基づく色の棒グラフを上方向に描画
            const barHeight = (available / maxValue) * barAreaHeight;
            const barY = baselineY - barHeight;
            barTopY = barY; // バーの上端を記録
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
            barTopY = barY; // バーの上端を記録

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
        } else {
            // 空き数が0（0%）の場合：capの値に関係なく、常に一番下に配置
            barTopY = baselineY;
        }

        // n/m表記とパーセンテージをグラフ上（バーの上）に大きく表示
        if (cap > 0 && barWidth >= 25) {
            // n/m表記と空き率%を計算
            const vacancyRate = Math.round((available / cap) * 100);
            const countCapText = `${count}/${cap}`;
            const vacancyText = `${vacancyRate}%`;
            
            // グラフ上のラベルのフォントサイズ（大きく表示）
            const graphLabelFontSize = isYearView ? 12 : 16;
            
            // バーの上に配置（バーの上端から十分な余白を取る）
            const labelSpacing = 20; // バーの上端からの余白
            const lineHeight = graphLabelFontSize + 4; // 行の高さ
            
            // グラフエリアの上端を超えないように調整
            const minLabelY = barAreaTop - lineHeight * 2 - labelSpacing;
            
            // テキスト色は常に黒（背景色に関係なく）
            const graphLabelColor = isSlotGrayed ? '#6b7280' : '#374151';
            
            // スロットの幅に応じて表示内容を調整
            const canShowFullInfo = barWidth >= 35;
            const canShowCountCap = barWidth >= 25;
            
            // n/m表記を描画（1行目）
            if (canShowCountCap) {
                let countCapLabelY = barTopY - labelSpacing - lineHeight;
                // グラフエリアの上端を超えないように調整
                countCapLabelY = Math.max(minLabelY, countCapLabelY);
                const countCapLabel = new Konva.Text({
                    x: barX,
                    y: countCapLabelY,
                    width: barWidth,
                    text: countCapText,
                    fontSize: graphLabelFontSize,
                    fontFamily: CONFIG.font.numberFamily,
                    fill: graphLabelColor,
                    align: 'center',
                    verticalAlign: 'bottom',
                    wrap: 'none',
                    opacity: isSlotGrayed ? 0.7 : 1
                });
                layers.content.add(countCapLabel);
            }
            
            // パーセンテージを描画（2行目）
            if (canShowFullInfo) {
                let vacancyLabelY = barTopY - labelSpacing;
                // グラフエリアの上端を超えないように調整
                vacancyLabelY = Math.max(minLabelY + lineHeight, vacancyLabelY);
                const vacancyLabel = new Konva.Text({
                    x: barX,
                    y: vacancyLabelY,
                    width: barWidth,
                    text: vacancyText,
                    fontSize: graphLabelFontSize,
                    fontFamily: CONFIG.font.numberFamily,
                    fill: graphLabelColor,
                    align: 'center',
                    verticalAlign: 'bottom',
                    wrap: 'none',
                    opacity: isSlotGrayed ? 0.7 : 1
                });
                layers.content.add(vacancyLabel);
            }
        }

        // 各スロットの開始時刻ラベルを下部に描画（セルが十分大きい場合のみ）
        if (labelAreaHeight > 0) {
            const timeRange = parseSlotTimeRange(slot, startHour, endHour);
            const slotStartTime = timeRange.start;
            
            // 開始時刻をHH:mm形式で取得
            const timeText = formatTime(slotStartTime);
            
            // ラベルのフォントサイズ（大きく表示）
            const labelFontSize = isYearView ? 10 : 13;
            
            // ラベルの配置：バーの下に表示
            const labelY = baselineY + 8; // バーの下に余白を設ける
            const labelColor = isSlotGrayed ? '#d1d5db' : '#9ca3af';
            
            // スロットの幅に応じて表示内容を調整
            const canShowTime = barWidth >= 30;
            
            // 時刻ラベルを描画
            if (canShowTime) {
                const slotLabel = new Konva.Text({
                    x: barX,
                    y: labelY,
                    width: barWidth,
                    text: timeText,
                    fontSize: labelFontSize,
                    fontFamily: CONFIG.font.numberFamily,
                    fill: labelColor,
                    align: 'center',
                    wrap: 'none', // 改行を防ぐ
                    opacity: isSlotGrayed ? 0.6 : 1
                });
                layers.content.add(slotLabel);
            }
        }

        // スロット間の区切り線を描画
        const slotSeparatorLine = new Konva.Line({
            points: [barX + barWidth, barAreaTop, barX + barWidth, baselineY],
            stroke: '#e5e7eb',
            strokeWidth: 1,
            opacity: 0.8
        });
        layers.content.add(slotSeparatorLine);
    });

    // 業務時間の開始・終了位置に縦ラインを描画（昼休みの縦ラインと同様に常に表示）
    // 時間外スロットまたは通常のビジネスアワー前/後のスロットに件数がある場合は赤、ない場合はグレー
    {
        // 開始位置に縦ライン
        const hasBeforeSlots = slotsBefore.length > 0 && slotsBefore.some(slot => {
            const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
            const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
            return count > 0 || cap > 0;
        });
        const beforeLineColor = (hasOutsideHoursBefore || hasBeforeSlots) ? '#ef4444' : '#d1d5db'; // 赤またはグレー
        const beforeLineWidth = (hasOutsideHoursBefore || hasBeforeSlots) ? 2 : 1;
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
        const hasAfterSlots = slotsAfter.length > 0 && slotsAfter.some(slot => {
            const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
            const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
            return count > 0 || cap > 0;
        });
        const afterLineColor = (hasOutsideHoursAfter || hasAfterSlots) ? '#ef4444' : '#d1d5db'; // 赤またはグレー
        const afterLineWidth = (hasOutsideHoursAfter || hasAfterSlots) ? 2 : 1;
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
        const hasLunchSlots = slotsLunch.length > 0;
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

