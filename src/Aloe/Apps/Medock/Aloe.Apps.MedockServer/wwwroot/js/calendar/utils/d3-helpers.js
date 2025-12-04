/**
 * D3.js Helper Functions
 *
 * D3.js を使用した円弧（arc）の計算を行うユーティリティ関数
 * グローバルの window.d3 を使用
 */

/**
 * 円弧のSVGパス文字列を生成
 * @param {number} startAngle - 開始角度（ラジアン）
 * @param {number} endAngle - 終了角度（ラジアン）
 * @param {number} innerRadius - 内側の半径
 * @param {number} outerRadius - 外側の半径
 * @returns {string} SVGパス文字列
 */
export function createArcPath(startAngle, endAngle, innerRadius, outerRadius) {
    const arc = d3.arc()
        .innerRadius(innerRadius)
        .outerRadius(outerRadius)
        .startAngle(startAngle)
        .endAngle(endAngle);
    return arc();
}

/**
 * AM/PM の使用率から円グラフ用の角度データを計算
 * @param {number} amRatio - AM の使用率（0.0 ～ 1.0）
 * @param {number} pmRatio - PM の使用率（0.0 ～ 1.0）
 * @returns {Object} AM/PM の filled/empty 領域の角度データ
 */
export function calculatePieData(amRatio, pmRatio) {
    // AM: left half (-PI/2 to PI/2)
    // PM: right half (PI/2 to 3*PI/2)
    return {
        am: {
            filled: { start: -Math.PI / 2, end: -Math.PI / 2 + Math.PI * amRatio },
            empty: { start: -Math.PI / 2 + Math.PI * amRatio, end: Math.PI / 2 }
        },
        pm: {
            filled: { start: Math.PI / 2, end: Math.PI / 2 + Math.PI * pmRatio },
            empty: { start: Math.PI / 2 + Math.PI * pmRatio, end: 3 * Math.PI / 2 }
        }
    };
}
