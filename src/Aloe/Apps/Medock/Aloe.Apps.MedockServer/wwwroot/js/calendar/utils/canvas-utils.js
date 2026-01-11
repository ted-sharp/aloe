/**
 * Canvas Drawing Utilities
 * 
 * Canvas描画のヘルパー関数
 * 矩形・線・テキスト・パス描画の共通処理
 */

import { CONFIG } from '../config.js';

/**
 * 矩形を描画
 * @param {CanvasRenderingContext2D} ctx - 描画コンテキスト
 * @param {object} params - パラメータ
 * @param {number} params.x - X座標
 * @param {number} params.y - Y座標
 * @param {number} params.width - 幅
 * @param {number} params.height - 高さ
 * @param {string} params.fill - 塗りつぶし色
 * @param {string} [params.stroke] - 枠線色
 * @param {number} [params.strokeWidth] - 枠線幅
 * @param {number[]} [params.strokeDashArray] - 破線パターン (例: [4, 4])
 * @param {number} [params.cornerRadius] - 角丸半径
 * @param {number} [params.opacity] - 不透明度 (0-1)
 */
export function drawRect(ctx, params) {
    const {
        x, y, width, height, fill,
        stroke = null,
        strokeWidth = 1,
        strokeDashArray = null,
        cornerRadius = 0,
        opacity = 1
    } = params;

    ctx.save();
    ctx.globalAlpha = opacity;

    // 破線パターンを設定
    if (strokeDashArray) {
        ctx.setLineDash(strokeDashArray);
    }

    if (cornerRadius > 0) {
        // 角丸矩形
        ctx.beginPath();
        ctx.moveTo(x + cornerRadius, y);
        ctx.lineTo(x + width - cornerRadius, y);
        ctx.arcTo(x + width, y, x + width, y + cornerRadius, cornerRadius);
        ctx.lineTo(x + width, y + height - cornerRadius);
        ctx.arcTo(x + width, y + height, x + width - cornerRadius, y + height, cornerRadius);
        ctx.lineTo(x + cornerRadius, y + height);
        ctx.arcTo(x, y + height, x, y + height - cornerRadius, cornerRadius);
        ctx.lineTo(x, y + cornerRadius);
        ctx.arcTo(x, y, x + cornerRadius, y, cornerRadius);
        ctx.closePath();

        if (fill) {
            ctx.fillStyle = fill;
            ctx.fill();
        }

        if (stroke) {
            ctx.strokeStyle = stroke;
            ctx.lineWidth = strokeWidth;
            ctx.stroke();
        }
    } else {
        // 通常の矩形
        if (fill) {
            ctx.fillStyle = fill;
            ctx.fillRect(x, y, width, height);
        }

        if (stroke) {
            ctx.strokeStyle = stroke;
            ctx.lineWidth = strokeWidth;
            ctx.strokeRect(x, y, width, height);
        }
    }

    ctx.restore();
}

/**
 * 線を描画
 * @param {CanvasRenderingContext2D} ctx - 描画コンテキスト
 * @param {object} params - パラメータ
 * @param {number[]} params.points - 座標配列 [x1, y1, x2, y2, ...]
 * @param {string} params.stroke - 線色
 * @param {number} [params.strokeWidth] - 線幅
 * @param {number} [params.opacity] - 不透明度 (0-1)
 */
export function drawLine(ctx, params) {
    const {
        points,
        stroke,
        strokeWidth = 1,
        opacity = 1
    } = params;

    if (points.length < 4) {
        return; // 最低2点必要
    }

    ctx.save();
    ctx.globalAlpha = opacity;
    ctx.strokeStyle = stroke;
    ctx.lineWidth = strokeWidth;

    ctx.beginPath();
    ctx.moveTo(points[0], points[1]);
    
    for (let i = 2; i < points.length; i += 2) {
        ctx.lineTo(points[i], points[i + 1]);
    }

    ctx.stroke();
    ctx.restore();
}

/**
 * テキストを描画
 * @param {CanvasRenderingContext2D} ctx - 描画コンテキスト
 * @param {object} params - パラメータ
 * @param {string} params.text - テキスト
 * @param {number} params.x - X座標
 * @param {number} params.y - Y座標
 * @param {number} [params.width] - 幅（中央揃え用）
 * @param {string} params.fill - 文字色
 * @param {number} [params.fontSize] - フォントサイズ
 * @param {string} [params.fontFamily] - フォントファミリー
 * @param {string} [params.fontStyle] - フォントスタイル ('normal', 'bold', 'italic')
 * @param {string} [params.align] - テキスト配置 ('left', 'center', 'right')
 * @param {string} [params.baseline] - ベースライン ('top', 'middle', 'bottom', 'alphabetic')
 * @param {number} [params.opacity] - 不透明度 (0-1)
 */
export function drawText(ctx, params) {
    const {
        text,
        x,
        y,
        width = null,
        fill,
        fontSize = CONFIG.font.sizeMedium,
        fontFamily = CONFIG.font.family,
        fontStyle = 'normal',
        align = 'left',
        baseline = 'top',
        opacity = 1
    } = params;

    ctx.save();
    ctx.globalAlpha = opacity;
    ctx.fillStyle = fill;
    
    // フォント設定
    const fontWeight = fontStyle === 'bold' ? 'bold' : 'normal';
    const fontStyleValue = fontStyle === 'italic' ? 'italic' : 'normal';
    ctx.font = `${fontStyleValue} ${fontWeight} ${fontSize}px ${fontFamily}`;
    
    // テキスト配置
    let textX = x;
    if (width !== null) {
        if (align === 'center') {
            textX = x + width / 2;
            ctx.textAlign = 'center';
        } else if (align === 'right') {
            textX = x + width;
            ctx.textAlign = 'right';
        } else {
            ctx.textAlign = 'left';
        }
    } else {
        ctx.textAlign = align;
    }
    
    ctx.textBaseline = baseline;

    ctx.fillText(text, textX, y);
    ctx.restore();
}

/**
 * 円を描画
 * @param {CanvasRenderingContext2D} ctx - 描画コンテキスト
 * @param {object} params - パラメータ
 * @param {number} params.x - 中心X座標
 * @param {number} params.y - 中心Y座標
 * @param {number} params.radius - 半径
 * @param {string} [params.fill] - 塗りつぶし色
 * @param {string} [params.stroke] - 枠線色
 * @param {number} [params.strokeWidth] - 枠線幅
 * @param {number} [params.opacity] - 不透明度 (0-1)
 */
export function drawCircle(ctx, params) {
    const {
        x,
        y,
        radius,
        fill = null,
        stroke = null,
        strokeWidth = 1,
        opacity = 1
    } = params;

    ctx.save();
    ctx.globalAlpha = opacity;

    ctx.beginPath();
    ctx.arc(x, y, radius, 0, 2 * Math.PI);

    if (fill) {
        ctx.fillStyle = fill;
        ctx.fill();
    }

    if (stroke) {
        ctx.strokeStyle = stroke;
        ctx.lineWidth = strokeWidth;
        ctx.stroke();
    }

    ctx.restore();
}

/**
 * 正多角形を描画
 * @param {CanvasRenderingContext2D} ctx - 描画コンテキスト
 * @param {object} params - パラメータ
 * @param {number} params.x - 中心X座標
 * @param {number} params.y - 中心Y座標
 * @param {number} params.sides - 辺の数
 * @param {number} params.radius - 半径
 * @param {number} [params.rotation] - 回転角度（度）
 * @param {string} [params.fill] - 塗りつぶし色
 * @param {string} [params.stroke] - 枠線色
 * @param {number} [params.strokeWidth] - 枠線幅
 * @param {number} [params.opacity] - 不透明度 (0-1)
 */
export function drawPolygon(ctx, params) {
    const {
        x,
        y,
        sides,
        radius,
        rotation = 0,
        fill = null,
        stroke = null,
        strokeWidth = 1,
        opacity = 1
    } = params;

    ctx.save();
    ctx.globalAlpha = opacity;

    ctx.beginPath();
    const angleStep = (2 * Math.PI) / sides;
    const rotationRad = (rotation * Math.PI) / 180;

    for (let i = 0; i < sides; i++) {
        const angle = i * angleStep + rotationRad;
        const px = x + radius * Math.cos(angle);
        const py = y + radius * Math.sin(angle);

        if (i === 0) {
            ctx.moveTo(px, py);
        } else {
            ctx.lineTo(px, py);
        }
    }

    ctx.closePath();

    if (fill) {
        ctx.fillStyle = fill;
        ctx.fill();
    }

    if (stroke) {
        ctx.strokeStyle = stroke;
        ctx.lineWidth = strokeWidth;
        ctx.stroke();
    }

    ctx.restore();
}

/**
 * ピクセルにスナップ（アンチエイリアシング回避用）
 * @param {number} value - 座標値
 * @returns {number} スナップ後の値
 */
export function snapToPixel(value) {
    return Math.floor(value) + 0.5;
}

/**
 * テキストの幅を測定
 * @param {CanvasRenderingContext2D} ctx - 描画コンテキスト
 * @param {string} text - テキスト
 * @param {number} fontSize - フォントサイズ
 * @param {string} fontFamily - フォントファミリー
 * @returns {number} テキスト幅
 */
export function measureTextWidth(ctx, text, fontSize = CONFIG.font.sizeMedium, fontFamily = CONFIG.font.family) {
    ctx.save();
    ctx.font = `${fontSize}px ${fontFamily}`;
    const width = ctx.measureText(text).width;
    ctx.restore();
    return width;
}

/**
 * Canvas全体をクリア
 * @param {CanvasRenderingContext2D} ctx - 描画コンテキスト
 * @param {number} width - Canvas幅
 * @param {number} height - Canvas高さ
 */
export function clearCanvas(ctx, width, height) {
    ctx.clearRect(0, 0, width, height);
}

/**
 * RGB値を16進数カラーコードに変換
 * @param {number} r - 赤（0-255）
 * @param {number} g - 緑（0-255）
 * @param {number} b - 青（0-255）
 * @returns {string} 16進数カラーコード（#RRGGBB）
 */
export function rgbToHex(r, g, b) {
    return '#' + [r, g, b].map(x => {
        const hex = x.toString(16);
        return hex.length === 1 ? '0' + hex : hex;
    }).join('');
}

/**
 * CONFIGから色を取得
 * @param {string} path - 色のパス（例: 'weekend.sun', 'slot.empty'）
 * @returns {string} 色コード
 */
export function getColor(path) {
    const parts = path.split('.');
    let value = CONFIG.colors;
    
    for (const part of parts) {
        value = value[part];
        if (value === undefined) {
            console.warn(`Color not found: ${path}`);
            return '#000000';
        }
    }
    
    return value;
}

/**
 * 描画バッチ処理のヘルパー
 * 同種の描画をまとめて実行することでパフォーマンス向上
 */
export class DrawBatch {
    constructor(ctx) {
        this.ctx = ctx;
        this.commands = [];
    }

    /**
     * 矩形描画コマンドを追加
     */
    addRect(params) {
        this.commands.push({ type: 'rect', params });
        return this;
    }

    /**
     * 線描画コマンドを追加
     */
    addLine(params) {
        this.commands.push({ type: 'line', params });
        return this;
    }

    /**
     * テキスト描画コマンドを追加
     */
    addText(params) {
        this.commands.push({ type: 'text', params });
        return this;
    }

    /**
     * 円描画コマンドを追加
     */
    addCircle(params) {
        this.commands.push({ type: 'circle', params });
        return this;
    }

    /**
     * 多角形描画コマンドを追加
     */
    addPolygon(params) {
        this.commands.push({ type: 'polygon', params });
        return this;
    }

    /**
     * バッチを実行
     */
    execute() {
        for (const command of this.commands) {
            switch (command.type) {
                case 'rect':
                    drawRect(this.ctx, command.params);
                    break;
                case 'line':
                    drawLine(this.ctx, command.params);
                    break;
                case 'text':
                    drawText(this.ctx, command.params);
                    break;
                case 'circle':
                    drawCircle(this.ctx, command.params);
                    break;
                case 'polygon':
                    drawPolygon(this.ctx, command.params);
                    break;
            }
        }
        this.commands = [];
    }

    /**
     * コマンドをクリア
     */
    clear() {
        this.commands = [];
    }
}


