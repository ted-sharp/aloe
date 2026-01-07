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
        // NOTE: DTO uses slotStartMins, slotEndMins (integers)
        const { slotStartMins, slotEndMins, slotAvailables, slotCaps } = resource;
        if (!slotStartMins || slotStartMins.length === 0) {
            return [];
        }

        const result = [];
        for (let i = 0; i < slotStartMins.length; i++) {
            const startMin = slotStartMins[i];
            const endMin = slotEndMins[i];
            const available = slotAvailables[i] || 0;
            const capacity = slotCaps[i] || 0;
            // 空き率を計算
            const availabilityRate = capacity > 0 ? (available / capacity) * 100 : 0;

            result.push({
                startHours: startMin / 60,
                endHours: endMin / 60,
                availabilityRate: availabilityRate
            });
        }
        return result;
    }

    // 複数リソースの場合: 時間軸上で合成
    // 全リソースの時間帯境界を収集
    const timePoints = new Set();
    resources.forEach(resource => {
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

    // 各時間区間で最小空き率を計算
    const result = [];
    for (let i = 0; i < sortedTimePoints.length - 1; i++) {
        const segmentStart = sortedTimePoints[i];
        const segmentEnd = sortedTimePoints[i + 1];
        const segmentMid = (segmentStart + segmentEnd) / 2;

        // この時間区間に含まれる各リソースの空き率を計算
        let minAvailabilityRate = 100; // 最良の状態（空き100%）からスタート
        let hasData = false;

        // AND合成: 全リソースが利用可能である必要がある
        // 空き率 = min(各リソースの空き率)
        // ただし、もしあるリソースにその時間帯のスロットが存在しない(=休み？)場合はどうする？
        // ここでは「スロットが定義されているリソース」の間でのANDをとる
        // （スロットがない=利用不可なら0%になるが、ここではスロットリストに含まれるかチェックしている）

        resources.forEach(resource => {
            const { slotStartMins, slotEndMins, slotAvailables, slotCaps } = resource;
            if (!slotStartMins) return;

            for (let j = 0; j < slotStartMins.length; j++) {
                const startMin = slotStartMins[j];
                const endMin = slotEndMins[j];

                // この区間がスロットに含まれるかチェック
                if (segmentMid >= startMin && segmentMid < endMin) {
                    const available = slotAvailables[j] || 0;
                    const capacity = slotCaps[j] || 0;
                    const rate = capacity > 0 ? (available / capacity) * 100 : 0;

                    minAvailabilityRate = Math.min(minAvailabilityRate, rate);
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
    const lineColor = '#fbbf24'; // amber-400 (少し明るく)

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

        // timeプロパティを追加（ギャップ検知用）
        dataPoints.push({ x: startX, y: y, time: slot.startHours });
        dataPoints.push({ x: endX, y: y, time: slot.endHours });
    });

    // 折れ線を描画
    if (dataPoints.length > 1) {
        for (let i = 0; i < dataPoints.length - 1; i++) {
            const p1 = dataPoints[i];
            const p2 = dataPoints[i + 1];

            // 接続チェック
            let isConnected = true;

            // 奇数インデックス（p1=End, p2=Start）はスロット間の接続
            // 時間差がある場合（ギャップ）は接続しない
            if (i % 2 === 1) {
                // 許容誤差 0.01時間 = 36秒
                if (Math.abs(p2.time - p1.time) > 0.01) {
                    isConnected = false;
                }
            }

            if (isConnected) {
                drawLine(contentCtx, {
                    points: [p1.x, p1.y, p2.x, p2.y],
                    stroke: lineColor,
                    strokeWidth: 2,
                    opacity: 0.8
                });
            }
        }
    }

    // データポイントを描画
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
