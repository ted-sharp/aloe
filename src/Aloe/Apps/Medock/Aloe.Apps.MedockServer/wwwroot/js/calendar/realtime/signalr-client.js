/**
 * SignalR Client for Appointment Stats Updates
 * 
 * リアルタイム更新通知の受信と処理
 */

// SignalRはグローバル変数として読み込まれる
const signalR = window.SignalR || window.signalR;

import { applyDiffUpdate } from '../cache/diff-update.js';

let connection = null;
let isConnected = false;
let reconnectAttempts = 0;
const MAX_RECONNECT_ATTEMPTS = 5;

/**
 * SignalR接続を開始
 * @param {function} onStatsUpdated - 統計更新時のコールバック
 */
export async function startConnection(onStatsUpdated) {
    if (connection && isConnected) {
        return;
    }

    if (!signalR) {
        console.error('SignalR library not loaded');
        return;
    }

    try {
        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/appointmentStats')
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.previousRetryCount < MAX_RECONNECT_ATTEMPTS) {
                        return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
                    }
                    return null; // 再接続を停止
                }
            })
            .build();

        // 統計更新通知を受信
        connection.on('OnStatsUpdated', async (updatedDate, updatedResourceIds) => {
            // console.log('AppointmentStats updated:', { updatedDate, updatedResourceIds });

            try {
                // 差分更新を適用
                const diffData = await applyDiffUpdate(updatedDate, updatedResourceIds);
                
                // コールバックを呼び出し（UI更新など）
                if (onStatsUpdated) {
                    onStatsUpdated(updatedDate, updatedResourceIds, diffData);
                }
            } catch (error) {
                console.error('Failed to handle stats update:', error);
            }
        });

        // 接続状態の監視
        connection.onclose(() => {
            isConnected = false;
            // console.log('SignalR connection closed');
        });

        connection.onreconnecting(() => {
            isConnected = false;
            reconnectAttempts++;
            // console.log(`SignalR reconnecting... (attempt ${reconnectAttempts})`);
        });

        connection.onreconnected(() => {
            isConnected = true;
            reconnectAttempts = 0;
            // console.log('SignalR reconnected');
        });

        // 接続開始
        await connection.start();
        isConnected = true;
        reconnectAttempts = 0;
        // console.log('SignalR connection started');

        // カレンダーグループに参加（必要に応じて）
        // await connection.invoke('JoinCalendarGroup', 'default');
    } catch (error) {
        console.error('Failed to start SignalR connection:', error);
        isConnected = false;
    }
}

/**
 * SignalR接続を停止
 */
export async function stopConnection() {
    if (connection) {
        try {
            await connection.stop();
            isConnected = false;
            // console.log('SignalR connection stopped');
        } catch (error) {
            console.error('Failed to stop SignalR connection:', error);
        }
    }
}

/**
 * カレンダーグループに参加
 * @param {string} calendarId - カレンダーID
 */
export async function joinCalendarGroup(calendarId) {
    if (connection && isConnected) {
        try {
            await connection.invoke('JoinCalendarGroup', calendarId);
            // console.log(`Joined calendar group: ${calendarId}`);
        } catch (error) {
            console.error('Failed to join calendar group:', error);
        }
    }
}

/**
 * カレンダーグループから退出
 * @param {string} calendarId - カレンダーID
 */
export async function leaveCalendarGroup(calendarId) {
    if (connection && isConnected) {
        try {
            await connection.invoke('LeaveCalendarGroup', calendarId);
            // console.log(`Left calendar group: ${calendarId}`);
        } catch (error) {
            console.error('Failed to leave calendar group:', error);
        }
    }
}

/**
 * 接続状態を取得
 */
export function isConnectionActive() {
    return isConnected && connection && signalR && connection.state === signalR.HubConnectionState?.Connected;
}

