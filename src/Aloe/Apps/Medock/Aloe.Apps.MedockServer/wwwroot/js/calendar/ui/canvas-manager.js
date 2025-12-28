/**
 * Canvas Manager
 * 
 * Canvas要素の作成・管理・リサイズ対応を行うモジュール
 * Konva.jsの代わりに、ネイティブCanvas APIを使用
 */

/**
 * Canvas Layer Names
 */
export const LAYER_NAMES = {
    BACKGROUND: 'background',
    GRID: 'grid',
    CONTENT: 'content',
    INTERACTION: 'interaction'
};

/**
 * Canvas Manager Class
 * 複数のCanvasレイヤーを管理
 */
export class CanvasManager {
    /**
     * Constructor
     * @param {string} containerId - コンテナ要素のID
     * @param {number} width - Canvas幅
     * @param {number} height - Canvas高さ
     */
    constructor(containerId, width, height) {
        this.containerId = containerId;
        this.container = document.getElementById(containerId);
        
        if (!this.container) {
            throw new Error(`Container not found: ${containerId}`);
        }

        this.width = width;
        this.height = height;
        this.canvases = new Map();
        this.contexts = new Map();
        this.resizeObserver = null;

        this._createCanvasLayers();
        this._setupResize();
    }

    /**
     * Canvasレイヤーを作成
     * @private
     */
    _createCanvasLayers() {
        // コンテナのスタイルを設定（相対位置指定で重ね合わせを可能にする）
        this.container.style.position = 'relative';
        this.container.style.width = '100%';
        this.container.style.height = '100%';

        // 各レイヤーを作成（下から順に）
        const layers = [
            LAYER_NAMES.BACKGROUND,
            LAYER_NAMES.GRID,
            LAYER_NAMES.CONTENT,
            LAYER_NAMES.INTERACTION
        ];

        layers.forEach((layerName, index) => {
            const canvas = document.createElement('canvas');
            canvas.id = `${this.containerId}-${layerName}`;
            canvas.width = this.width;
            canvas.height = this.height;
            
            // Canvas要素のスタイル設定
            canvas.style.position = 'absolute';
            canvas.style.top = '0';
            canvas.style.left = '0';
            canvas.style.zIndex = String(index);
            
            // インタラクションレイヤーは透明に設定
            if (layerName === LAYER_NAMES.INTERACTION) {
                canvas.style.pointerEvents = 'auto';
            }

            this.container.appendChild(canvas);
            
            const context = canvas.getContext('2d');
            this.canvases.set(layerName, canvas);
            this.contexts.set(layerName, context);
        });
    }

    /**
     * リサイズ対応を設定
     * @private
     */
    _setupResize() {
        this.resizeObserver = new ResizeObserver(entries => {
            for (let entry of entries) {
                const newWidth = entry.contentRect.width;
                const newHeight = entry.contentRect.height || 600;

                if (this.width !== newWidth || this.height !== newHeight) {
                    this.resize(newWidth, newHeight);
                }
            }
        });

        this.resizeObserver.observe(this.container);
    }

    /**
     * Canvas要素を取得
     * @param {string} layerName - レイヤー名
     * @returns {HTMLCanvasElement} Canvas要素
     */
    getCanvas(layerName) {
        return this.canvases.get(layerName);
    }

    /**
     * Canvas描画コンテキストを取得
     * @param {string} layerName - レイヤー名
     * @returns {CanvasRenderingContext2D} 描画コンテキスト
     */
    getContext(layerName) {
        return this.contexts.get(layerName);
    }

    /**
     * すべてのCanvasコンテキストを取得
     * @returns {Map<string, CanvasRenderingContext2D>} レイヤー名 -> コンテキストのマップ
     */
    getAllContexts() {
        return this.contexts;
    }

    /**
     * Canvasサイズを変更
     * @param {number} width - 新しい幅
     * @param {number} height - 新しい高さ
     */
    resize(width, height) {
        this.width = width;
        this.height = height;

        this.canvases.forEach((canvas) => {
            canvas.width = width;
            canvas.height = height;
        });

        // リサイズ後は再描画が必要
        // （呼び出し側でrenderを呼ぶ必要がある）
    }

    /**
     * 指定レイヤーをクリア
     * @param {string} layerName - レイヤー名
     */
    clearLayer(layerName) {
        const ctx = this.contexts.get(layerName);
        if (ctx) {
            ctx.clearRect(0, 0, this.width, this.height);
        }
    }

    /**
     * すべてのレイヤーをクリア
     */
    clearAll() {
        this.contexts.forEach((ctx) => {
            ctx.clearRect(0, 0, this.width, this.height);
        });
    }

    /**
     * Canvas Managerを破棄
     */
    destroy() {
        // ResizeObserverを切断
        if (this.resizeObserver) {
            this.resizeObserver.disconnect();
            this.resizeObserver = null;
        }

        // Canvas要素を削除
        this.canvases.forEach((canvas) => {
            if (canvas.parentNode) {
                canvas.parentNode.removeChild(canvas);
            }
        });

        this.canvases.clear();
        this.contexts.clear();
    }

    /**
     * Canvas要素の座標を取得（マウスイベント用）
     * @param {MouseEvent} event - マウスイベント
     * @returns {{ x: number, y: number }} Canvas座標
     */
    getCanvasCoordinates(event) {
        const canvas = this.getCanvas(LAYER_NAMES.INTERACTION);
        const rect = canvas.getBoundingClientRect();
        
        return {
            x: event.clientX - rect.left,
            y: event.clientY - rect.top
        };
    }
}

/**
 * Canvas Managerを作成
 * @param {string} containerId - コンテナ要素のID
 * @param {number} width - Canvas幅
 * @param {number} height - Canvas高さ
 * @returns {CanvasManager} CanvasManagerインスタンス
 */
export function createCanvasManager(containerId, width, height) {
    return new CanvasManager(containerId, width, height);
}


