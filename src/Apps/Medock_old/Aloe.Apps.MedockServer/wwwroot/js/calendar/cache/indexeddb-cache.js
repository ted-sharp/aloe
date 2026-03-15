/**
 * IndexedDB Cache for Calendar Data
 * 
 * 10秒有効期限の永続化キャッシュ
 * 日別無効化対応
 */

const DB_NAME = 'MedockCalendarCache';
const DB_VERSION = 1;
const STORE_NAME = 'mainStats';
const CACHE_EXPIRY_SECONDS = 10;

let db = null;

/**
 * IndexedDBを開く
 */
async function openDB() {
    if (db) {
        return db;
    }

    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onerror = () => {
            console.error('IndexedDB open error:', request.error);
            reject(request.error);
        };

        request.onsuccess = () => {
            db = request.result;
            resolve(db);
        };

        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            if (!db.objectStoreNames.contains(STORE_NAME)) {
                const store = db.createObjectStore(STORE_NAME, { keyPath: 'key' });
                store.createIndex('date', 'date', { unique: false });
                store.createIndex('expiresAt', 'expiresAt', { unique: false });
            }
        };
    });
}

/**
 * キャッシュキーを生成
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 * @param {string} resourceId - リソースID
 * @param {string} filterHash - フィルターハッシュ（オプション）
 */
function generateCacheKey(date, resourceId, filterHash = '') {
    return `mainStats_${date}_${resourceId}${filterHash ? '_' + filterHash : ''}`;
}

/**
 * 日付からキャッシュキーのプレフィックスを生成
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 */
function generateDatePrefix(date) {
    return `mainStats_${date}_`;
}

/**
 * データをキャッシュに保存
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 * @param {string} resourceId - リソースID
 * @param {object} data - 保存するデータ
 * @param {string} filterHash - フィルターハッシュ（オプション）
 */
export async function setCache(date, resourceId, data, filterHash = '') {
    try {
        const db = await openDB();
        const key = generateCacheKey(date, resourceId, filterHash);
        const expiresAt = new Date(Date.now() + CACHE_EXPIRY_SECONDS * 1000);

        const cacheEntry = {
            key,
            date,
            resourceId,
            filterHash,
            data,
            lastUpdatedAt: new Date().toISOString(),
            expiresAt: expiresAt.toISOString(),
            version: 1
        };

        const transaction = db.transaction([STORE_NAME], 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        await store.put(cacheEntry);

        return true;
    } catch (error) {
        console.error('IndexedDB setCache error:', error);
        return false;
    }
}

/**
 * キャッシュからデータを取得
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 * @param {string} resourceId - リソースID
 * @param {string} filterHash - フィルターハッシュ（オプション）
 */
export async function getCache(date, resourceId, filterHash = '') {
    try {
        const db = await openDB();
        const key = generateCacheKey(date, resourceId, filterHash);

        const transaction = db.transaction([STORE_NAME], 'readonly');
        const store = transaction.objectStore(STORE_NAME);
        const request = store.get(key);

        return new Promise((resolve, reject) => {
            request.onsuccess = () => {
                const entry = request.result;
                if (!entry) {
                    resolve(null);
                    return;
                }

                // 有効期限チェック
                const expiresAt = new Date(entry.expiresAt);
                if (expiresAt < new Date()) {
                    // 期限切れの場合は削除
                    deleteCache(date, resourceId, filterHash);
                    resolve(null);
                    return;
                }

                resolve(entry.data);
            };

            request.onerror = () => {
                reject(request.error);
            };
        });
    } catch (error) {
        console.error('IndexedDB getCache error:', error);
        return null;
    }
}

/**
 * 指定日付のキャッシュを無効化（削除）
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 */
export async function invalidateDateCache(date) {
    try {
        const db = await openDB();
        const transaction = db.transaction([STORE_NAME], 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        const dateIndex = store.index('date');

        const prefix = generateDatePrefix(date);
        const range = IDBKeyRange.lowerBound(prefix);
        const request = dateIndex.openCursor(range);

        return new Promise((resolve, reject) => {
            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    const entry = cursor.value;
                    if (entry.key.startsWith(prefix)) {
                        cursor.delete();
                    }
                    cursor.continue();
                } else {
                    resolve(true);
                }
            };

            request.onerror = () => {
                reject(request.error);
            };
        });
    } catch (error) {
        console.error('IndexedDB invalidateDateCache error:', error);
        return false;
    }
}

/**
 * 特定のキーのキャッシュを削除
 * @param {string} date - 日付文字列 (yyyy-MM-dd)
 * @param {string} resourceId - リソースID
 * @param {string} filterHash - フィルターハッシュ（オプション）
 */
export async function deleteCache(date, resourceId, filterHash = '') {
    try {
        const db = await openDB();
        const key = generateCacheKey(date, resourceId, filterHash);

        const transaction = db.transaction([STORE_NAME], 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        await store.delete(key);

        return true;
    } catch (error) {
        console.error('IndexedDB deleteCache error:', error);
        return false;
    }
}

/**
 * 期限切れのキャッシュをクリーンアップ
 */
export async function cleanupExpiredCache() {
    try {
        const db = await openDB();
        const transaction = db.transaction([STORE_NAME], 'readwrite');
        const store = transaction.objectStore(STORE_NAME);
        const expiresIndex = store.index('expiresAt');
        const now = new Date().toISOString();
        const range = IDBKeyRange.upperBound(now);

        const request = expiresIndex.openCursor(range);

        return new Promise((resolve, reject) => {
            request.onsuccess = (event) => {
                const cursor = event.target.result;
                if (cursor) {
                    cursor.delete();
                    cursor.continue();
                } else {
                    resolve(true);
                }
            };

            request.onerror = () => {
                reject(request.error);
            };
        });
    } catch (error) {
        console.error('IndexedDB cleanupExpiredCache error:', error);
        return false;
    }
}

