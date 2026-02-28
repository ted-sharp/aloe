using System.Text.RegularExpressions;
using Aloe.Apps.MedockLib.Services;
using Aloe.Apps.MedockLib.Services.VoiceCommand;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Aloe.Apps.MedockServer.Components.Voice;

/// <summary>
/// 音声コマンド入力ボタン。
/// Web Speech API で音声認識し、正規表現パーサーで予約コマンドに変換する。
///
/// 動作フロー:
///   1. マイク押下 → continuous モードで録音開始
///   2. 断片テキストが蓄積され、リアルタイムでカードに表示
///   3. 1.5秒の無音 or 停止ボタン → 蓄積テキスト全体をパース
///   4. 有効なコマンドなら確認UI → 実行ボタンで確定
///   5. Unknown なら「もう一度お試しください」表示
///
/// ハンズフリー操作:
///   「コマンド確定」「コマンド実行」「コマンドリセット」「コマンドクリア」
///   を音声で言うと対応するボタン操作が実行される。
/// </summary>
public partial class VoiceCommandButton : ComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IVoiceCommandParser Parser { get; set; } = default!;
    [Inject] private IDateTimeProvider DateTimeProvider { get; set; } = default!;

    /// <summary>
    /// パース済みコマンドの確定時コールバック
    /// </summary>
    [Parameter] public EventCallback<VoiceCommandResult> OnCommandReady { get; set; }

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<VoiceCommandButton>? _dotNetRef;
    private bool _isSupported;
    private bool _isListening;
    private bool _showResultCard;
    private string? _currentFullText;
    private string? _errorMessage;
    private VoiceCommandResult? _parsedResult;

    /// <summary>音声コマンド処理中フラグ（OnTranscriptFinalized の二重処理を防ぐ）</summary>
    private bool _processingVoiceCommand;

    private enum BarState { Hidden, Initial30s, Countdown15s }
    private BarState _barState = BarState.Hidden;

    /// <summary>カウントダウンバーのアニメーション再起動用キー</summary>
    private int _debounceKey = 0;

    // 固定ボタンバーの有効/無効判定
    private bool CanReset => true;
    private bool CanFinalize => _isListening && !string.IsNullOrEmpty(_currentFullText);
    private bool CanExecute => _parsedResult is not null;

    // --- 音声コマンドトリガー定義 ---
    // 「コマンド○○」で対応するボタン操作をハンズフリー実行
    private static readonly (string[] Triggers, VoiceAction Action)[] VoiceActions =
    [
        (["コマンドリセット", "コマンドクリア"], VoiceAction.Reset),
        (["コマンド確定", "コマンドかくてい", "コマンドけってい",
          "コマンド確定して", "コマンドかくていして", "コマンドけっていして"], VoiceAction.Finalize),
        (["コマンド実行", "コマンドじっこう",
          "コマンド実行して", "コマンドじっこうして"], VoiceAction.Execute),
    ];

    private enum VoiceAction { Reset, Finalize, Execute }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        _dotNetRef = DotNetObjectReference.Create(this);
        _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/voice/speech-recognition.js");
        _isSupported = await _jsModule.InvokeAsync<bool>("initialize", _dotNetRef);
        StateHasChanged();
    }

    private async Task ToggleListening()
    {
        if (_jsModule is null)
            return;

        if (_isListening)
        {
            // 停止 → 蓄積テキストで確定（JS側が finalize を呼ぶ）
            await _jsModule.InvokeVoidAsync("stopListening");
        }
        else
        {
            // 前回の結果をクリアして開始
            _currentFullText = null;
            _errorMessage = null;
            _parsedResult = null;
            _showResultCard = true;

            await _jsModule.InvokeVoidAsync("startListening");
            _isListening = true;
        }
    }

    /// <summary>
    /// JS から呼ばれる: 録音開始通知（30sカウントダウンバーを表示）
    /// </summary>
    [JSInvokable]
    public async Task OnListeningStarted()
    {
        _barState = BarState.Initial30s;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// JS から呼ばれる: 発話検出通知（バーを即時非表示）
    /// </summary>
    [JSInvokable]
    public async Task OnSpeechActivity()
    {
        _barState = BarState.Hidden;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// JS から呼ばれる: 発話後0.5s沈黙通知（1.5sカウントダウンバーを表示）
    /// </summary>
    [JSInvokable]
    public async Task OnDebounceReset()
    {
        _barState = BarState.Countdown15s;
        _debounceKey++;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// JS から呼ばれる: 蓄積テキストのリアルタイム更新
    /// </summary>
    [JSInvokable]
    public void OnTranscriptUpdated(string fullText, string latestInterim)
    {
        if (_processingVoiceCommand)
            return;

        // 空テキストは無視（resetFragments 後の残余イベント対策）
        if (string.IsNullOrEmpty(fullText))
            return;

        // 音声コマンド検出（「コマンド確定」「コマンド実行」等）
        var detected = DetectVoiceAction(fullText);
        if (detected is not null)
        {
            var (strippedText, action) = detected.Value;
            // コマンド除去後のテキストが残る場合のみ_currentFullTextを更新
            // （空の場合は既存テキストとパース結果を保持）
            if (!string.IsNullOrEmpty(strippedText))
            {
                _currentFullText = strippedText;
                _parsedResult = null;
                _errorMessage = null;
            }
            _processingVoiceCommand = true;
            InvokeAsync(async () =>
            {
                await HandleVoiceActionAsync(action);
                _processingVoiceCommand = false;
                StateHasChanged();
            });
            return;
        }

        // 通常の更新（コマンドトリガーなし）
        // 録音中はパース結果をクリア（まだ話している可能性がある）
        _currentFullText = fullText;
        _parsedResult = null;
        _errorMessage = null;
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// JS から呼ばれる: 無音デバウンス後の確定テキスト
    /// </summary>
    [JSInvokable]
    public void OnTranscriptFinalized(string fullText)
    {
        // 音声コマンド処理中なら無視（コマンド側で処理済み）
        if (_processingVoiceCommand)
            return;

        _currentFullText = fullText;
        _isListening = false;
        ParseAndSetResult(fullText);
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// JS から呼ばれるエラーコールバック
    /// </summary>
    [JSInvokable]
    public void OnSpeechError(string errorCode)
    {
        _isListening = false;
        _errorMessage = errorCode switch
        {
            "no-speech" => "音声が検出されませんでした。もう一度お試しください。",
            "audio-capture" => "マイクが見つかりません。マイクの接続を確認してください。",
            "not-allowed" => "マイクへのアクセスが許可されていません。ブラウザの設定を確認してください。",
            _ => $"音声認識エラー: {errorCode}",
        };
        InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// JS から呼ばれる音声認識終了コールバック
    /// </summary>
    [JSInvokable]
    public void OnSpeechEnded()
    {
        _isListening = false;
        InvokeAsync(StateHasChanged);
    }

    private async Task ExecuteCommand()
    {
        if (_parsedResult is null)
            return;

        await OnCommandReady.InvokeAsync(_parsedResult);

        _showResultCard = false;
        _parsedResult = null;
        _currentFullText = null;
        _errorMessage = null;
    }

    /// <summary>
    /// 録音中に「確定」ボタンを押した場合。
    /// デバウンスを待たずに即座に蓄積テキストでパースする。
    /// </summary>
    private async Task FinalizeManually()
    {
        if (_jsModule is null)
            return;

        // JS側の finalize() → stopListening → OnTranscriptFinalized コールバックが発火する
        await _jsModule.InvokeVoidAsync("stopListening");
    }

    private async Task CancelResult()
    {
        if (_isListening && _jsModule is not null)
        {
            // 録音中はキャンセルと同時に停止（finalize は走るが結果は無視される）
            await _jsModule.InvokeVoidAsync("stopListening");
            _isListening = false;
        }

        _showResultCard = false;
        _parsedResult = null;
        _currentFullText = null;
        _errorMessage = null;
    }

    /// <summary>
    /// 蓄積テキスト・パース結果・エラーをクリアして録音を再開する。
    /// 録音中でも呼べるよう、先に stopListening してから startListening を呼ぶ。
    /// </summary>
    private async Task ResetAndRestart()
    {
        if (_jsModule is null)
            return;

        if (_isListening)
            await _jsModule.InvokeVoidAsync("stopListening");

        _errorMessage = null;
        _parsedResult = null;
        _currentFullText = null;
        _isListening = false;

        await _jsModule.InvokeVoidAsync("startListening");
        _isListening = true;
    }

    // --- 音声コマンド検出 ---

    /// <summary>
    /// 蓄積テキストの末尾から音声コマンドを検出する。
    /// 検出した場合、コマンド部分を除去したテキストとアクションを返す。
    /// </summary>
    private static (string StrippedText, VoiceAction Action)? DetectVoiceAction(string fullText)
    {
        // スペースを除去して末尾マッチ
        var normalized = fullText.Replace(" ", "").Replace("\u3000", "");

        foreach (var (triggers, action) in VoiceActions)
        {
            foreach (var trigger in triggers)
            {
                if (normalized.EndsWith(trigger, StringComparison.Ordinal))
                {
                    var stripped = normalized[..^trigger.Length].Trim();
                    return (stripped, action);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 音声コマンドに対応するアクションを実行する。
    /// 「確定」後も録音を継続し、「実行」をハンズフリーで待つ。
    /// </summary>
    private async Task HandleVoiceActionAsync(VoiceAction action)
    {
        switch (action)
        {
            case VoiceAction.Reset:
                _errorMessage = null;
                _parsedResult = null;
                _currentFullText = null;
                // JS側のフラグメントをクリアして録音継続
                if (_jsModule is not null)
                    await _jsModule.InvokeVoidAsync("resetFragments", "");
                break;

            case VoiceAction.Finalize:
                if (!string.IsNullOrEmpty(_currentFullText))
                {
                    ParseAndSetResult(_currentFullText);
                    // JS側のフラグメントをクリアして録音継続（「コマンド実行」を待つ）
                    if (_jsModule is not null)
                        await _jsModule.InvokeVoidAsync("resetFragments", "");
                }
                break;

            case VoiceAction.Execute:
                if (_parsedResult is not null)
                {
                    await OnCommandReady.InvokeAsync(_parsedResult);
                    _showResultCard = false;
                    _parsedResult = null;
                    _currentFullText = null;
                    _errorMessage = null;
                    // 実行完了 → 録音停止
                    if (_jsModule is not null && _isListening)
                        await _jsModule.InvokeVoidAsync("stopListening");
                    _isListening = false;
                }
                break;
        }
    }

    /// <summary>
    /// テキストをパースして結果またはエラーメッセージをセットする。
    /// </summary>
    private void ParseAndSetResult(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _errorMessage = "音声が検出されませんでした。もう一度お試しください。";
            _parsedResult = null;
            return;
        }

        var result = Parser.Parse(text);
        if (result.Intent == VoiceCommandIntent.Unknown)
        {
            _errorMessage = "コマンドを認識できませんでした。「予約を入れて」「予約を確認して」のように話してください。";
            _parsedResult = null;
        }
        else
        {
            _parsedResult = result;
            _errorMessage = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule is not null)
        {
            await _jsModule.InvokeVoidAsync("dispose");
            await _jsModule.DisposeAsync();
        }

        _dotNetRef?.Dispose();
        GC.SuppressFinalize(this);
    }
}
