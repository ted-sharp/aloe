/**
 * Modal Management
 *
 * ダブルクリック時に表示されるモーダルダイアログのDOM要素管理
 * 年表示・月表示でのダブルクリック時に使用
 */

import { buildModalContent } from '../renderers/bar-chart/modal-content.js';
import { getState, setState } from '../state.js';
import { renderDayDetailBarChart } from '../renderers/bar-chart/index.js';

let currentDateStr = null;
let currentDotNetRef = null;
let modalStage = null; // モーダル用のKonva Stageインスタンス
let modalResizeObserver = null; // リサイズ監視用のResizeObserver
let modalCurrentSlots = null; // 現在表示中のスロットデータ（リサイズ時の再描画用）
let modalResizeHandler = null; // リサイズイベントハンドラ（クリーンアップ用）

/**
 * モーダルのDOM要素を作成してbodyに追加
 */
export function createModal() {
    let modal = document.getElementById('medock-day-modal');
    if (!modal) {
        modal = document.createElement('dialog');
        modal.id = 'medock-day-modal';
        modal.className = 'modal';
        modal.innerHTML = `
            <div class="modal-box" style="max-width: 90vw; max-height: 90vh; width: 90vw; height: 90vh; display: flex; flex-direction: column;">
                <button class="btn btn-sm btn-circle btn-ghost absolute right-2 top-2" id="medock-modal-close">✕</button>
                <h3 class="font-bold text-lg mb-2" id="medock-modal-title"></h3>
                <div class="flex-1 overflow-hidden" id="medock-modal-content" style="min-height: 0;">
                    <div id="medock-modal-chart-container" style="width: 100%; height: 100%;"></div>
                </div>
                <div class="modal-action mt-4">
                    <button class="btn btn-primary" id="medock-modal-week-btn">週表示へ</button>
                    <button class="btn" id="medock-modal-close-btn">閉じる</button>
                </div>
            </div>
            <form method="dialog" class="modal-backdrop">
                <button>close</button>
            </form>
        `;
        document.body.appendChild(modal);

        // ×ボタンのイベント
        modal.querySelector('#medock-modal-close').addEventListener('click', () => {
            hideModal();
        });

        // 閉じるボタンのイベント
        modal.querySelector('#medock-modal-close-btn').addEventListener('click', () => {
            hideModal();
        });

        // 週表示へボタンのイベント
        modal.querySelector('#medock-modal-week-btn').addEventListener('click', async () => {
            if (currentDotNetRef && currentDateStr) {
                try {
                    await currentDotNetRef.invokeMethodAsync('OnDateDoubleClicked', currentDateStr);
                } catch (error) {
                    console.error('MedockCalendar: Failed to invoke OnDateDoubleClicked', error);
                }
            } else {
                console.warn('MedockCalendar: dotNetRef or dateStr is not set');
            }
            hideModal();
        });

        // Escキーでも閉じる（dialogの標準動作）
        modal.addEventListener('close', () => {
            // ResizeObserverを切断
            if (modalResizeObserver) {
                modalResizeObserver.disconnect();
                modalResizeObserver = null;
            }
            // ウィンドウのリサイズイベントリスナーを削除
            if (modalResizeHandler) {
                window.removeEventListener('resize', modalResizeHandler);
                modalResizeHandler = null;
            }
            // モーダル用のStageを破棄
            if (modalStage) {
                modalStage.destroy();
                modalStage = null;
            }
            currentDateStr = null;
            currentDotNetRef = null;
            modalCurrentSlots = null;
        });

        // 背景クリックで閉じる
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                hideModal();
            }
        });
    }
}

/**
 * モーダルを表示
 * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
 * @param {Array|null} slots - 時間帯枠データ
 * @param {object} dotNetRef - .NET object reference for callbacks
 */
export function showDayModal(dateStr, slots, dotNetRef) {
    console.log('MedockCalendar: showDayModal called', { dateStr, hasSlots: !!slots, hasDotNetRef: !!dotNetRef });
    let modal = document.getElementById('medock-day-modal');
    // モーダルが存在しない場合は作成
    if (!modal) {
        console.log('MedockCalendar: Modal not found, creating...');
        createModal();
        modal = document.getElementById('medock-day-modal');
        if (!modal) {
            console.error('MedockCalendar: Failed to create modal');
            return;
        }
    }

    currentDateStr = dateStr;
    currentDotNetRef = dotNetRef;

    // タイトルを設定
    const titleEl = modal.querySelector('#medock-modal-title');
    if (titleEl) {
        titleEl.textContent = dateStr;
    }

    // 既存のStageがあれば破棄
    if (modalStage) {
        modalStage.destroy();
        modalStage = null;
    }

    // モーダルを表示（先に表示してからサイズを取得するため）
    console.log('MedockCalendar: Showing modal', { modalExists: !!modal, modalOpen: modal.open });
    try {
        modal.showModal();
        console.log('MedockCalendar: Modal shown successfully', { modalOpen: modal.open });
    } catch (error) {
        console.error('MedockCalendar: Failed to show modal', error);
        return;
    }

    // スロットデータを保存（リサイズ時の再描画用）
    modalCurrentSlots = slots;

    // モーダルが表示された後に、状態を取得してStageを作成
    // requestAnimationFrameを使用して、レイアウトが完了した後にサイズを取得
    requestAnimationFrame(() => {
        // 状態を取得
        const state = getState();

        // Konva.jsのStageを作成して棒グラフを描画
        modalStage = buildModalContent(dateStr, slots, state);
        if (!modalStage) {
            console.error('MedockCalendar: Failed to create modal stage');
            return;
        }

        // リサイズ監視を開始
        setupResizeObserver(dateStr, slots, state);
    });
}

/**
 * リサイズ監視を設定
 * @param {string} dateStr - 日付文字列
 * @param {Array|null} slots - スロットデータ
 * @param {object} state - カレンダーの状態
 */
function setupResizeObserver(dateStr, slots, state) {
    const container = document.getElementById('medock-modal-chart-container');
    if (!container) {
        return;
    }

    // 既存のResizeObserverを切断
    if (modalResizeObserver) {
        modalResizeObserver.disconnect();
        modalResizeObserver = null;
    }

    // 既存のリサイズイベントリスナーを削除
    if (modalResizeHandler) {
        window.removeEventListener('resize', modalResizeHandler);
        modalResizeHandler = null;
    }

    // リサイズ時の処理を定義
    let resizeTimeout = null;
    modalResizeHandler = () => {
        // デバウンス処理（リサイズが完了するまで待機）
        if (resizeTimeout) {
            clearTimeout(resizeTimeout);
        }
        resizeTimeout = setTimeout(() => {
            if (!modalStage || !currentDateStr) {
                return;
            }

            // コンテナのサイズを取得
            const rect = container.getBoundingClientRect();
            const newWidth = Math.max(800, rect.width);
            const newHeight = Math.max(500, rect.height);

            // Stageのサイズが変更されている場合のみ更新
            if (modalStage.width() !== newWidth || modalStage.height() !== newHeight) {
                console.log('MedockCalendar: Resizing modal stage', { 
                    oldWidth: modalStage.width(), 
                    oldHeight: modalStage.height(),
                    newWidth, 
                    newHeight 
                });

                // Stageのサイズを更新
                modalStage.width(newWidth);
                modalStage.height(newHeight);

                // 再描画（最新のstateを取得）
                const currentState = getState();
                updateModalContent(dateStr, modalCurrentSlots || slots, currentState, newWidth, newHeight);
            }
        }, 150); // 150msのデバウンス
    };

    // ResizeObserverを作成
    modalResizeObserver = new ResizeObserver(modalResizeHandler);
    modalResizeObserver.observe(container);

    // ウィンドウのリサイズイベントも監視（モーダルボックスのサイズ変更に対応）
    window.addEventListener('resize', modalResizeHandler);
}

/**
 * モーダルコンテンツを更新（リサイズ時）
 * @param {string} dateStr - 日付文字列
 * @param {Array|null} slots - スロットデータ
 * @param {object} state - カレンダーの状態
 * @param {number} width - 新しい幅
 * @param {number} height - 新しい高さ
 */
function updateModalContent(dateStr, slots, state, width, height) {
    if (!modalStage) {
        return;
    }

    const modalLayers = modalStage.children;
    if (!modalLayers || modalLayers.length === 0) {
        return;
    }

    // 元のstate.layersを保存
    const originalLayers = state.layers;
    const originalStage = state.stage;

    try {
        // 一時的にstate.layersをモーダル用のlayersに置き換え
        const modalLayersObj = {
            background: modalLayers[0],
            grid: modalLayers[1],
            content: modalLayers[2],
            interaction: modalLayers[3]
        };

        setState({
            layers: modalLayersObj,
            stage: modalStage
        });

        // 既存のレイヤーをクリア
        modalLayersObj.background.destroyChildren();
        modalLayersObj.grid.destroyChildren();
        modalLayersObj.content.destroyChildren();
        modalLayersObj.interaction.destroyChildren();

        // 日付から日数を取得
        const date = new Date(dateStr);
        const dayNumber = date.getDate();
        const isHoliday = state.holidays.has(dateStr);

        // renderDayDetailBarChartを呼び出して棒グラフを再描画
        renderDayDetailBarChart(0, 0, width, height, dateStr, dayNumber, isHoliday);

        // レイヤーを描画
        modalLayersObj.background.batchDraw();
        modalLayersObj.grid.batchDraw();
        modalLayersObj.content.batchDraw();
        modalLayersObj.interaction.batchDraw();
    } catch (error) {
        console.error('MedockCalendar: Failed to update modal content on resize', error);
    } finally {
        // 元のstate.layersを復元
        setState({
            layers: originalLayers,
            stage: originalStage
        });
    }
}

/**
 * モーダルを非表示にする
 */
export function hideModal() {
    const modal = document.getElementById('medock-day-modal');
    if (!modal) return;

    // ResizeObserverを切断
    if (modalResizeObserver) {
        modalResizeObserver.disconnect();
        modalResizeObserver = null;
    }

    // ウィンドウのリサイズイベントリスナーを削除
    if (modalResizeHandler) {
        window.removeEventListener('resize', modalResizeHandler);
        modalResizeHandler = null;
    }

    // モーダル用のStageを破棄
    if (modalStage) {
        modalStage.destroy();
        modalStage = null;
    }

    modal.close();
    currentDateStr = null;
    currentDotNetRef = null;
    modalCurrentSlots = null;
}
