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
        this.offscreenCanvases = new Map();
        this.offscreenContexts = new Map();
        this.snapshotCanvases = new Map(); // クロスフェード用スナップショット
        this.resizeObserver = null;

        this._createCanvasLayers();
        this._createOffscreenBuffers();
        this._createSnapshotBuffers();
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
     * オフスクリーンバッファを作成（ダブルバッファリング用）
     * @private
     */
    _createOffscreenBuffers() {
        const layers = [
            LAYER_NAMES.BACKGROUND,
            LAYER_NAMES.GRID,
            LAYER_NAMES.CONTENT,
            LAYER_NAMES.INTERACTION
        ];

        layers.forEach((layerName) => {
            const offscreenCanvas = document.createElement('canvas');
            offscreenCanvas.width = this.width;
            offscreenCanvas.height = this.height;

            const offscreenContext = offscreenCanvas.getContext('2d');
            this.offscreenCanvases.set(layerName, offscreenCanvas);
            this.offscreenContexts.set(layerName, offscreenContext);
        });
    }

    /**
     * スナップショットバッファを作成（クロスフェード用）
     * @private
     */
    _createSnapshotBuffers() {
        const layers = [
            LAYER_NAMES.BACKGROUND,
            LAYER_NAMES.GRID,
            LAYER_NAMES.CONTENT,
            LAYER_NAMES.INTERACTION
        ];

        layers.forEach((layerName) => {
            const snapshotCanvas = document.createElement('canvas');
            snapshotCanvas.width = this.width;
            snapshotCanvas.height = this.height;
            this.snapshotCanvases.set(layerName, snapshotCanvas);
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
     * オフスクリーン描画コンテキストを取得
     * @param {string} layerName - レイヤー名
     * @returns {CanvasRenderingContext2D} オフスクリーン描画コンテキスト
     */
    getOffscreenContext(layerName) {
        return this.offscreenContexts.get(layerName);
    }

    /**
     * すべてのオフスクリーンCanvasコンテキストを取得
     * @returns {Map<string, CanvasRenderingContext2D>} レイヤー名 -> オフスクリーンコンテキストのマップ
     */
    getAllOffscreenContexts() {
        return this.offscreenContexts;
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

        this.offscreenCanvases.forEach((canvas) => {
            canvas.width = width;
            canvas.height = height;
        });

        this.snapshotCanvases.forEach((canvas) => {
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
     * 指定オフスクリーンレイヤーをクリア
     * @param {string} layerName - レイヤー名
     */
    clearOffscreenLayer(layerName) {
        const ctx = this.offscreenContexts.get(layerName);
        if (ctx) {
            ctx.clearRect(0, 0, this.width, this.height);
        }
    }

    /**
     * すべてのオフスクリーンレイヤーをクリア
     */
    clearAllOffscreen() {
        this.offscreenContexts.forEach((ctx) => {
            ctx.clearRect(0, 0, this.width, this.height);
        });
    }

    /**
     * オフスクリーンレイヤーをメインCanvasに転送
     * @param {string} layerName - レイヤー名
     */
    commitLayer(layerName) {
        const offscreenCanvas = this.offscreenCanvases.get(layerName);
        const ctx = this.contexts.get(layerName);
        if (offscreenCanvas && ctx) {
            ctx.clearRect(0, 0, this.width, this.height);
            ctx.drawImage(offscreenCanvas, 0, 0);
        }
    }

    /**
     * すべてのオフスクリーンレイヤーをメインCanvasに転送（一括コミット）
     * @param {string} fadeMode - フェードモード: 'instant', 'crossfade', 'fadethrough', 'slidefade', 'scalefade', 'sharedelement'（デフォルト: 'instant'）
     * @param {number} fadeDuration - フェード時間（ミリ秒、デフォルト: 150）
     * @param {object} options - 追加オプション { sourceBounds, targetBounds }
     */
    commitAll(fadeMode = 'instant', fadeDuration = 150, options = {}) {
        // 下位互換性のため、古いAPI（boolean）もサポート
        if (typeof fadeMode === 'boolean') {
            fadeMode = fadeMode ? 'crossfade' : 'instant';
        }

        if (fadeMode === 'instant') {
            // 通常の一括転送（瞬時に切り替え）
            this.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
                const ctx = this.contexts.get(layerName);
                if (ctx) {
                    ctx.clearRect(0, 0, this.width, this.height);
                    ctx.drawImage(offscreenCanvas, 0, 0);
                }
            });

        } else if (fadeMode === 'crossfade') {
            // クロスフェードトランジション（古い画面と新しい画面をブレンド）

            // 1. 現在のメインCanvasの内容をスナップショット（古い画面）
            this.canvases.forEach((canvas, layerName) => {
                const snapshotCanvas = this.snapshotCanvases.get(layerName);
                if (snapshotCanvas) {
                    const snapshotCtx = snapshotCanvas.getContext('2d');
                    snapshotCtx.clearRect(0, 0, this.width, this.height);
                    snapshotCtx.drawImage(canvas, 0, 0);
                }
            });

            // 2. アニメーションループでクロスフェード
            const startTime = performance.now();
            const animate = (currentTime) => {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / fadeDuration, 1);

                this.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
                    const ctx = this.contexts.get(layerName);
                    const snapshotCanvas = this.snapshotCanvases.get(layerName);

                    if (ctx && snapshotCanvas) {
                        ctx.clearRect(0, 0, this.width, this.height);

                        // 古い画面をフェードアウト（アルファ: 1 → 0）
                        ctx.globalAlpha = 1 - progress;
                        ctx.drawImage(snapshotCanvas, 0, 0);

                        // 新しい画面をフェードイン（アルファ: 0 → 1）
                        ctx.globalAlpha = progress;
                        ctx.drawImage(offscreenCanvas, 0, 0);

                        ctx.globalAlpha = 1.0; // 元に戻す
                    }
                });

                if (progress < 1) {
                    requestAnimationFrame(animate);
                }
            };
            requestAnimationFrame(animate);

        } else if (fadeMode === 'fadethrough') {
            // フェードスルートランジション（フェードアウト → フェードイン）

            // 1. 現在のメインCanvasの内容をスナップショット（古い画面）
            this.canvases.forEach((canvas, layerName) => {
                const snapshotCanvas = this.snapshotCanvases.get(layerName);
                if (snapshotCanvas) {
                    const snapshotCtx = snapshotCanvas.getContext('2d');
                    snapshotCtx.clearRect(0, 0, this.width, this.height);
                    snapshotCtx.drawImage(canvas, 0, 0);
                }
            });

            // 2. アニメーションループでフェードスルー
            const halfDuration = fadeDuration / 2;
            const startTime = performance.now();

            const animate = (currentTime) => {
                const elapsed = currentTime - startTime;
                const totalProgress = Math.min(elapsed / fadeDuration, 1);

                this.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
                    const ctx = this.contexts.get(layerName);
                    const snapshotCanvas = this.snapshotCanvases.get(layerName);

                    if (ctx && snapshotCanvas) {
                        ctx.clearRect(0, 0, this.width, this.height);

                        if (elapsed < halfDuration) {
                            // 前半: 古い画面をフェードアウト（アルファ: 1 → 0）
                            const fadeOutProgress = elapsed / halfDuration;
                            ctx.globalAlpha = 1 - fadeOutProgress;
                            ctx.drawImage(snapshotCanvas, 0, 0);
                            ctx.globalAlpha = 1.0;
                        } else {
                            // 後半: 新しい画面をフェードイン（アルファ: 0 → 1）
                            const fadeInProgress = (elapsed - halfDuration) / halfDuration;
                            ctx.globalAlpha = fadeInProgress;
                            ctx.drawImage(offscreenCanvas, 0, 0);
                            ctx.globalAlpha = 1.0;
                        }
                    }
                });

                if (totalProgress < 1) {
                    requestAnimationFrame(animate);
                }
            };
            requestAnimationFrame(animate);

        } else if (fadeMode === 'slidefade') {
            // スライドフェードトランジション（弱ディゾルブ＋微小スライド）

            // 1. 現在のメインCanvasの内容をスナップショット（古い画面）
            this.canvases.forEach((canvas, layerName) => {
                const snapshotCanvas = this.snapshotCanvases.get(layerName);
                if (snapshotCanvas) {
                    const snapshotCtx = snapshotCanvas.getContext('2d');
                    snapshotCtx.clearRect(0, 0, this.width, this.height);
                    snapshotCtx.drawImage(canvas, 0, 0);
                }
            });

            // 2. アニメーションループでスライドフェード
            const slideDistance = 30; // スライド距離（ピクセル）
            const startTime = performance.now();

            const animate = (currentTime) => {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / fadeDuration, 1);

                // イージング関数（ease-out）
                const easeOut = 1 - Math.pow(1 - progress, 3);

                this.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
                    const ctx = this.contexts.get(layerName);
                    const snapshotCanvas = this.snapshotCanvases.get(layerName);

                    if (ctx && snapshotCanvas) {
                        ctx.clearRect(0, 0, this.width, this.height);

                        // 古い画面: フェードアウト＋微小左スライド（アルファ: 1 → 0、X: 0 → -15px）
                        ctx.save();
                        ctx.globalAlpha = 1 - progress; // 完全に消える
                        ctx.translate(-slideDistance * 0.5 * easeOut, 0);
                        ctx.drawImage(snapshotCanvas, 0, 0);
                        ctx.restore();

                        // 新しい画面: フェードイン＋右からスライド（アルファ: 0 → 1、X: 30px → 0）
                        ctx.save();
                        ctx.globalAlpha = progress;
                        ctx.translate(slideDistance * (1 - easeOut), 0);
                        ctx.drawImage(offscreenCanvas, 0, 0);
                        ctx.restore();
                    }
                });

                if (progress < 1) {
                    requestAnimationFrame(animate);
                }
            };
            requestAnimationFrame(animate);

        } else if (fadeMode === 'scalefade') {
            // スケールフェードトランジション（弱ディゾルブ＋微小スケール）

            // 1. 現在のメインCanvasの内容をスナップショット（古い画面）
            this.canvases.forEach((canvas, layerName) => {
                const snapshotCanvas = this.snapshotCanvases.get(layerName);
                if (snapshotCanvas) {
                    const snapshotCtx = snapshotCanvas.getContext('2d');
                    snapshotCtx.clearRect(0, 0, this.width, this.height);
                    snapshotCtx.drawImage(canvas, 0, 0);
                }
            });

            // 2. アニメーションループでスケールフェード
            const startTime = performance.now();

            const animate = (currentTime) => {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / fadeDuration, 1);

                // イージング関数（ease-out）
                const easeOut = 1 - Math.pow(1 - progress, 3);

                this.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
                    const ctx = this.contexts.get(layerName);
                    const snapshotCanvas = this.snapshotCanvases.get(layerName);

                    if (ctx && snapshotCanvas) {
                        ctx.clearRect(0, 0, this.width, this.height);

                        // 古い画面: フェードアウト＋微小縮小（アルファ: 1 → 0、スケール: 1.0 → 0.95）
                        const oldScale = 1.0 - easeOut * 0.05;
                        const oldOffsetX = (this.width - this.width * oldScale) / 2;
                        const oldOffsetY = (this.height - this.height * oldScale) / 2;

                        ctx.save();
                        ctx.globalAlpha = 1 - progress; // 完全に消える
                        ctx.translate(oldOffsetX, oldOffsetY);
                        ctx.scale(oldScale, oldScale);
                        ctx.drawImage(snapshotCanvas, 0, 0);
                        ctx.restore();

                        // 新しい画面: フェードイン＋拡大（アルファ: 0 → 1、スケール: 0.95 → 1.0）
                        const newScale = 0.95 + easeOut * 0.05;
                        const newOffsetX = (this.width - this.width * newScale) / 2;
                        const newOffsetY = (this.height - this.height * newScale) / 2;

                        ctx.save();
                        ctx.globalAlpha = progress;
                        ctx.translate(newOffsetX, newOffsetY);
                        ctx.scale(newScale, newScale);
                        ctx.drawImage(offscreenCanvas, 0, 0);
                        ctx.restore();
                    }
                });

                if (progress < 1) {
                    requestAnimationFrame(animate);
                }
            };
            requestAnimationFrame(animate);

        } else if (fadeMode === 'sharedelement') {
            // 共有要素トランジション（位置・サイズを主に補間）

            const sourceBounds = options.sourceBounds;
            const targetBounds = options.targetBounds;

            if (!sourceBounds || !targetBounds) {
                // bounds情報がない場合は通常のスケールフェードにフォールバック
                this.commitAll('scalefade', fadeDuration);
                return;
            }

            // 1. すべてのレイヤーを合成したスナップショットを作成（古い画面）
            // 各レイヤーごとにスナップショットを取るのではなく、すべてのレイヤーを1つに合成
            const compositeSnapshot = document.createElement('canvas');
            compositeSnapshot.width = this.width;
            compositeSnapshot.height = this.height;
            const compositeCtx = compositeSnapshot.getContext('2d');

            // すべてのメインCanvasレイヤーを順番に合成
            this.canvases.forEach((canvas, layerName) => {
                compositeCtx.drawImage(canvas, 0, 0);
            });

            console.log('Composite snapshot created:', {
                size: `${compositeSnapshot.width}x${compositeSnapshot.height}`
            });

            // 2. アニメーションループ
            const startTime = performance.now();

            const animate = (currentTime) => {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / fadeDuration, 1);

                // ease-in-out-cubic イージングで開始と終了を滑らかに
                const easeInOutCubic = progress < 0.5
                    ? 4 * progress * progress * progress
                    : 1 - Math.pow(-2 * progress + 2, 3) / 2;

                this.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
                    const ctx = this.contexts.get(layerName);
                    // 合成スナップショットを使用（各レイヤーの個別スナップショットは使わない）
                    const snapshotCanvas = compositeSnapshot;

                    if (ctx && snapshotCanvas) {
                        ctx.clearRect(0, 0, this.width, this.height);

                        // 遷移方向を判定
                        const transitionType = options.transitionType || '';
                        const isExpandingTransition = transitionType.endsWith('-to-year') || transitionType.endsWith('-to-week');

                        // 月→年/週の場合: 年/週ビューをすっぱり表示、その上に月ビューを縮小
                        if (isExpandingTransition) {
                            // 新しい画面（年/週ビュー）を最初から完全表示
                            ctx.drawImage(offscreenCanvas, 0, 0);

                            // progress が 0.95 未満の場合のみ月ビューを描画（最後は完全に消す）
                            if (progress < 0.95) {
                                // 共有要素の位置・サイズを補間
                                const currentX = sourceBounds.x + (targetBounds.x - sourceBounds.x) * easeInOutCubic;
                                const currentY = sourceBounds.y + (targetBounds.y - sourceBounds.y) * easeInOutCubic;
                                const currentWidth = sourceBounds.width + (targetBounds.width - sourceBounds.width) * easeInOutCubic;
                                const currentHeight = sourceBounds.height + (targetBounds.height - sourceBounds.height) * easeInOutCubic;

                                // 古い画面（月ビュー）を不透明のまま縮小（オーバーレイ）
                                // 縮小比率を計算
                                const scaleX = currentWidth / sourceBounds.width;
                                const scaleY = currentHeight / sourceBounds.height;
                                const offsetX = currentX - sourceBounds.x * scaleX;
                                const offsetY = currentY - sourceBounds.y * scaleY;

                                ctx.save();

                                // クリップマスク：縮小する領域だけ描画
                                ctx.beginPath();
                                ctx.rect(currentX, currentY, currentWidth, currentHeight);
                                ctx.clip();

                                // 変換を適用して月ビューを縮小描画
                                ctx.translate(offsetX, offsetY);
                                ctx.scale(scaleX, scaleY);

                                // まず背景を白で塗りつぶす（変換後の座標系、元のキャンバスサイズ）
                                ctx.fillStyle = '#ffffff';
                                ctx.fillRect(0, 0, sourceBounds.width, sourceBounds.height);

                                // 月ビューを描画
                                ctx.drawImage(snapshotCanvas, 0, 0);
                                ctx.restore();
                            }

                        } else {
                            // 年/週→月の場合: 従来の動作を維持
                            // 共有要素の位置・サイズを補間
                            const currentX = sourceBounds.x + (targetBounds.x - sourceBounds.x) * easeInOutCubic;
                            const currentY = sourceBounds.y + (targetBounds.y - sourceBounds.y) * easeInOutCubic;
                            const currentWidth = sourceBounds.width + (targetBounds.width - sourceBounds.width) * easeInOutCubic;
                            const currentHeight = sourceBounds.height + (targetBounds.height - sourceBounds.height) * easeInOutCubic;

                            // 古い画面のフェードアウト
                            const fadeOutSpeed = 1.2;
                            ctx.save();
                            ctx.globalAlpha = Math.max(0, 1 - progress * fadeOutSpeed);
                            ctx.drawImage(snapshotCanvas, 0, 0);
                            ctx.restore();

                            // 共有要素: 位置・サイズのみ補間
                            ctx.save();

                            // クリップマスク：共有要素の領域だけ描画
                            ctx.beginPath();
                            ctx.rect(currentX, currentY, currentWidth, currentHeight);
                            ctx.clip();

                            // 新しい画面の全体を描画（クリップ内のみ表示される）
                            const scaleX = currentWidth / targetBounds.width;
                            const scaleY = currentHeight / targetBounds.height;
                            const offsetX = currentX - targetBounds.x * scaleX;
                            const offsetY = currentY - targetBounds.y * scaleY;

                            ctx.translate(offsetX, offsetY);
                            ctx.scale(scaleX, scaleY);
                            ctx.drawImage(offscreenCanvas, 0, 0);
                            ctx.restore();

                            // 周辺コンテンツ: progress > 0.7 からフェードイン
                            if (progress > 0.7) {
                                const delayedProgress = (progress - 0.7) / 0.3;
                                const contentAlpha = delayedProgress * 0.6;
                                ctx.save();
                                ctx.globalAlpha = contentAlpha;
                                ctx.drawImage(offscreenCanvas, 0, 0);
                                ctx.restore();
                            }
                        }
                    }
                });

                if (progress < 1) {
                    requestAnimationFrame(animate);
                } else {
                    // アニメーション完了後の処理
                    const transitionType = options.transitionType || '';
                    const isExpandingTransition = transitionType.endsWith('-to-year') || transitionType.endsWith('-to-week');

                    // 月→年/週の場合、年/週ビューはすでに完全表示されているので何もしない
                    // 年/週→月の場合は、月ビューを即座に完全表示
                    if (!isExpandingTransition) {
                        this.offscreenCanvases.forEach((offscreenCanvas, layerName) => {
                            const ctx = this.contexts.get(layerName);
                            if (ctx) {
                                ctx.clearRect(0, 0, this.width, this.height);
                                ctx.drawImage(offscreenCanvas, 0, 0);
                            }
                        });
                    }
                }
            };
            requestAnimationFrame(animate);
        }
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
        this.offscreenCanvases.clear();
        this.offscreenContexts.clear();
        this.snapshotCanvases.clear();
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


