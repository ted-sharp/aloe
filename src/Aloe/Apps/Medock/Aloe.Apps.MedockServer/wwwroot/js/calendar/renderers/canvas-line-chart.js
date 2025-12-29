/**
 * Canvas Line Chart Renderer
 * 
 * Equipmentリソースの空き数（available）を時間軸に沿った折れ線グラフで描画
 * Mainリソースの棒グラフと同じ時間軸を使用
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
 * 時間文字列（"HH:mm"）を時間数に変換
 * @param {string} timeStr - 時間文字列（"09:30"形式）
 * @returns {number} 時間数（例：9.5）
 */
function parseTime(timeStr) {
    const parts = timeStr.split(':');
    return parseInt(parts[0], 10) + (parseInt(parts[1] || 0, 10) / 60);
}

/**
 * 折れ線グラフを描画（時間軸に沿った折れ線グラフ）
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
    if (!equipmentStats || !equipmentStats.resources) {
        return false;
    }

    const resources = Object.values(equipmentStats.resources);
    if (resources.length === 0) {
        return false;
    }

    const barAreaWidth = cellWidth - 4;
    const businessStartX = cellLeft + 2;
    const baselineY = barAreaTop + barAreaHeight;

    // 各Equipmentリソースのスロットデータから最大available値を計算
    let maxAvailable = Math.max(...resources.flatMap(r => r.slotAvailables || []));

    if (!isFinite(maxAvailable) || maxAvailable <= 0) {
        maxAvailable = 1;
    }

    // 各Equipmentリソースの折れ線グラフを描画
    const colors = [
        '#f59e0b', // amber-500
        '#8b5cf6', // violet-500
        '#ec4899', // pink-500
        '#14b8a6', // teal-500
        '#f97316'  // orange-500
    ];

    resources.forEach((resource, index) => {
        const color = colors[index % colors.length];

        // 新形式: 配列から直接アクセス
        const { slotStartMinutes, slotEndMinutes, slotAvailables, slotFlags } = resource;

        // 配列が空の場合は早期リターン
        if (!slotStartMinutes || slotStartMinutes.length === 0) {
            return;
        }

        const dataPoints = [];

        // 配列を反復処理（サーバー側で既にソート済み）
        for (let i = 0; i < slotStartMinutes.length; i++) {
            // フラグチェック（将来の実装用、現時点ではslotFlagsはnull）
            // if (slotFlags && (slotFlags[i] & 2)) {
            //     continue; // 時間外スロットをスキップ
            // }

            // 分を時間に変換（例: 540 → 9.0, 570 → 9.5）
            const startHours = slotStartMinutes[i] / 60;
            const endHours = slotEndMinutes[i] / 60;
            const available = slotAvailables[i];

            // X座標を計算（既存のtimeToX関数を使用）
            const startX = timeToX(
                startHours, startHour, endHour,
                lunchStartHour, lunchEndHour,
                businessStartX, barAreaWidth
            );
            const endX = timeToX(
                endHours, startHour, endHour,
                lunchStartHour, lunchEndHour,
                businessStartX, barAreaWidth
            );

            // Y座標を計算
            const y = baselineY - (available / maxAvailable) * barAreaHeight;

            // スロットの開始点と終了点を追加（プラトー形状）
            dataPoints.push({ x: startX, y: y });
            dataPoints.push({ x: endX, y: y });
        }

        // 折れ線を描画
        if (dataPoints.length > 1) {
            for (let i = 0; i < dataPoints.length - 1; i++) {
                const p1 = dataPoints[i];
                const p2 = dataPoints[i + 1];

                drawLine(contentCtx, {
                    points: [p1.x, p1.y, p2.x, p2.y],
                    stroke: color,
                    strokeWidth: 2,
                    opacity: 0.8
                });
            }
        }

        // データポイントを描画
        dataPoints.forEach(point => {
            drawCircle(contentCtx, {
                x: point.x,
                y: point.y,
                radius: isYearView ? 1.5 : 2,
                fill: color,
                opacity: 0.9
            });
        });
    });

    return true;
}

