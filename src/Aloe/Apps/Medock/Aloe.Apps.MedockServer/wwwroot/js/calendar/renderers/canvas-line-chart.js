/**
 * Canvas Line Chart Renderer
 *
 * EquipmentリソースとMainリソースの空き件数を時間軸に沿った折れ線グラフで描画
 * - Y軸: 0-最大空き件数
 * - 空き件数: SlotCap - SlotCount（フィルター時はSlotCap - SlotFilteredCount）
 * - 複数リソース選択時: AND合成（最小空き件数を採用）
 * - Mainリソースも含めたAND条件で計算
 */

import { drawLine, drawCircle } from '../utils/canvas-utils.js';

/**
 * 時刻をX座標に変換する関数（棒グラフと同じロジック）
 * @param {number} timeInHours - 時刻（時間単位、例：9.5 = 9:30）
 * @param {number} startHour - 開始時刻
 * @param {number} endHour - 終了時刻
 * @param {number|null} lunchStartHour - 昼休み開始時刻
 * @param {number|null} lunchEndHour - 昼休み終了時刻
 * @param {number} businessStartX - 業務時間開始X座標
 * @param {number} barAreaWidth - バーエリアの幅
 * @returns {number} X座標
 */
function timeToX(timeInHours, startHour, endHour, lunchStartHour, lunchEndHour, businessStartX, barAreaWidth) {
    const totalHours = endHour - startHour;
    const lunchDuration = (lunchStartHour !== null && lunchEndHour !== null)
        ? (lunchEndHour - lunchStartHour)
        : 0;
    const effectiveTotalHours = totalHours - lunchDuration;

    let relativePosition;
    if (lunchStartHour !== null && lunchEndHour !== null && effectiveTotalHours > 0) {
        const morningHours = lunchStartHour - startHour;
        const afternoonHours = endHour - lunchEndHour;

        if (timeInHours < lunchStartHour) {
            const morningRatio = (timeInHours - startHour) / morningHours;
            relativePosition = morningRatio * (morningHours / effectiveTotalHours);
        } else if (timeInHours >= lunchEndHour) {
            const afternoonRatio = (timeInHours - lunchEndHour) / afternoonHours;
            const morningWidth = morningHours / effectiveTotalHours;
            relativePosition = morningWidth + afternoonRatio * (afternoonHours / effectiveTotalHours);
        } else {
            relativePosition = morningHours / effectiveTotalHours;
        }
    } else {
        relativePosition = Math.max(0, Math.min(1, (timeInHours - startHour) / totalHours));
    }
    return businessStartX + relativePosition * barAreaWidth;
}

/**
 * 空き件数を取得する（フィルター時はSlotFilteredCountsを使用）
 * @param {object} resource - リソースデータ
 * @param {number} index - スロットインデックス
 * @returns {number} 空き件数
 */
function getAvailableCount(resource, index) {
    const slotCap = resource.slotCaps?.[index] || 0;
    const slotCount = resource.slotCounts?.[index] || 0;
    const slotFilteredCount = resource.slotFilteredCounts?.[index];
    
    // フィルターが有効な場合（SlotFilteredCountsが存在する場合）はそれを使用
    if (slotFilteredCount !== undefined && slotFilteredCount !== null) {
        return Math.max(0, slotCap - slotFilteredCount);
    }
    
    // 通常はSlotAvailablesを使用（既に計算済み）
    const slotAvailable = resource.slotAvailables?.[index] || 0;
    return slotAvailable;
}

/**
 * MainリソースとEquipmentリソースのスロットをAND合成（最小空き件数を計算）
 * @param {object|null} mainStats - Mainリソース統計データ
 * @param {Array} equipmentResources - Equipmentリソースの配列
 * @param {number} startHour - 業務開始時刻（時間単位）
 * @param {number} endHour - 業務終了時刻（時間単位）
 * @param {number|null} lunchStartHour - 昼休み開始時刻（時間単位、nullの場合は昼休みなし）
 * @param {number|null} lunchEndHour - 昼休み終了時刻（時間単位、nullの場合は昼休みなし）
 * @returns {Array} 合成されたスロットデータ [{startHours, endHours, availableCount, centerHours}, ...]
 */
function computeAndCompositeSlotsWithMain(mainStats, equipmentResources, startHour, endHour, lunchStartHour, lunchEndHour) {
    const allResources = [];
    
    // Mainリソースを追加
    if (mainStats) {
        allResources.push(mainStats);
    }
    
    // Equipmentリソースを追加
    if (equipmentResources && equipmentResources.length > 0) {
        allResources.push(...equipmentResources);
    }
    
    if (allResources.length === 0) {
        return [];
    }
    
    // 全リソースの時間帯境界を収集
    const timePoints = new Set();
    allResources.forEach(resource => {
        const { slotStartMins, slotEndMins } = resource;
        if (!slotStartMins) return;
        for (let i = 0; i < slotStartMins.length; i++) {
            timePoints.add(slotStartMins[i]);
            timePoints.add(slotEndMins[i]);
        }
    });
    
    // 時間帯境界をソート
    const sortedTimePoints = Array.from(timePoints).sort((a, b) => a - b);
    if (sortedTimePoints.length < 2) {
        return [];
    }
    
    // 各時間区間で最小空き件数を計算
    const result = [];
    for (let i = 0; i < sortedTimePoints.length - 1; i++) {
        const segmentStart = sortedTimePoints[i];
        const segmentEnd = sortedTimePoints[i + 1];
        const segmentMid = (segmentStart + segmentEnd) / 2;
        const centerHours = segmentMid / 60;
        
        // 時間外のスロットを除外
        // 1. ビジネスアワー外の判定: startHourより前、endHour以降、または昼休み時間内
        const segmentStartHour = segmentStart / 60;
        const segmentEndHour = segmentEnd / 60;
        const isBeforeBusinessHours = segmentEndHour < startHour;
        const isAfterBusinessHours = segmentStartHour >= endHour;
        const isDuringLunch = lunchStartHour !== null && lunchEndHour !== null &&
            segmentStartHour < lunchEndHour && segmentEndHour > lunchStartHour;
        
        // ビジネスアワー外のスロットは描画対象から除外
        if (isBeforeBusinessHours || isAfterBusinessHours || isDuringLunch) {
            continue;
        }
        
        // 2. スロット定義から外れている時間帯を時間外として除外
        // セグメントの中心時刻が、いずれかのリソースのスロット定義に含まれているかチェック
        let isInAnySlotDefinition = false;
        allResources.forEach(resource => {
            const { slotStartMins, slotEndMins } = resource;
            if (!slotStartMins) return;
            
            for (let j = 0; j < slotStartMins.length; j++) {
                const startMin = slotStartMins[j];
                const endMin = slotEndMins[j];
                
                // セグメントの中心時刻がスロット定義に含まれているかチェック
                if (segmentMid >= startMin && segmentMid < endMin) {
                    isInAnySlotDefinition = true;
                    return; // このリソースで見つかったので、次のリソースをチェック
                }
            }
        });
        
        // すべてのリソースのスロット定義から外れている場合は時間外として除外
        // （例：水曜日・土曜日の午後はスロット定義が存在しないため、時間外として扱う）
        if (!isInAnySlotDefinition) {
            continue;
        }
        
        // この時間区間に含まれる各リソースの空き件数を計算
        let minAvailableCount = Infinity;
        let hasData = false;
        
        // AND合成: 全リソースが利用可能である必要がある
        // 空き件数 = min(各リソースの空き件数)
        allResources.forEach(resource => {
            const { slotStartMins, slotEndMins, slotFlags } = resource;
            if (!slotStartMins) return;
            
            for (let j = 0; j < slotStartMins.length; j++) {
                const startMin = slotStartMins[j];
                const endMin = slotEndMins[j];
                
                // この区間がスロットに含まれるかチェック
                if (segmentMid >= startMin && segmentMid < endMin) {
                    // SlotFlagsで時間外を判定（存在する場合）
                    // オーバーライドやその他の理由で時間外としてマークされている場合
                    if (slotFlags && slotFlags.length > j) {
                        const flags = slotFlags[j];
                        const isOutsideBefore = (flags & 0b0010) !== 0;
                        const isOutsideAfter = (flags & 0b0100) !== 0;
                        const isOutsideLunch = (flags & 0b1000) !== 0;
                        if (isOutsideBefore || isOutsideAfter || isOutsideLunch) {
                            // 時間外スロットは除外
                            continue;
                        }
                    }
                    
                    const availableCount = getAvailableCount(resource, j);
                    minAvailableCount = Math.min(minAvailableCount, availableCount);
                    hasData = true;
                    // このリソースについてはこの時間帯の有効なスロットが見つかったのでbreak
                    break;
                }
            }
        });
        
        if (hasData) {
            result.push({
                startHours: segmentStart / 60,
                endHours: segmentEnd / 60,
                availableCount: minAvailableCount,
                centerHours: centerHours
            });
        }
    }
    
    return result;
}

/**
 * 折れ線グラフを描画（0-最大空き件数、AND合成、Mainリソース含む）
 * @param {CanvasRenderingContext2D} contentCtx - コンテンツレイヤーコンテキスト
 * @param {object} params - パラメータ
 */
export function renderCanvasLineChart(contentCtx, params) {
    const {
        cellLeft, cellTop, cellWidth, cellHeight,
        dateStr, barAreaTop, barAreaHeight,
        equipmentStats, mainStats, startHour, endHour,
        lunchStartHour, lunchEndHour, isYearView = false,
        businessHours = null
    } = params;

    // Equipment統計データがない場合は描画しない
    if (!equipmentStats) {
        return false;
    }

    const equipmentResources = Object.values(equipmentStats);
    if (equipmentResources.length === 0) {
        return false;
    }

    // Mainリソース統計データを取得（dateStrに対応するデータ）
    const mainStatsForDate = mainStats || null;

    // AND合成で最小空き件数のスロットデータを計算（Mainリソースも含む）
    const compositeSlots = computeAndCompositeSlotsWithMain(mainStatsForDate, equipmentResources, startHour, endHour, lunchStartHour, lunchEndHour);
    if (compositeSlots.length === 0) {
        return false;
    }

    // 最大空き件数を計算（Y軸スケール用）
    let maxAvailableCount = 0;
    compositeSlots.forEach(slot => {
        maxAvailableCount = Math.max(maxAvailableCount, slot.availableCount);
    });
    
    // 最大空き件数が0の場合は描画しない
    if (maxAvailableCount === 0) {
        return false;
    }

    const barAreaWidth = cellWidth - 4;
    const businessStartX = cellLeft + 2;
    const baselineY = barAreaTop + barAreaHeight;

    // 折れ線の色（単一色、Equipment用）
    const lineColor = '#fbbf24'; // amber-400 (少し明るく)

    // データポイントを生成（一番小さいスロットの分割範囲の中心箇所に点を打つ）
    const dataPoints = [];
    compositeSlots.forEach(slot => {
        // セグメントの中心時刻をX座標として使用
        const centerX = timeToX(
            slot.centerHours, startHour, endHour,
            lunchStartHour, lunchEndHour,
            businessStartX, barAreaWidth
        );

        // Y座標: 0-最大空き件数スケール（0件が下、最大件数が上）
        const y = baselineY - (slot.availableCount / maxAvailableCount) * barAreaHeight;

        dataPoints.push({ 
            x: centerX, 
            y: y, 
            time: slot.centerHours,
            availableCount: slot.availableCount
        });
    });

    // データポイントを時間順にソート
    dataPoints.sort((a, b) => a.time - b.time);

    // 折れ線を描画（すべての点を順番につなぐ）
    if (dataPoints.length > 1) {
        // すべての点を順番につなぐ折れ線を描画
        const linePoints = [];
        dataPoints.forEach(point => {
            linePoints.push(point.x, point.y);
        });
        
        // 折れ線を描画
        if (linePoints.length >= 4) {
            drawLine(contentCtx, {
                points: linePoints,
                stroke: lineColor,
                strokeWidth: 2,
                opacity: 0.8
            });
        }
    }

    // データポイントを描画（一番小さいスロットの分割範囲の中心箇所に点を打つ）
    dataPoints.forEach(point => {
        drawCircle(contentCtx, {
            x: point.x,
            y: point.y,
            radius: isYearView ? 1.5 : 2,
            fill: lineColor,
            opacity: 0.9
        });
    });

    return true;
}
