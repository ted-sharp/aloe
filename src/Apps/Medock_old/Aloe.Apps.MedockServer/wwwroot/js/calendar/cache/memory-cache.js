/**
 * Memory Cache for Calendar Data
 * 
 * 10秒有効期限のメモリキャッシュ
 * LRUキャッシュ実装
 * 日別無効化対応
 */

const CACHE_EXPIRY_SECONDS = 10;
const MAX_CACHE_SIZE = 50 * 1024 * 1024; // 50MB

class MemoryCacheEntry {
    constructor(data, filterHash = '') {
        this.data = data;
        this.filterHash = filterHash;
        this.lastAccessed = Date.now();
        this.expiresAt = Date.now() + CACHE_EXPIRY_SECONDS * 1000;
        this.size = this.estimateSize(data);
    }

    estimateSize(data) {
        // 簡易的なサイズ推定（JSON文字列の長さ）
        return JSON.stringify(data).length * 2; // UTF-16として2バイト/文字
    }

    isExpired() {
        return Date.now() > this.expiresAt;
    }

    touch() {
        this.lastAccessed = Date.now();
    }
}

class MemoryCache {
    constructor() {
        this.cache = new Map();
        this.totalSize = 0;
    }

    /**
     * キャッシュキーを生成
     */
    generateKey(date, resourceId, filterHash = '') {
        return `mainStats_${date}_${resourceId}${filterHash ? '_' + filterHash : ''}`;
    }

    /**
     * データをキャッシュに保存
     */
    set(date, resourceId, data, filterHash = '') {
        const key = this.generateKey(date, resourceId, filterHash);
        const entry = new MemoryCacheEntry(data, filterHash);

        // 既存のエントリがあればサイズを減算
        if (this.cache.has(key)) {
            this.totalSize -= this.cache.get(key).size;
        }

        // サイズチェック
        if (this.totalSize + entry.size > MAX_CACHE_SIZE) {
            this.evictLRU();
        }

        this.cache.set(key, entry);
        this.totalSize += entry.size;
    }

    /**
     * キャッシュからデータを取得
     */
    get(date, resourceId, filterHash = '') {
        const key = this.generateKey(date, resourceId, filterHash);
        const entry = this.cache.get(key);

        if (!entry) {
            return null;
        }

        // 期限切れチェック
        if (entry.isExpired()) {
            this.cache.delete(key);
            this.totalSize -= entry.size;
            return null;
        }

        entry.touch();
        return entry.data;
    }

    /**
     * 指定日付のキャッシュを無効化
     */
    invalidateDate(date) {
        const prefix = `mainStats_${date}_`;
        const keysToDelete = [];

        for (const [key, entry] of this.cache.entries()) {
            if (key.startsWith(prefix)) {
                keysToDelete.push(key);
                this.totalSize -= entry.size;
            }
        }

        keysToDelete.forEach(key => this.cache.delete(key));
    }

    /**
     * 特定のキーのキャッシュを削除
     */
    delete(date, resourceId, filterHash = '') {
        const key = this.generateKey(date, resourceId, filterHash);
        const entry = this.cache.get(key);

        if (entry) {
            this.cache.delete(key);
            this.totalSize -= entry.size;
        }
    }

    /**
     * LRU（Least Recently Used）エントリを削除
     */
    evictLRU() {
        if (this.cache.size === 0) {
            return;
        }

        // 最も古くアクセスされたエントリを探す
        let oldestKey = null;
        let oldestTime = Infinity;

        for (const [key, entry] of this.cache.entries()) {
            if (entry.lastAccessed < oldestTime) {
                oldestTime = entry.lastAccessed;
                oldestKey = key;
            }
        }

        if (oldestKey) {
            const entry = this.cache.get(oldestKey);
            this.cache.delete(oldestKey);
            this.totalSize -= entry.size;
        }
    }

    /**
     * 期限切れのキャッシュをクリーンアップ
     */
    cleanup() {
        const keysToDelete = [];

        for (const [key, entry] of this.cache.entries()) {
            if (entry.isExpired()) {
                keysToDelete.push(key);
                this.totalSize -= entry.size;
            }
        }

        keysToDelete.forEach(key => this.cache.delete(key));
    }

    /**
     * 全キャッシュをクリア
     */
    clear() {
        this.cache.clear();
        this.totalSize = 0;
    }
}

// シングルトンインスタンス
const memoryCache = new MemoryCache();

// 定期的にクリーンアップ（30秒ごと）
setInterval(() => {
    memoryCache.cleanup();
}, 30000);

export const setMemoryCache = (date, resourceId, data, filterHash = '') => {
    memoryCache.set(date, resourceId, data, filterHash);
};

export const getMemoryCache = (date, resourceId, filterHash = '') => {
    return memoryCache.get(date, resourceId, filterHash);
};

export const invalidateDateMemoryCache = (date) => {
    memoryCache.invalidateDate(date);
};

export const deleteMemoryCache = (date, resourceId, filterHash = '') => {
    memoryCache.delete(date, resourceId, filterHash);
};

export const clearMemoryCache = () => {
    memoryCache.clear();
};

