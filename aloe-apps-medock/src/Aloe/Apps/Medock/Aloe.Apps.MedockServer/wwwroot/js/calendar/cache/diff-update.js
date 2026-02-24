/**
 * Diff Update Strategy
 * 
 * 日別キャッシュ無効化と差分取得の実装
 */

import { getCache, setCache, invalidateDateCache } from './indexeddb-cache.js';
import { getMemoryCache, setMemoryCache, invalidateDateMemoryCache } from './memory-cache.js';

/**
 * フィルターハッシュを計算
 * @param {object} filter - フィルターオブジェクト
 */
function computeFilterHash(filter) {
    if (!filter || !filter.IsActive) {
        return '';
    }

    const filterData = {
        floorIds: filter.SelectedFloorIds?.sort() || [],
        resourceGroupIds: filter.SelectedResourceGroupIds?.sort() || [],
        resourceIds: filter.SelectedResourceIds?.sort() || [],
        planIds: filter.SelectedPlanIds?.sort() || [],
        optionPlanIds: filter.SelectedOptionPlanIds?.sort() || []
    };

    // 簡易的なハッシュ計算（実際の実装ではより堅牢なハッシュ関数を使用）
    const hashString = JSON.stringify(filterData);
    let hash = 0;
    for (let i = 0; i < hashString.length; i++) {
        const char = hashString.charCodeAt(i);
        hash = ((hash << 5) - hash) + char;
        hash = hash & hash; // Convert to 32bit integer
    }
    return Math.abs(hash).toString(16);
}

/**
 * 日別キャッシュを無効化
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 */
export async function invalidateDateCaches(date) {
    // IndexedDBとメモリキャッシュの両方を無効化
    await invalidateDateCache(date);
    invalidateDateMemoryCache(date);
}

/**
 * キャッシュからデータを取得（優先順位: メモリ → IndexedDB）
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 * @param {string} resourceId - リソースID
 * @param {object} filter - フィルターオブジェクト
 */
export async function getCachedData(date, resourceId, filter = null) {
    const filterHash = computeFilterHash(filter);

    // 1. メモリキャッシュから取得
    const memoryData = getMemoryCache(date, resourceId, filterHash);
    if (memoryData) {
        return memoryData;
    }

    // 2. IndexedDBから取得
    const indexedDBData = await getCache(date, resourceId, filterHash);
    if (indexedDBData) {
        // メモリキャッシュにも保存
        setMemoryCache(date, resourceId, indexedDBData, filterHash);
        return indexedDBData;
    }

    return null;
}

/**
 * データをキャッシュに保存（メモリとIndexedDBの両方）
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 * @param {string} resourceId - リソースID
 * @param {object} data - 保存するデータ
 * @param {object} filter - フィルターオブジェクト
 */
export async function setCachedData(date, resourceId, data, filter = null) {
    const filterHash = computeFilterHash(filter);

    // メモリキャッシュとIndexedDBの両方に保存
    setMemoryCache(date, resourceId, data, filterHash);
    await setCache(date, resourceId, data, filterHash);
}

/**
 * 差分データを取得（サーバーから）
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 * @param {Array<string>} resourceIds - リソースIDのリスト
 * @param {object} filter - フィルターオブジェクト
 */
export async function fetchDiffData(date, resourceIds, filter = null) {
    try {
        // APIエンドポイントを呼び出し（実装が必要）
        const response = await fetch('/api/calendar/stats/diff', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                date,
                resourceIds,
                filter
            })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();
        return data;
    } catch (error) {
        console.error('Failed to fetch diff data:', error);
        throw error;
    }
}

/**
 * 差分更新処理
 * @param {string} date - 更新された日付文字列 (yyyy-MM-dd)
 * @param {Array<string>} resourceIds - 更新されたリソースIDのリスト
 * @param {object} filter - フィルターオブジェクト
 */
export async function applyDiffUpdate(date, resourceIds, filter = null) {
    try {
        // 1. キャッシュを無効化
        await invalidateDateCaches(date);

        // 2. 差分データを取得
        const diffData = await fetchDiffData(date, resourceIds, filter);

        // 3. キャッシュを更新
        if (diffData && Array.isArray(diffData)) {
            for (const item of diffData) {
                const resourceId = item.apptResId || item.ApptResId;
                if (resourceId) {
                    await setCachedData(date, resourceId.toString(), item, filter);
                }
            }
        }

        return diffData;
    } catch (error) {
        console.error('Failed to apply diff update:', error);
        throw error;
    }
}

