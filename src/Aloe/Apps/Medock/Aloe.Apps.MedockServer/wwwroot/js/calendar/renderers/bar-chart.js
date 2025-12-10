/**
 * Bar Chart Renderer
 *
 * 棒グラフ形式での日付セル描画
 * 年表示・月表示で使用される
 */

import { getState, setState } from '../state.js';
import { CONFIG } from '../config.js';
import { isToday, isDateInRange } from '../utils/date-utils.js';
import { showDayModal } from '../ui/modal.js';

/**
 * モーダル表示用のコンテンツHTMLを生成
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {Array|null} slots - 時間帯枠データ
 * @param {object} state - カレンダーの状態
 * @returns {string} HTML文字列
 */
function buildModalContent(dateStr, slots, state) {
    let content = '';

    if (slots && slots.length > 0) {
        const totalCount = slots.reduce((sum, s) => sum + s.count, 0);
        const totalMax = slots.reduce((sum, s) => sum + s.max, 0);
        content += `<p class="text-base mb-2">予約: ${totalCount}/${totalMax}件</p>`;

        // Show room filter summary
        const hasRoomFilter = slots.some(s => s.filteredCount > 0);
        if (hasRoomFilter) {
            const totalFiltered = slots.reduce((sum, s) => sum + (s.filteredCount || 0), 0);
            content += `<p class="text-warning mb-2">選択部屋: ${totalFiltered}件</p>`;
        }

        // 時間帯別の予約状況
        content += '<div class="space-y-3 mt-4">';
        slots.forEach(s => {
            // 空き率を計算（1 - 使用率）
            const vacancyRatio = s.max > 0 ? (1 - s.count / s.max) * 100 : 0;
            // 空き率に基づいて色クラスを決定（空きが多い→緑、空きが少ない→グレー）
            const colorClass = vacancyRatio >= 70 ? 'bg-success' : vacancyRatio >= 30 ? 'bg-info' : vacancyRatio > 0 ? 'bg-base-300' : '';

            // Show room-filtered count in each slot
            let roomInfo = '';
            if (s.filteredCount > 0) {
                roomInfo = ` <span class="text-warning">[部屋:${s.filteredCount}]</span>`;
            }

            // 空き率が0%の場合は表示しない
            if (vacancyRatio > 0) {
                content += `
                    <div>
                        <div class="flex justify-between text-sm mb-1">
                            <span>${s.time}</span>
                            <span>${s.count}/${s.max} (空き:${Math.round(vacancyRatio)}%)${roomInfo}</span>
                        </div>
                        <progress class="progress ${colorClass} w-full" value="${vacancyRatio}" max="100"></progress>
                    </div>
                `;
            }
        });
        content += '</div>';
    } else {
        const st = state.dayStats.get(dateStr) || { am: 0, pm: 0 };
        content += `<p class="text-base">午前: ${st.am}件</p>`;
        content += `<p class="text-base">午後: ${st.pm}件</p>`;
    }

    return content;
}

/**
 * 時間帯枠の空き率から色を取得
 * 空きが多い→目立つ色（緑）、空きが少ない→目立たない色（グレー）
 * @param {number} vacancyRatio - 空き率（0.0 ～ 1.0）
 * @returns {string} カラーコード
 */
export function getSlotColor(vacancyRatio) {
    if (vacancyRatio >= 0.7) {
        // 空き率70%以上 - 空きが多い → 目立つ緑色
        return '#10b981'; // green
    } else if (vacancyRatio >= 0.3) {
        // 空き率30-70% - 空きあり → 明るい緑
        return '#34d399'; // emerald
    } else if (vacancyRatio > 0) {
        // 空き率0-30% - 空き少ない → 目立たないグレー
        return '#9ca3af'; // gray
    } else {
        // 空き率0% - 満杯 → 描画しない（この関数は呼ばれない想定）
        return '#6b7280'; // dark gray
    }
}

/**
 * 時刻をX座標に変換（セル内の相対位置）
 * @param {number} timeInHours - 時刻（時間単位、例：8.5 = 8:30）
 * @param {number} startHour - 業務開始時刻
 * @param {number} endHour - 業務終了時刻
 * @param {number} cellLeft - セルの左端X座標
 * @param {number} barAreaWidth - 棒グラフエリアの幅
 * @returns {number} X座標
 */
function timeToX(timeInHours, startHour, endHour, cellLeft, barAreaWidth) {
    const totalHours = endHour - startHour;
    const relativePosition = Math.max(0, Math.min(1, (timeInHours - startHour) / totalHours));
    return cellLeft + 2 + relativePosition * barAreaWidth;
}

/**
 * スロットの時刻文字列を時刻（時間単位）に変換
 * @param {string} timeStr - 時刻文字列（"08:00"、"08:00-09:00"、"AM"、"PM"など）
 * @param {number} startHour - 業務開始時刻（AM/PMなどの緩いスロット用）
 * @param {number} endHour - 業務終了時刻（AM/PMなどの緩いスロット用）
 * @returns {number} 時刻（時間単位）
 */
function parseTimeSlot(timeStr, startHour, endHour) {
    // "08:00-09:00"形式の場合は開始時刻を取得
    const timePart = timeStr.split('-')[0].trim();

    // "HH:MM"形式の時刻を解析
    if (timePart.includes(':')) {
        const parts = timePart.split(':');
        const hour = parseInt(parts[0], 10);
        const minute = parts[1] ? parseInt(parts[1], 10) : 0;
        return hour + minute / 60;
    }

    // "AM"、"PM"などの緩いスロット
    const upper = timePart.toUpperCase();
    if (upper === 'AM') {
        return startHour + (12 - startHour) / 2; // 午前の中央時刻
    } else if (upper === 'PM') {
        return 12 + (endHour - 12) / 2; // 午後の中央時刻
    }

    // その他の場合は開始時刻を返す
    return startHour;
}

/**
 * 空き率から記号を決定
 * @param {number} vacancyRatio - 空き率（0.0 ～ 1.0）
 * @returns {string} 記号の種類: 'x', 'triangle', 'circle', 'double-circle'
 */
function getSymbolFromVacancyRatio(vacancyRatio) {
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
function renderSimpleViewSymbol(cellLeft, cellTop, cellWidth, cellHeight, dateStr, symbolType, isDateGrayed, vacancyRatio = 0) {
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
    const stats = state.dayStats.get(dateStr);
    const slots = stats?.slots || null;

    // グレーアウト判定: confirmedDateRange がある場合は範囲外をグレーアウト
    let isDateGrayed = false;
    if (state.confirmedDateRange) {
        isDateGrayed = !isDateInRange(dateStr, state.confirmedDateRange.start, state.confirmedDateRange.end);
    } else {
        isDateGrayed = stats?.isGrayedOut || false;
    }

    let bgRect = null; // 小さなセルの場合でもhitAreaで参照できるようにする

    // 日付テキスト表示エリア（小さなセルでも計算しておく）
    const dayTextHeight = CONFIG.font.sizeSmall + 4;
    const barAreaTop = cellTop + dayTextHeight;
    const barAreaHeight = Math.max(0, cellHeight - dayTextHeight - 4); // 下部余白4px、負の値を防止

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
            fontSize: CONFIG.font.sizeSmall,
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
            // 設備フィルターが有効かどうかを判定
            const showEquipmentGraph = state.options?.showEquipmentGraph ?? false;
            const hasEquipmentFilter = showEquipmentGraph && slots.some(s => s.filteredCount > 0);
            
            // 全スロットの平均空き率を計算（フィルタ条件を考慮）
            let totalVacancy = 0;
            let slotCount = 0;
            
            slots.forEach(slot => {
                // グレーアウトされたスロットは除外
                if (slot.isGrayedOut || isDateGrayed) {
                    return;
                }
                
                const max = (slot.max !== undefined && slot.max !== null && slot.max > 0) ? slot.max : 1;
                
                // 設備フィルターが有効な場合はfilteredCountを使用、そうでない場合はcountを使用
                let count;
                if (hasEquipmentFilter && slot.filteredCount !== undefined && slot.filteredCount !== null) {
                    count = slot.filteredCount;
                } else {
                    count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
                }
                
                const vacancyRatio = Math.max(0, Math.min(1, 1 - (count / max)));
                totalVacancy += vacancyRatio;
                slotCount++;
            });
            
            if (slotCount > 0) {
                overallVacancyRatio = totalVacancy / slotCount;
            }
        } else {
            // フォールバック: AM/PM データから空き率を計算
            const stats = state.dayStats.get(dateStr) || { am: 0, pm: 0, amMax: 10, pmMax: 10 };
            const amVacancyRatio = stats.amMax > 0 ? Math.max(0, Math.min(1, 1 - (stats.am / stats.amMax))) : 0;
            const pmVacancyRatio = stats.pmMax > 0 ? Math.max(0, Math.min(1, 1 - (stats.pm / stats.pmMax))) : 0;
            overallVacancyRatio = (amVacancyRatio + pmVacancyRatio) / 2;
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
        const totalHours = endHour - startHour;
        const barAreaWidth = cellWidth - 4; // 左右余白2px
        
        // スロット数に応じて均等に分割して隙間なく配置
        const slotCount = slots.length;
        const gapWidth = slotCount > 1 ? 1 : 0; // スロット間の隙間（1px、スロットが1つの場合は隙間なし）
        const totalGapWidth = gapWidth * (slotCount - 1); // 全隙間の合計
        const availableWidth = barAreaWidth - totalGapWidth; // 隙間を除いた利用可能な幅
        const barWidth = Math.max(1, Math.floor(availableWidth / slotCount)); // 各棒グラフの幅（均等分割）
        
        // 設備条件フィルター判定（showEquipmentGraphオプションも考慮）
        const showEquipmentGraph = state.options?.showEquipmentGraph ?? false;
        const hasEquipmentFilter = showEquipmentGraph && slots.some(s => s.filteredCount > 0);

        // 折れ線グラフ用のポイント配列（設備条件フィルターが選択されている場合）
        const linePoints = [];

        slots.forEach((slot, index) => {
            // データの値検証とデフォルト値設定（棒グラフ用）
            const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
            const max = (slot.max !== undefined && slot.max !== null && slot.max > 0) ? slot.max : 1;
            // 空き率を計算（1 - 使用率）
            const vacancyRatio = Math.max(0, Math.min(1, 1 - (count / max)));
            const isSlotGrayed = slot.isGrayedOut || isDateGrayed;
            const slotColor = isSlotGrayed ? '#9ca3af' : getSlotColor(vacancyRatio);
            // 空き率に基づいて棒の高さを計算
            const barHeight = Math.max(0, barAreaHeight * vacancyRatio);

            // 均等分割によるX座標計算（隙間を考慮）
            const barX = cellLeft + 2 + (index * (barWidth + gapWidth));
            const barY = barAreaTop + barAreaHeight - barHeight;

            // 通常の棒グラフ（高さが0より大きい場合のみ描画）
            if (barHeight > 0) {
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
            }

            // 折れ線グラフ用のポイントを計算（設備条件フィルターが選択されている場合）
            if (hasEquipmentFilter) {
                // filteredCountの値検証とデフォルト値設定（maxは棒グラフで既に定義済み）
                const filteredCount = (slot.filteredCount !== undefined && slot.filteredCount !== null) ? slot.filteredCount : 0;

                // 空き率を計算（1 - 使用率）* 100 で0-100%の割合を計算（100%を超える場合は100%にクランプ）
                const filteredVacancyRatio = Math.min(100, Math.max(0, (1 - filteredCount / max) * 100));

                // デバッグログ（開発時のみ、本番では削除または条件付きで出力）
                if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
                    console.debug(`[折れ線グラフ] ${slot.time}: count=${count}, filteredCount=${filteredCount}, max=${max}, vacancyRatio=${(vacancyRatio * 100).toFixed(1)}%, filteredVacancyRatio=${filteredVacancyRatio.toFixed(1)}%`);
                }

                // Y座標: 0%が下、100%が上（barAreaTop + barAreaHeight - (空き率 / 100 * barAreaHeight)）
                // セルの範囲内に収まるように、barAreaTopからbarAreaTop + barAreaHeightの範囲にクランプ
                const lineY = Math.max(barAreaTop, Math.min(barAreaTop + barAreaHeight, barAreaTop + barAreaHeight - (filteredVacancyRatio / 100 * barAreaHeight)));
                // X座標は均等分割に合わせて棒グラフの中央位置を使用
                const lineX = barX + barWidth / 2;
                linePoints.push(lineX, lineY);
            }
        });

        // 折れ線グラフを描画（設備条件フィルターが選択されている場合、かつポイントが2つ以上ある場合）
        if (hasEquipmentFilter && linePoints.length >= 4) {
            const lineGraph = new Konva.Line({
                points: linePoints,
                stroke: '#8b5cf6', // 紫系の色
                strokeWidth: 2,
                opacity: 0.8,
                lineCap: 'round',
                lineJoin: 'round'
            });
            layers.content.add(lineGraph);
        }
    } else if (!isTooSmall) {
        // フォールバック: AM/PM 2本の棒
        const stats = state.dayStats.get(dateStr) || { am: 0, pm: 0, amMax: 10, pmMax: 10 };
        // 空き率を計算（1 - 使用率）
        const amVacancyRatio = stats.amMax > 0 ? Math.max(0, Math.min(1, 1 - (stats.am / stats.amMax))) : 0;
        const pmVacancyRatio = stats.pmMax > 0 ? Math.max(0, Math.min(1, 1 - (stats.pm / stats.pmMax))) : 0;

        const barAreaWidth = Math.max(0, cellWidth - 4);
        const gapWidth = 1;
        const barWidth = Math.max(1, (barAreaWidth - gapWidth) / 2);

        // AM棒（空き率が0より大きい場合のみ描画）
        const amBarHeight = Math.max(0, barAreaHeight * amVacancyRatio);
        if (amBarHeight > 0 && barWidth > 0) {
            const amBar = new Konva.Rect({
                x: cellLeft + 2,
                y: barAreaTop + barAreaHeight - amBarHeight,
                width: barWidth,
                height: amBarHeight,
                fill: getSlotColor(amVacancyRatio),
                cornerRadius: Math.min(1, amBarHeight / 2, barWidth / 2)
            });
            layers.content.add(amBar);
        }

        // PM棒（空き率が0より大きい場合のみ描画）
        const pmBarHeight = Math.max(0, barAreaHeight * pmVacancyRatio);
        if (pmBarHeight > 0 && barWidth > 0) {
            const pmBar = new Konva.Rect({
                x: cellLeft + 2 + barWidth + gapWidth,
                y: barAreaTop + barAreaHeight - pmBarHeight,
                width: barWidth,
                height: pmBarHeight,
                fill: getSlotColor(pmVacancyRatio),
                cornerRadius: Math.min(1, pmBarHeight / 2, barWidth / 2)
            });
            layers.content.add(pmBar);
        }
    }

    // インタラクションエリア
    const hitArea = new Konva.Rect({
        x: cellLeft,
        y: cellTop,
        width: cellWidth,
        height: cellHeight,
        fill: 'transparent'
    });

    hitArea.on('mouseenter', function (e) {
        // ホバー時は枠線ハイライトのみ（ツールチップは廃止）
        if (bgRect) {
            bgRect.stroke(CONFIG.colors.today);
            bgRect.strokeWidth(2);
            layers.content.batchDraw();
        }
    });

    hitArea.on('mouseleave', function () {
        if (!state.isDragging && bgRect) {
            // ホバー解除後は枠線を削除（今日の日付のみ枠線を残す）
            if (isToday(dateStr)) {
                bgRect.stroke(CONFIG.colors.today);
                bgRect.strokeWidth(2);
            } else {
                // 選択状態に関わらず、ホバー解除後は枠線を削除
                bgRect.stroke(null);
                bgRect.strokeWidth(0);
            }
            layers.content.batchDraw();
        }
    });

    // クリックハンドラ（ダブルクリック・Shift+クリック対応）
    hitArea.on('click', function (e) {
        console.log('MedockCalendar: hitArea click event fired', { dateStr, cellWidth, cellHeight });
        const now = Date.now();
        const isDoubleClick = (now - state.lastClickTime < 300) && (state.lastClickDate === dateStr);
        const isShiftClick = e.evt.shiftKey;
        const isYearView = state.currentView === 'year';

        // 年間カレンダーの場合は日付選択と範囲選択をスキップし、ダブルクリックのみ処理
        if (isYearView) {
            if (isDoubleClick) {
                console.log('MedockCalendar: Double click detected', { dateStr, hasDotNetRef: !!state.dotNetRef });
                setState({ lastClickTime: 0, lastClickDate: null });
                // モーダルダイアログを表示
                const modalContent = buildModalContent(dateStr, slots, state);
                showDayModal(dateStr, modalContent, state.dotNetRef);
            } else {
                // ダブルクリック判定用に時刻と日付を記録
                setState({
                    lastClickTime: now,
                    lastClickDate: dateStr
                });
            }
            return;
        }

        // 月間カレンダー・週間カレンダーの既存処理
        if (isDoubleClick) {
            console.log('MedockCalendar: Double click detected', { dateStr, hasDotNetRef: !!state.dotNetRef });
            setState({ lastClickTime: 0, lastClickDate: null });
            // モーダルダイアログを表示
            const modalContent = buildModalContent(dateStr, slots, state);
            showDayModal(dateStr, modalContent, state.dotNetRef);
        } else if (isShiftClick && state.selectedDate) {
            const range = { start: state.selectedDate, end: dateStr };
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateRangeSelected', range.start, range.end);
            }
            // Shift+クリックで範囲選択（両方の日付が異なる場合のみ confirmedDateRange を設定）
            setState({
                selectedDate: null,  // 範囲選択時は単一選択をクリア
                lastClickTime: 0,    // ダブルクリック判定をリセット
                lastClickDate: null,
                selectedDateRange: range,
                confirmedDateRange: range.start !== range.end ? range : null
            });
        } else if (state.confirmedDateRange &&
            isDateInRange(dateStr, state.confirmedDateRange.start, state.confirmedDateRange.end)) {
            // 範囲選択内の日付をクリックした場合は範囲選択を解除し、その日付を選択
            setState({
                lastClickTime: now,      // ダブルクリック判定用に時刻を記録
                lastClickDate: dateStr,
                selectedDate: dateStr,   // クリックした日付を選択状態にする
                selectedDateRange: null,
                confirmedDateRange: null
            });
            if (state.dotNetRef) {
                state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
            }
            // カレンダーを再描画してグレーアウトを解除
            if (window.MedockCalendar) {
                window.MedockCalendar.render();
            }
        } else {
            // 同じ日付をクリックした場合は選択解除（だがダブルクリック判定用に時刻は記録）
            if (state.selectedDate === dateStr) {
                setState({
                    lastClickTime: now,  // ダブルクリック判定用に時刻を記録
                    lastClickDate: dateStr,
                    selectedDate: null,
                    selectedDateRange: null,
                    confirmedDateRange: null
                });
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', null);
                }
            } else {
                setState({
                    lastClickTime: now,
                    lastClickDate: dateStr,
                    selectedDate: dateStr,
                    selectedDateRange: null,
                    confirmedDateRange: null
                });
                if (state.dotNetRef) {
                    state.dotNetRef.invokeMethodAsync('OnDateSelectedSingle', dateStr);
                }
            }
        }
    });

    // ドラッグによる範囲選択
    hitArea.on('mousedown', function (e) {
        // 年間カレンダーの場合はドラッグ処理をスキップ
        if (state.currentView === 'year') {
            return;
        }
        // mousedown では isDragging と dragStartDate のみ設定
        // selectedDateRange は mousemove で実際にドラッグが開始された時に設定
        setState({
            isDragging: true,
            dragStartDate: dateStr
        });
    });

    hitArea.on('mouseup', function () {
        // mouseup は処理しない（calendar-main.js の stage mouseup で処理される）
        // ここでは何もしない
    });

    hitArea.on('mousemove', function () {
        // 年間カレンダーの場合はドラッグ処理をスキップ
        if (state.currentView === 'year') {
            return;
        }
        if (state.isDragging && state.dragStartDate) {
            // 実際にドラッグが開始された場合のみ selectedDateRange を設定
            setState({ selectedDateRange: { start: state.dragStartDate, end: dateStr } });
        }
    });

    layers.interaction.add(hitArea);
}
