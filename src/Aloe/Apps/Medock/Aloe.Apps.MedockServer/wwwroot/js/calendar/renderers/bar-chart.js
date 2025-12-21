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
        const totalCount = slots.reduce((sum, s) => sum + (s.count || 0), 0);
        const totalCap = slots.reduce((sum, s) => sum + (s.cap || 0), 0);
        content += `<p class="text-base mb-2">予約: ${totalCount}/${totalCap}件</p>`;

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
            const cap = (s.cap !== undefined && s.cap !== null && s.cap > 0) ? s.cap : 1;
            const vacancyRatio = cap > 0 ? (1 - s.count / cap) * 100 : 0;
            // 空き率に基づいて色クラスを決定（空きが多い→青、空きが少ない→緑）
            const colorClass = vacancyRatio >= 70 ? 'bg-info' : vacancyRatio >= 30 ? 'bg-accent' : vacancyRatio > 0 ? 'bg-success' : '';

            // Show room-filtered count in each slot
            let roomInfo = '';
            if (s.filteredCount > 0) {
                roomInfo = ` <span class="text-warning">[部屋:${s.filteredCount}]</span>`;
            }

            // 時間範囲の表示（start-end形式）
            const timeRange = s.start && s.end ? `${s.start}-${s.end}` : (s.time || '');

            // 空き率が0%の場合は表示しない
            if (vacancyRatio > 0) {
                content += `
                    <div>
                        <div class="flex justify-between text-sm mb-1">
                            <span>${timeRange}</span>
                            <span>${s.count}/${cap} (空き:${Math.round(vacancyRatio)}%)${roomInfo}</span>
                        </div>
                        <progress class="progress ${colorClass} w-full" value="${vacancyRatio}" max="100"></progress>
                    </div>
                `;
            }
        });
        content += '</div>';
    }

    return content;
}

/**
 * 時間帯枠の空き率から色を取得
 * 空きが多い→青、空きが少ない（満杯に近い）→緑
 * @param {number} vacancyRatio - 空き率（0.0 ～ 1.0）
 * @returns {string} カラーコード
 */
export function getSlotColor(vacancyRatio) {
    if (vacancyRatio >= 0.7) {
        // 空き率70%以上 - 空きが多い → 青色
        return '#3b82f6'; // blue
    } else if (vacancyRatio >= 0.3) {
        // 空き率30-70% - 空きあり → シアン/ティール（青と緑の中間）
        return '#14b8a6'; // teal
    } else if (vacancyRatio > 0) {
        // 空き率0-30% - 空き少ない（満杯に近い）→ 緑色
        return '#10b981'; // green
    } else {
        // 空き率0% - 満杯 → 描画しない（この関数は呼ばれない想定）
        return '#10b981'; // green
    }
}

/**
 * 空き数から色を取得
 * 空きが多い→青、空きが少ない（満杯に近い）→緑
 * @param {number} available - 空き数（正の値）
 * @param {number} cap - キャパシティ
 * @returns {string} カラーコード
 */
function getSlotColorFromAvailable(available, cap) {
    if (cap <= 0) return '#9ca3af'; // キャパシティが0以下の場合はグレー

    const vacancyRatio = available / cap;
    if (vacancyRatio >= 0.7) {
        // 空き率70%以上 - 空きが多い → 青色
        return '#3b82f6'; // blue
    } else if (vacancyRatio >= 0.3) {
        // 空き率30-70% - 空きあり → シアン/ティール（青と緑の中間）
        return '#14b8a6'; // teal
    } else if (vacancyRatio > 0) {
        // 空き率0-30% - 空き少ない（満杯に近い）→ 緑色
        return '#10b981'; // green
    } else {
        // 空き率0% - 満杯 → 緑色
        return '#10b981'; // green
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
 * @param {string|object} timeData - 時刻データ（"HH:mm-HH:mm"形式の文字列、または{start, end}オブジェクト）
 * @param {number} startHour - 業務開始時刻（フォールバック用）
 * @param {number} endHour - 業務終了時刻（フォールバック用）
 * @returns {number} 時刻（時間単位）
 */
function parseTimeSlot(timeData, startHour, endHour) {
    // オブジェクト形式の場合（{start: "HH:mm", end: "HH:mm"}）
    if (typeof timeData === 'object' && timeData.start) {
        const startParts = timeData.start.split(':');
        const hour = parseInt(startParts[0], 10);
        const minute = startParts[1] ? parseInt(startParts[1], 10) : 0;
        return hour + minute / 60;
    }

    // 文字列形式の場合
    const timeStr = typeof timeData === 'string' ? timeData : '';
    // "HH:mm-HH:mm"形式の場合は開始時刻を取得
    const timePart = timeStr.split('-')[0].trim();

    // "HH:MM"形式の時刻を解析
    if (timePart.includes(':')) {
        const parts = timePart.split(':');
        const hour = parseInt(parts[0], 10);
        const minute = parts[1] ? parseInt(parts[1], 10) : 0;
        return hour + minute / 60;
    }

    // その他の場合は開始時刻を返す
    return startHour;
}

/**
 * スロットの開始・終了時刻を取得
 * @param {object} slot - スロットオブジェクト（{start, end}または{time}）
 * @param {number} startHour - 業務開始時刻（フォールバック用）
 * @param {number} endHour - 業務終了時刻（フォールバック用）
 * @returns {{start: number, end: number}} 開始時刻と終了時刻（時間単位）
 */
function parseSlotTimeRange(slot, startHour, endHour) {
    // オブジェクト形式でstart/endがある場合
    if (slot.start && slot.end) {
        const parseTime = (timeStr) => {
            const parts = timeStr.split(':');
            const hour = parseInt(parts[0], 10);
            const minute = parts[1] ? parseInt(parts[1], 10) : 0;
            return hour + minute / 60;
        };
        return {
            start: parseTime(slot.start),
            end: parseTime(slot.end)
        };
    }

    // timeプロパティがある場合（"HH:mm-HH:mm"形式）
    if (slot.time) {
        const timeStr = typeof slot.time === 'string' ? slot.time : '';
        const parts = timeStr.split('-');
        if (parts.length >= 2) {
            const parseTime = (timeStr) => {
                const timePart = timeStr.trim();
                if (timePart.includes(':')) {
                    const timeParts = timePart.split(':');
                    const hour = parseInt(timeParts[0], 10);
                    const minute = timeParts[1] ? parseInt(timeParts[1], 10) : 0;
                    return hour + minute / 60;
                }
                return startHour;
            };
            return {
                start: parseTime(parts[0]),
                end: parseTime(parts[1])
            };
        }
    }

    // フォールバック: 開始時刻のみ取得
    const start = parseTimeSlot(slot.time || slot, startHour, endHour);
    // 終了時刻は開始時刻+1時間と仮定（デフォルト）
    return {
        start: start,
        end: start + 1
    };
}

/**
 * スロットを集約（count/capを合算）
 * @param {Array} slots - 集約するスロットの配列
 * @returns {object|null} 集約されたスロットオブジェクト、またはnull
 */
function aggregateSlots(slots) {
    if (!slots || slots.length === 0) {
        return null;
    }

    let totalCount = 0;
    let totalCap = 0;
    let hasGrayedOut = false;

    slots.forEach(slot => {
        const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
        const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
        totalCount += count;
        totalCap += cap;
        if (slot.isGrayedOut) {
            hasGrayedOut = true;
        }
    });

    return {
        count: totalCount,
        cap: totalCap,
        available: totalCap - totalCount,
        isGrayedOut: hasGrayedOut,
        isAggregated: true
    };
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

    // 日付テキスト表示エリア（小さなセルでも計算しておく）
    const dayTextHeight = CONFIG.font.sizeSmall + 4;
    const barAreaTop = cellTop + dayTextHeight;
    // ラベル表示用のスペースを確保（年間ビューはより厳しい条件、月間ビューは標準）
    const isYearView = state.currentView === 'year';
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
        // データがない場合は描画しない
        const validSlots = slots.filter(slot => {
            const cap = (slot.cap !== undefined && slot.cap !== null && slot.cap > 0) ? slot.cap : 0;
            const count = (slot.count !== undefined && slot.count !== null) ? slot.count : 0;
            return cap > 0 || count > 0;
        });

        if (validSlots.length === 0) {
            // データがない場合は描画しない
            layers.interaction.add(hitArea);
            return;
        }

        // 業務時間設定を取得
        const startHour = state.options.startHour || 8;
        const endHour = state.options.endHour || 18;
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
            layers.interaction.add(hitArea);
            return;
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
                // 空き数が正の場合: 緑色の棒グラフを上方向に描画
                const barHeight = (available / maxValue) * barAreaHeight;
                const barY = baselineY - barHeight;
                const slotColor = isSlotGrayed ? '#9ca3af' : getSlotColorFromAvailable(available, cap);

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
