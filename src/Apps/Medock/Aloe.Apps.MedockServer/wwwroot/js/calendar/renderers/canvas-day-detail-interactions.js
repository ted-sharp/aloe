/**
 * Canvas Day Detail Interactions
 *
 * 日詳細ポップアップ用のインタラクションハンドラー
 * 独立したCanvas ManagerとRenderStateを使用
 */

/**
 * 日詳細ポップアップのインタラクションハンドラーをセットアップ
 * @param {object} canvasManager - ポップアップ専用のCanvasManagerインスタンス
 * @param {object} renderState - ポップアップ専用のRenderStateインスタンス
 * @param {object} dotNetRef - DayDetailPopupコンポーネントの.NET参照
 * @param {string} dateStr - 対象日付（YYYY-MM-DD）
 */
export function setupDayDetailInteractions(canvasManager, renderState, dotNetRef, dateStr) {
    if (!canvasManager || !renderState || !dotNetRef) {
        console.warn('setupDayDetailInteractions: 必要なパラメータが不足しています');
        return;
    }

    const interactionCanvas = canvasManager.getCanvas('interaction');
    if (!interactionCanvas) {
        console.warn('setupDayDetailInteractions: interaction canvasが見つかりません');
        return;
    }

    let hoveredBar = null;
    let selectedBar = null;
    let lastClickTime = 0;
    let lastClickBarId = null;

    /**
     * マウス座標からバーをHit Test
     */
    function hitTestBar(x, y) {
        // 日詳細ダイアログではセルが登録されていないため、bars を直接検索
        for (const bar of renderState.bars.values()) {
            if (x >= bar.x && x <= bar.x + bar.width &&
                y >= bar.y && y <= bar.y + bar.height) {
                return bar;
            }
        }
        return null;
    }

    /**
     * ハイライト・選択状態を描画
     */
    function drawHighlightAndSelection(hoveredBar, selectedBar) {
        const interactionCtx = canvasManager.getContext('interaction');
        canvasManager.clearLayer('interaction');

        // 選択状態を描画（実線）
        if (selectedBar && !selectedBar.isGrayed) {
            interactionCtx.strokeStyle = '#3b82f6';
            interactionCtx.lineWidth = 3;
            interactionCtx.setLineDash([]); // 実線
            interactionCtx.strokeRect(selectedBar.x - 2, selectedBar.y - 2, selectedBar.width + 4, selectedBar.height + 4);
        }

        // ホバー状態を描画（点線、選択状態でなければ）
        if (hoveredBar && !hoveredBar.isGrayed && (!selectedBar || selectedBar.id !== hoveredBar.id)) {
            interactionCtx.strokeStyle = '#3b82f6';
            interactionCtx.lineWidth = 2;
            interactionCtx.setLineDash([4, 4]); // 点線パターン: 4px線、4px空白
            interactionCtx.strokeRect(hoveredBar.x - 1, hoveredBar.y - 1, hoveredBar.width + 2, hoveredBar.height + 2);
            interactionCtx.setLineDash([]); // 点線をリセット
        }
    }

    /**
     * マウス移動ハンドラ
     */
    interactionCanvas.addEventListener('mousemove', (e) => {
        const coords = canvasManager.getCanvasCoordinates(e);
        const bar = hitTestBar(coords.x, coords.y);

        if (bar) {
            // バーが変わった場合のみ更新
            if (!hoveredBar || hoveredBar.id !== bar.id) {
                hoveredBar = bar;
                drawHighlightAndSelection(hoveredBar, selectedBar);
                interactionCanvas.style.cursor = bar.isGrayed ? 'not-allowed' : 'pointer';
            }
        } else {
            if (hoveredBar) {
                hoveredBar = null;
                drawHighlightAndSelection(null, selectedBar);
                interactionCanvas.style.cursor = 'default';
            }
        }
    });

    /**
     * マウスリーブハンドラ
     */
    interactionCanvas.addEventListener('mouseleave', () => {
        if (hoveredBar) {
            hoveredBar = null;
            drawHighlightAndSelection(null, selectedBar);
            interactionCanvas.style.cursor = 'default';
        }
    });

    /**
     * クリックハンドラ（ダブルクリック検出）
     */
    interactionCanvas.addEventListener('click', (e) => {
        const coords = canvasManager.getCanvasCoordinates(e);
        const bar = hitTestBar(coords.x, coords.y);

        if (bar) {
            // グレーアウトされたバーのクリックは無視
            if (bar.isGrayed) {
                console.log('Day detail bar clicked: グレーアウトされたスロット、無視');
                return;
            }

            // ダブルクリック検出（300ms以内に同じバーを2回クリック）
            const now = Date.now();
            const isDoubleClick = (now - lastClickTime < 300) && (lastClickBarId === bar.id);
            lastClickTime = now;
            lastClickBarId = bar.id;

            if (isDoubleClick) {
                // ダブルクリック：予約モーダルを開く
                console.log('Day detail bar double-clicked:', {
                    dateStr,
                    slotIndex: bar.slotIndex,
                    startMinutes: bar.startMinutes,
                    endMinutes: bar.endMinutes,
                    available: bar.available,
                    capacity: bar.cap
                });

                // Blazor側に通知
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnSlotClicked',
                        dateStr,
                        bar.startMinutes,
                        bar.endMinutes,
                        bar.cap,
                        bar.count
                    );
                }

                // ダブルクリック後は選択を解除
                selectedBar = null;
                lastClickTime = 0;
                lastClickBarId = null;
            } else {
                // シングルクリック：選択状態に
                console.log('Day detail bar selected:', {
                    dateStr,
                    slotIndex: bar.slotIndex,
                    startMinutes: bar.startMinutes,
                    endMinutes: bar.endMinutes,
                    available: bar.available,
                    capacity: bar.cap
                });

                selectedBar = bar;
                drawHighlightAndSelection(hoveredBar, selectedBar);

                // Blazor側に選択を通知
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnSlotSelected',
                        dateStr,
                        bar.startMinutes,
                        bar.endMinutes,
                        bar.cap,
                        bar.count
                    );
                }
            }
        } else {
            // バーの外をクリック：選択を解除
            if (selectedBar) {
                selectedBar = null;
                lastClickTime = 0;
                lastClickBarId = null;
                drawHighlightAndSelection(hoveredBar, selectedBar);

                // Blazor側に選択解除を通知
                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnSlotDeselected');
                }
            }
        }
    });
}
