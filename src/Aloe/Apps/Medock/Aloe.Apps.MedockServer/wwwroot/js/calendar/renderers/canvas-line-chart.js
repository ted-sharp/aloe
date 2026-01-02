/**
 * Canvas Line Chart Renderer
 *
 * Equipmentリソースの空き率を時間軸に沿った折れ線グラフで描画
 * - Y軸: 0-100%（第二軸、右側に配置）
 * - 空き率: (available / capacity) × 100
 * - 複数リソース選択時: AND合成（最小空き率を採用）
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
 * 複数リソースのスロットをAND合成（最小空き率を計算）
 * @param {Array} resources - Equipmentリソースの配列
 * @returns {Array} 合成されたスロットデータ [{startHours, endHours, availabilityRate}, ...]
 */
function computeAndCompositeSlots(resources) {
    if (!resources || resources.length === 0) {
        return [];
    }

    // 単一リソースの場合はそのまま空き率を計算
    if (resources.length === 1) {
        const resource = resources[0];
        const { slotStarts, slotEnds, slotAvailables, slotCaps } = resource;
        if (!slotStarts || slotStarts.length === 0) {
            return [];
        }

        const result = [];
        for (let i = 0; i < slotStarts.length; i++) {
            const startMinutes = slotStarts[i];  // 既にint（分）
            const endMinutes = slotEnds[i];      // 既にint（分）
            const available = slotAvailables[i] || 0;
            const capacity = slotCaps[i] || 0;
            // 空き率を計算（オーバーブッキング時は負の値になり、下に突き抜けて表示される）
            const availabilityRate = capacity > 0 ? (available / capacity) * 100 : 0;

            result.push({
                startHours: startMinutes / 60,
                endHours: endMinutes / 60,
                availabilityRate: availabilityRate
            });
        }
        return result;
    }

    // 複数リソースの場合: 時間軸上で合成
    // 全リソースの時間帯境界を収集
    const timePoints = new Set();
    resources.forEach(resource => {
        const { slotStarts, slotEnds } = resource;
        if (!slotStarts) return;
        for (let i = 0; i < slotStarts.length; i++) {
            const startMinutes = slotStarts[i];  // 既にint（分）
            const endMinutes = slotEnds[i];      // 既にint（分）
            timePoints.add(startMinutes);
            timePoints.add(endMinutes);
        }
    });

    // 時間帯境界をソート
    const sortedTimePoints = Array.from(timePoints).sort((a, b) => a - b);
    if (sortedTimePoints.length < 2) {
        return [];
    }

    // 各時間区間で最小空き率を計算
    const result = [];
    for (let i = 0; i < sortedTimePoints.length - 1; i++) {
        const segmentStart = sortedTimePoints[i];
        const segmentEnd = sortedTimePoints[i + 1];
        const segmentMid = (segmentStart + segmentEnd) / 2;

        // この時間区間に含まれる各リソースの空き率を計算
        let minAvailabilityRate = 100; // 最大値から開始
        let hasData = false;

        resources.forEach(resource => {
            const { slotStarts, slotEnds, slotAvailables, slotCaps } = resource;
            if (!slotStarts) return;

            for (let j = 0; j < slotStarts.length; j++) {
                const slotStartMinutes = slotStarts[j].split(':').map(Number).reduce((h, m) => h * 60 + m);
                const slotEndMinutes = slotEnds[j].split(':').map(Number).reduce((h, m) => h * 60 + m);

                // この区間がスロットに含まれるかチェック
                if (segmentMid >= slotStartMinutes && segmentMid < slotEndMinutes) {
                    const available = slotAvailables[j] || 0;
                    const capacity = slotCaps[j] || 0;
                    // 空き率を計算（オーバーブッキング時は負の値になり、下に突き抜けて表示される）
                    const rate = capacity > 0 ? (available / capacity) * 100 : 0;
                    minAvailabilityRate = Math.min(minAvailabilityRate, rate);
                    hasData = true;
                    break; // このリソースでは1つのスロットのみ
                }
            }
        });

        if (hasData) {
            result.push({
                startHours: segmentStart / 60,
                endHours: segmentEnd / 60,
                availabilityRate: minAvailabilityRate
            });
        }
    }

    return result;
}

/**
 * 折れ線グラフを描画（0-100%空き率、AND合成）
 * @param {CanvasRenderingContext2D} contentCtx - コンテンツレイヤーコンテキスト
 * @param {object} params - パラメータ
 */
export function renderCanvasLineChart(contentCtx, params) {
    const {
        cellLeft, cellTop, cellWidth, cellHeight,
        dateStr, barAreaTop, barAreaHeight,
        equipmentStats, startHour, endHour,
        lunchStartHour, lunchEndHour, isYearView = false
    } = params;

    // Equipment統計データがない場合は描画しない
    if (!equipmentStats) {
        return false;
    }

    const resources = Object.values(equipmentStats);
    if (resources.length === 0) {
        return false;
    }

    // AND合成で最小空き率のスロットデータを計算
    const compositeSlots = computeAndCompositeSlots(resources);
    if (compositeSlots.length === 0) {
        return false;
    }

    const barAreaWidth = cellWidth - 4;
    const businessStartX = cellLeft + 2;
    const baselineY = barAreaTop + barAreaHeight;

    // 折れ線の色（単一色、Equipment用）
    const lineColor = '#f59e0b'; // amber-500

    // データポイントを生成（プラトー形状）
    const dataPoints = [];
    compositeSlots.forEach(slot => {
        const startX = timeToX(
            slot.startHours, startHour, endHour,
            lunchStartHour, lunchEndHour,
            businessStartX, barAreaWidth
        );
        const endX = timeToX(
            slot.endHours, startHour, endHour,
            lunchStartHour, lunchEndHour,
            businessStartX, barAreaWidth
        );

        // Y座標: 0-100%スケール（0%が下、100%が上）
        const y = baselineY - (slot.availabilityRate / 100) * barAreaHeight;

        dataPoints.push({ x: startX, y: y });
        dataPoints.push({ x: endX, y: y });
    });

    // 折れ線を描画
    if (dataPoints.length > 1) {
        for (let i = 0; i < dataPoints.length - 1; i++) {
            const p1 = dataPoints[i];
            const p2 = dataPoints[i + 1];

            drawLine(contentCtx, {
                points: [p1.x, p1.y, p2.x, p2.y],
                stroke: lineColor,
                strokeWidth: 2,
                opacity: 0.8
            });
        }
    }

    // データポイントを描画（年間ビューでは小さく）
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
