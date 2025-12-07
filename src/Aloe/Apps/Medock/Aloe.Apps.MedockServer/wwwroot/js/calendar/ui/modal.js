/**
 * Modal Management
 *
 * ダブルクリック時に表示されるモーダルダイアログのDOM要素管理
 * 年表示・月表示でのダブルクリック時に使用
 */

let currentDateStr = null;
let currentDotNetRef = null;

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
            <div class="modal-box">
                <button class="btn btn-sm btn-circle btn-ghost absolute right-2 top-2" id="medock-modal-close">✕</button>
                <h3 class="font-bold text-lg" id="medock-modal-title"></h3>
                <div class="py-4" id="medock-modal-content"></div>
                <div class="modal-action">
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
            currentDateStr = null;
            currentDotNetRef = null;
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
 * @param {string} content - 表示するHTML文字列
 * @param {object} dotNetRef - .NET object reference for callbacks
 */
export function showDayModal(dateStr, content, dotNetRef) {
    console.log('MedockCalendar: showDayModal called', { dateStr, hasContent: !!content, hasDotNetRef: !!dotNetRef });
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

    // タイトルと内容を設定
    const titleEl = modal.querySelector('#medock-modal-title');
    const contentEl = modal.querySelector('#medock-modal-content');

    if (titleEl) {
        titleEl.textContent = dateStr;
    }
    if (contentEl) {
        contentEl.innerHTML = content;
    }

    // モーダルを表示
    console.log('MedockCalendar: Showing modal', { modalExists: !!modal, modalOpen: modal.open });
    try {
        modal.showModal();
        console.log('MedockCalendar: Modal shown successfully', { modalOpen: modal.open });
    } catch (error) {
        console.error('MedockCalendar: Failed to show modal', error);
    }
}

/**
 * モーダルを非表示にする
 */
export function hideModal() {
    const modal = document.getElementById('medock-day-modal');
    if (!modal) return;
    modal.close();
    currentDateStr = null;
    currentDotNetRef = null;
}
