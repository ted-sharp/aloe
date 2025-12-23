/**
 * Bar Chart Renderer
 *
 * 棒グラフ形式での日付セル描画のエントリーポイント
 * 年表示・月表示で使用される
 */

// 年間/月間カレンダー用（集約版）
export { renderDayBarChart } from './day-cell-rendering.js';
// 日詳細ビュー用（詳細版）
export { renderDayDetailBarChart } from './day-detail-cell-rendering.js';
