/**
 * Canvas Render State
 * 
 * 描画データ構造と空間インデックスの管理
 * Hit Test（クリック検出）に使用
 */

/**
 * Render State Class
 * 描画されたすべての要素の位置・サイズ・メタデータを保持
 */
export class RenderState {
    constructor() {
        this.reset();
    }

    /**
     * 状態をリセット
     */
    reset() {
        // 空間インデックス: 日付文字列 -> セル情報
        this.cells = new Map();

        // バーインデックス: バーID -> バー情報
        this.bars = new Map();

        // 月間/年間ビュー用: 月情報の配列
        this.months = [];

        // 週間ビュー用: スロット情報の配列
        this.weekSlots = [];

        // 月間ビュー用: 週行情報の配列（月↔週のトランジション用）
        this.weekRows = [];

        // 現在のビュータイプ
        this.viewType = null;

        // バーIDカウンター
        this._barIdCounter = 0;
    }

    /**
     * セル情報を追加
     * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
     * @param {object} cellInfo - セル情報 { x, y, width, height, month?, day? }
     */
    addCell(dateStr, cellInfo) {
        this.cells.set(dateStr, {
            ...cellInfo,
            dateStr,
            bars: []
        });
    }

    /**
     * バー情報を追加
     * @param {string} dateStr - 日付文字列 (YYYY-MM-DD)
     * @param {object} barInfo - バー情報 { x, y, width, height, slot, color, ... }
     * @returns {string} バーID
     */
    addBar(dateStr, barInfo) {
        const barId = `bar-${this._barIdCounter++}`;
        const bar = {
            id: barId,
            dateStr,
            ...barInfo
        };

        this.bars.set(barId, bar);

        // セルにバーを追加
        const cell = this.cells.get(dateStr);
        if (cell) {
            cell.bars.push(bar);
        }

        return barId;
    }

    /**
     * 月情報を追加（年間ビュー用）
     * @param {object} monthInfo - 月情報 { index, bounds: { x, y, width, height }, days: [] }
     */
    addMonth(monthInfo) {
        this.months.push(monthInfo);
    }

    /**
     * 週行情報を追加（月間ビュー用、月↔週トランジション用）
     * @param {object} weekRowInfo - 週行情報 { rowIndex, weekStartDate, weekEndDate, bounds: { x, y, width, height } }
     */
    addWeekRow(weekRowInfo) {
        this.weekRows.push(weekRowInfo);
    }

    /**
     * 週間スロット情報を追加
     * @param {object} slotInfo - スロット情報 { bounds: { x, y, width, height }, appointment, ... }
     */
    addWeekSlot(slotInfo) {
        this.weekSlots.push(slotInfo);
    }

    /**
     * ビュータイプを設定
     * @param {string} viewType - 'year', 'month', 'week'
     */
    setViewType(viewType) {
        this.viewType = viewType;
    }

    /**
     * 指定座標のセルを検索
     * @param {number} x - X座標
     * @param {number} y - Y座標
     * @returns {object|null} セル情報
     */
    hitTestCell(x, y) {
        // すべてのセルを検索（小規模なので線形探索で十分）
        for (const cell of this.cells.values()) {
            if (x >= cell.x && x <= cell.x + cell.width &&
                y >= cell.y && y <= cell.y + cell.height) {
                return cell;
            }
        }
        return null;
    }

    /**
     * 指定座標のバーを検索
     * @param {number} x - X座標
     * @param {number} y - Y座標
     * @returns {object|null} バー情報
     */
    hitTestBar(x, y) {
        // まずセルを特定（高速化）
        const cell = this.hitTestCell(x, y);
        if (!cell) {
            return null;
        }

        // そのセル内のバーを検索（後ろから = 前面から）
        for (let i = cell.bars.length - 1; i >= 0; i--) {
            const bar = cell.bars[i];
            if (x >= bar.x && x <= bar.x + bar.width &&
                y >= bar.y && y <= bar.y + bar.height) {
                return bar;
            }
        }

        return null;
    }

    /**
     * 指定座標の月を検索（年間ビュー用）
     * @param {number} x - X座標
     * @param {number} y - Y座標
     * @returns {object|null} 月情報
     */
    hitTestMonth(x, y) {
        for (const month of this.months) {
            const bounds = month.bounds;
            if (x >= bounds.x && x <= bounds.x + bounds.width &&
                y >= bounds.y && y <= bounds.y + bounds.height) {
                return month;
            }
        }
        return null;
    }

    /**
     * 指定座標の週間スロットを検索
     * @param {number} x - X座標
     * @param {number} y - Y座標
     * @returns {object|null} スロット情報
     */
    hitTestWeekSlot(x, y) {
        // 後ろから検索（前面の要素から）
        for (let i = this.weekSlots.length - 1; i >= 0; i--) {
            const slot = this.weekSlots[i];
            const bounds = slot.bounds;
            if (x >= bounds.x && x <= bounds.x + bounds.width &&
                y >= bounds.y && y <= bounds.y + bounds.height) {
                return slot;
            }
        }
        return null;
    }

    /**
     * 階層的Hit Test（年間ビュー用）
     * 月 -> 日 -> バーの順に検索
     * @param {number} x - X座標
     * @param {number} y - Y座標
     * @returns {{ type: string, data: object }|null} Hit結果
     */
    hierarchicalHitTest(x, y) {
        if (this.viewType === 'year') {
            // 1. 月を特定
            const month = this.hitTestMonth(x, y);
            if (!month) {
                return null;
            }

            // 2. その月のセルを検索
            const cell = this.hitTestCell(x, y);
            if (!cell) {
                return { type: 'month', data: month };
            }

            // 3. その日のバーを検索
            const bar = this.hitTestBar(x, y);
            if (bar) {
                return { type: 'bar', data: bar };
            }

            return { type: 'cell', data: cell };
        } else if (this.viewType === 'month') {
            // 月間ビュー: セル -> バー
            const cell = this.hitTestCell(x, y);
            if (!cell) {
                return null;
            }

            const bar = this.hitTestBar(x, y);
            if (bar) {
                return { type: 'bar', data: bar };
            }

            return { type: 'cell', data: cell };
        } else if (this.viewType === 'week') {
            // 週間ビュー: スロットのみ
            const slot = this.hitTestWeekSlot(x, y);
            if (slot) {
                return { type: 'slot', data: slot };
            }

            return null;
        }

        return null;
    }

    /**
     * セル情報を取得
     * @param {string} dateStr - 日付文字列
     * @returns {object|null} セル情報
     */
    getCell(dateStr) {
        return this.cells.get(dateStr) || null;
    }

    /**
     * バー情報を取得
     * @param {string} barId - バーID
     * @returns {object|null} バー情報
     */
    getBar(barId) {
        return this.bars.get(barId) || null;
    }

    /**
     * デバッグ情報を出力
     */
    debugInfo() {
        console.log('RenderState Debug Info:', {
            viewType: this.viewType,
            cellsCount: this.cells.size,
            barsCount: this.bars.size,
            monthsCount: this.months.length,
            weekRowsCount: this.weekRows.length,
            weekSlotsCount: this.weekSlots.length
        });
    }
}

/**
 * グローバルRender Stateインスタンス
 */
let globalRenderState = null;

/**
 * グローバルRender Stateを取得
 * @returns {RenderState} RenderStateインスタンス
 */
export function getRenderState() {
    if (!globalRenderState) {
        globalRenderState = new RenderState();
    }
    return globalRenderState;
}

/**
 * グローバルRender Stateをリセット
 */
export function resetRenderState() {
    if (globalRenderState) {
        globalRenderState.reset();
    }
}

/**
 * グローバルRender Stateを作成（初期化）
 * @returns {RenderState} 新しいRenderStateインスタンス
 */
export function createRenderState() {
    globalRenderState = new RenderState();
    return globalRenderState;
}


