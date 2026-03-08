/**
 * Web Speech API ラッパー
 *
 * 設計方針:
 *   continuous=true で聞き続け、isFinal な断片を蓄積する。
 *   無音が DEBOUNCE_MS 続いたら蓄積テキスト全体を「確定」として送る。
 *   ユーザーが途中で止まっても、続きを話せば蓄積に追加される。
 *   「確定」ボタン or ストップボタン or 無音タイムアウトで最終確定。
 *
 * Blazorコールバック:
 *   OnTranscriptUpdated(fullText, latestInterim)  - リアルタイム表示用
 *   OnTranscriptFinalized(fullText)                - 確定（パース対象）
 *   OnSpeechError(errorCode)
 *   OnSpeechEnded()
 */

/** @type {SpeechRecognition | null} */
let recognition = null;

/** @type {object | null} */
let dotNetReference = null;

/** @type {boolean} */
let isListening = false;

/** 確定済み断片の蓄積 @type {string[]} */
let finalizedFragments = [];

/** 最新の interim テキスト（まだ isFinal でない断片） @type {string} */
let currentInterim = '';

/** finalize 済みフラグ（二重呼び出し防止） @type {boolean} */
let isFinalized = false;

/** デバウンスタイマー @type {number | null} */
let debounceTimer = null;

/** 無音と判定するまでの待機時間(ms) */
const DEBOUNCE_MS = 3000;

/** 最大録音時間(ms) - 安全弁 */
const MAX_LISTEN_MS = 30000;

/** 最大録音タイマー @type {number | null} */
let maxTimer = null;

/** バー表示遅延タイマー（発話後0.5s沈黙でバーを出す） @type {number | null} */
let displayTimer = null;

/** バー表示までの待機時間(ms) */
const DISPLAY_DELAY_MS = 1500;

/**
 * 音声認識を初期化する
 * @param {object} dotNetRef - Blazor DotNetObjectReference
 * @returns {boolean} 音声認識が利用可能な場合 true
 */
export function initialize(dotNetRef) {
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!SpeechRecognition) {
        console.warn('[SpeechRecognition] Web Speech API is not supported in this browser.');
        return false;
    }

    dotNetReference = dotNetRef;

    recognition = new SpeechRecognition();
    recognition.lang = 'ja-JP';
    recognition.continuous = true;
    recognition.interimResults = true;
    recognition.maxAlternatives = 1;

    recognition.onresult = handleResult;
    recognition.onerror = handleError;
    recognition.onend = handleEnd;

    return true;
}

/**
 * @param {SpeechRecognitionEvent} event
 */
function handleResult(event) {
    // finalize 済みなら無視（stop() 後に届く残りイベント）
    if (isFinalized) return;

    currentInterim = '';

    for (let i = event.resultIndex; i < event.results.length; i++) {
        const result = event.results[i];
        const transcript = result[0].transcript.trim();
        if (!transcript) continue;

        if (result.isFinal) {
            finalizedFragments.push(transcript);
        } else {
            currentInterim = transcript;
        }
    }

    // 音声活動があったのでデバウンスをリセット
    resetDebounce();

    const fullText = buildFullText();
    dotNetReference?.invokeMethodAsync('OnTranscriptUpdated', fullText, currentInterim);
}

/**
 * デバウンスタイマーをリセット。
 * 最後の音声活動から DEBOUNCE_MS 後に確定を送る。
 */
function resetDebounce() {
    if (debounceTimer !== null) {
        clearTimeout(debounceTimer);
    }
    debounceTimer = setTimeout(() => {
        finalize();
    }, DEBOUNCE_MS);

    // バーを即時非表示にし、0.5s後に1.5sカウントダウンバーを表示
    if (displayTimer !== null) {
        clearTimeout(displayTimer);
    }
    dotNetReference?.invokeMethodAsync('OnSpeechActivity');
    displayTimer = setTimeout(() => {
        dotNetReference?.invokeMethodAsync('OnDebounceReset');
    }, DISPLAY_DELAY_MS);
}

/**
 * 蓄積テキストを確定として送り、録音を停止する。
 * interim テキストも含めて確定する（確定ボタン対応）。
 */
function finalize() {
    if (isFinalized) return;
    isFinalized = true;

    clearTimers();

    // interim も含めた全テキストで確定
    const fullText = buildFullText();
    if (fullText) {
        dotNetReference?.invokeMethodAsync('OnTranscriptFinalized', fullText);
    }

    // 録音停止（onend が発火する）
    if (recognition && isListening) {
        recognition.stop();
    }
}

/**
 * finalized + interim を連結した全文を返す
 * @returns {string}
 */
function buildFullText() {
    const parts = [...finalizedFragments];
    if (currentInterim) {
        parts.push(currentInterim);
    }
    return parts.join('');
}

function clearTimers() {
    if (debounceTimer !== null) {
        clearTimeout(debounceTimer);
        debounceTimer = null;
    }
    if (maxTimer !== null) {
        clearTimeout(maxTimer);
        maxTimer = null;
    }
    if (displayTimer !== null) {
        clearTimeout(displayTimer);
        displayTimer = null;
    }
}

/**
 * @param {SpeechRecognitionErrorEvent} event
 */
function handleError(event) {
    if (event.error === 'aborted') return;

    console.warn('[SpeechRecognition] Error:', event.error);
    clearTimers();
    isListening = false;
    isFinalized = true;
    dotNetReference?.invokeMethodAsync('OnSpeechError', event.error);
}

function handleEnd() {
    clearTimers();
    isListening = false;
    dotNetReference?.invokeMethodAsync('OnSpeechEnded');
}

/**
 * 音声認識を開始する
 */
export function startListening() {
    if (!recognition) {
        console.warn('[SpeechRecognition] Not initialized.');
        return;
    }
    if (isListening) return;

    finalizedFragments = [];
    currentInterim = '';
    isFinalized = false;
    isListening = true;
    recognition.start();

    // 安全弁: 最大録音時間で強制確定
    maxTimer = setTimeout(() => {
        finalize();
    }, MAX_LISTEN_MS);

    // 開始直後から30sカウントダウンバーを表示する
    dotNetReference?.invokeMethodAsync('OnListeningStarted');
}

/**
 * 音声認識を停止する（確定ボタン / 停止ボタン）
 * 蓄積テキスト + interim テキストを確定として送る。
 */
export function stopListening() {
    if (!recognition || !isListening) return;
    finalize();
}

/**
 * 蓄積テキストをリセットして録音を継続する。
 * 音声コマンド（「コマンド確定」「コマンドリセット」等）で使用。
 * @param {string} baseText - リセット後の初期テキスト（空文字なら全クリア）
 */
export function resetFragments(baseText = '') {
    finalizedFragments = baseText ? [baseText] : [];
    currentInterim = '';
    isFinalized = false;
    // デバウンスをリセット（録音継続するため）
    resetDebounce();
}

/**
 * リソースを解放する
 */
export function dispose() {
    clearTimers();
    if (recognition && isListening) {
        recognition.stop();
    }
    recognition = null;
    dotNetReference = null;
    finalizedFragments = [];
    currentInterim = '';
    isListening = false;
    isFinalized = false;
}
