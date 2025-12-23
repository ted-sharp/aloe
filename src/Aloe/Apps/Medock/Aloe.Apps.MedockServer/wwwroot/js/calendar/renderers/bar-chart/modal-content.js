/**
 * Modal Content Builder
 *
 * モーダル表示用のコンテンツHTML生成
 */

/**
 * モーダル表示用のコンテンツHTMLを生成
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {Array|null} slots - 時間帯枠データ
 * @param {object} state - カレンダーの状態
 * @returns {string} HTML文字列
 */
export function buildModalContent(dateStr, slots, state) {
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

