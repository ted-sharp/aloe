using R3;
using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.Logging;
using System.Windows.Input;
using System.Reactive.Linq;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Common.AloeCoreLib.Wpf.Behaviors;
using Aloe.Common.AloeCoreLib.Wpf.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Key = System.Windows.Input.Key;
using Aloe.Common.AloeCoreLib.Mvvm;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

/// <summary>
/// FunctionBar の各コマンドで実行するための Function を定義します。
/// </summary>
/// <param name="Key"><see cref="FunctionKey"/>を使います。</param>
/// <param name="Name">ボタンに表示するテキストです。</param>
/// <param name="FuncAsync">コマンドで実行する処理です。コマンド自体はF1とAltF1で共通ですので、メソッド内でガード節を使ってください。</param>
public record Function(string Key, string Name, Func<Task> FuncAsync);

// バインドするだけで、実行の定義などは親側でやる
public class FunctionBarViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{

    #region IDisposable

    private DisposableBag _disposable;

    private bool _disposed = false;

    public new void Dispose()
    {
        if (!this._disposed)
        {
            this._disposable.Dispose();
            this._disposed = true;
        }

        base.Dispose();

        GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    private Dictionary<string /* key */, Function>? _functions;

    /// <summary>
    /// <see cref="Common.AloeCoreLib.Wpf.Behaviors.Key"/> で Window の KeyDown にバインドします。
    /// </summary>
    public ReactiveCommand<KeyEventArgs> KeyDownCommand { get; } = new();

    /// <summary>
    /// Altキーが押されるたびにトグルします。
    /// </summary>
    /// <remarks>
    /// Alt + F1 などの同時押しを想定していましたが、
    /// キーリピートでイベントが上手く処理できないため、トグルにしています。
    /// ひとまず名前はそのままです。
    /// </remarks>
    public BindableReactiveProperty<bool> IsAltKeyPressed { get; } = new();

    public BindableReactiveProperty<string> EscText { get; } = new();
    public BindableReactiveProperty<string> F1Text { get; } = new();
    public BindableReactiveProperty<string> F2Text { get; } = new();
    public BindableReactiveProperty<string> F3Text { get; } = new();
    public BindableReactiveProperty<string> F4Text { get; } = new();
    public BindableReactiveProperty<string> F5Text { get; } = new();
    public BindableReactiveProperty<string> F6Text { get; } = new();
    public BindableReactiveProperty<string> F7Text { get; } = new();
    public BindableReactiveProperty<string> F8Text { get; } = new();
    public BindableReactiveProperty<string> F9Text { get; } = new();
    public BindableReactiveProperty<string> F10Text { get; } = new();
    public BindableReactiveProperty<string> F11Text { get; } = new();
    public BindableReactiveProperty<string> F12Text { get; } = new();

    /// <summary>
    /// ESCコマンドの実行制御プロパティです。
    /// </summary>
    public ReactiveProperty<bool> EscCanExecute { get; } = new(false);

    /// <summary>
    /// 共通コマンドの実行制御プロパティです。
    /// </summary>
    /// <remarks>
    /// コマンドをバインドしているボタンの IsEnabled プロパティに影響します。
    /// CanExecute が false のときは、コマンドを実行しようとしても無視されます。
    /// </remarks>
    public ReactiveProperty<bool> SharedCanExecute { get; } = new(true);

    public ReactiveCommand EscCommand { get; }
    public ReactiveCommand F1Command { get; }
    public ReactiveCommand F2Command { get; }
    public ReactiveCommand F3Command { get; }
    public ReactiveCommand F4Command { get; }
    public ReactiveCommand F5Command { get; }
    public ReactiveCommand F6Command { get; }
    public ReactiveCommand F7Command { get; }
    public ReactiveCommand F8Command { get; }
    public ReactiveCommand F9Command { get; }
    public ReactiveCommand F10Command { get; }
    public ReactiveCommand F11Command { get; }
    public ReactiveCommand F12Command { get; }

    private readonly ILogger _logger;
    private readonly WindowService _windowService;

    public FunctionBarViewModel(
        ILogger<FunctionBarViewModel> logger,
        WindowService windowService)
    {
        this._logger = logger;
        this._windowService = windowService;

        this.KeyDownCommand
            .Subscribe(this.OnKeyDown)
            .AddTo(ref this._disposable);

        this.IsAltKeyPressed
            .Subscribe(this.RefreshFunctionText)
            .AddTo(ref this._disposable);

        this.EscCommand = InitializeCommand(this.EscCanExecute, this.EscText);
        this.F1Command = InitializeCommand(this.SharedCanExecute, this.F1Text);
        this.F2Command = InitializeCommand(this.SharedCanExecute, this.F2Text);
        this.F3Command = InitializeCommand(this.SharedCanExecute, this.F3Text);
        this.F4Command = InitializeCommand(this.SharedCanExecute, this.F4Text);
        this.F5Command = InitializeCommand(this.SharedCanExecute, this.F5Text);
        this.F6Command = InitializeCommand(this.SharedCanExecute, this.F6Text);
        this.F7Command = InitializeCommand(this.SharedCanExecute, this.F7Text);
        this.F8Command = InitializeCommand(this.SharedCanExecute, this.F8Text);
        this.F9Command = InitializeCommand(this.SharedCanExecute, this.F9Text);
        this.F10Command = InitializeCommand(this.SharedCanExecute, this.F10Text);
        this.F11Command = InitializeCommand(this.SharedCanExecute, this.F11Text);
        this.F12Command = InitializeCommand(this.SharedCanExecute, this.F12Text);

        return;

        // local function
        ReactiveCommand InitializeCommand(
            ReactiveProperty<bool> sharedCanExecute,
            ReactiveProperty<string> functionTextProp)
        {
            // sharedCanExecute で他のコマンドを実行時に、実行できないようにします
            return sharedCanExecute
                .CombineLatest(
                    // ボタンのテキストが空であれば、実行できないようにします
                    functionTextProp.Select(text => !String.IsNullOrWhiteSpace(text)),
                    (canExecute, isNotEmpty) => canExecute && isNotEmpty)
                //.Catch(Observable.Return(false))
                .ToReactiveCommand()
                .AddTo(ref this._disposable);
        }
    }

    #region Function

    /// <summary>
    /// ファンクションボタンを設定します。
    /// </summary>
    public void InitializeFunctions(Dictionary<string /* key */, Function> functions)
    {
        this._functions = functions;
        this.SubscribeFunctions();
        this.RefreshFunctionText(this.IsAltKeyPressed.Value);
    }

    private void SubscribeFunctions()
    {
        SubscribeFunction(FunctionKey.Esc, this.EscCommand);

        SubscribeFunction(FunctionKey.F1, this.F1Command);
        SubscribeFunction(FunctionKey.F2, this.F2Command);
        SubscribeFunction(FunctionKey.F3, this.F3Command);
        SubscribeFunction(FunctionKey.F4, this.F4Command);
        SubscribeFunction(FunctionKey.F5, this.F5Command);
        SubscribeFunction(FunctionKey.F6, this.F6Command);
        SubscribeFunction(FunctionKey.F7, this.F7Command);
        SubscribeFunction(FunctionKey.F8, this.F8Command);
        SubscribeFunction(FunctionKey.F9, this.F9Command);
        SubscribeFunction(FunctionKey.F10, this.F10Command);
        SubscribeFunction(FunctionKey.F11, this.F11Command);
        SubscribeFunction(FunctionKey.F12, this.F12Command);

        // 通常のコマンドと共通なので注意が必要です
        SubscribeFunction(FunctionKey.AltF1, this.F1Command);
        SubscribeFunction(FunctionKey.AltF2, this.F2Command);
        SubscribeFunction(FunctionKey.AltF3, this.F3Command);
        SubscribeFunction(FunctionKey.AltF4, this.F4Command);
        SubscribeFunction(FunctionKey.AltF5, this.F5Command);
        SubscribeFunction(FunctionKey.AltF6, this.F6Command);
        SubscribeFunction(FunctionKey.AltF7, this.F7Command);
        SubscribeFunction(FunctionKey.AltF8, this.F8Command);
        SubscribeFunction(FunctionKey.AltF9, this.F9Command);
        SubscribeFunction(FunctionKey.AltF10, this.F10Command);
        SubscribeFunction(FunctionKey.AltF11, this.F11Command);
        SubscribeFunction(FunctionKey.AltF12, this.F12Command);
        return;

        // local function
        //void SubscribeFunction(string key, ReactiveCommand command)
        //{
        //    if (this._functions?.TryGetValue(key, out var function) ?? false)
        //    {
        //        command.Subscribe(function.Action).AddTo(this.Disposables);
        //    }
        //}
        void SubscribeFunction(string key, ReactiveCommand command)
        {
            if (this._functions?.TryGetValue(key, out var function) ?? false)
            {
                command.SubscribeAwait(async (_, cancellationToken) =>
                {
                    try
                    {
                        this.SharedCanExecute.Value = false;

                        // ある程度のディレイを入れないと Button.IsEnabled の切り替えが遅くなる
                        await Task.Delay(200, cancellationToken);

                        await function.FuncAsync.Invoke();
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, "Error!");
                    }
                    finally
                    {
                        this.SharedCanExecute.Value = true;
                    }
                }).AddTo(ref this._disposable);
            }
        }
    }

    public void RefreshFunctionText(bool isAltKeyPressed = false)
    {
        if (isAltKeyPressed)
        {
            this.EscText.Value = GetFunctionText(FunctionKey.Esc);
            this.F1Text.Value = GetFunctionText(FunctionKey.AltF1);
            this.F2Text.Value = GetFunctionText(FunctionKey.AltF2);
            this.F3Text.Value = GetFunctionText(FunctionKey.AltF3);
            this.F4Text.Value = GetFunctionText(FunctionKey.AltF4);
            this.F5Text.Value = GetFunctionText(FunctionKey.AltF5);
            this.F6Text.Value = GetFunctionText(FunctionKey.AltF6);
            this.F7Text.Value = GetFunctionText(FunctionKey.AltF7);
            this.F8Text.Value = GetFunctionText(FunctionKey.AltF8);
            this.F9Text.Value = GetFunctionText(FunctionKey.AltF9);
            this.F10Text.Value = GetFunctionText(FunctionKey.AltF10);
            this.F11Text.Value = GetFunctionText(FunctionKey.AltF11);
            this.F12Text.Value = GetFunctionText(FunctionKey.AltF12);
            this._logger.LogInformation("Alt が押されました。");
        }
        else
        {
            this.EscText.Value = GetFunctionText(FunctionKey.Esc);
            this.F1Text.Value = GetFunctionText(FunctionKey.F1);
            this.F2Text.Value = GetFunctionText(FunctionKey.F2);
            this.F3Text.Value = GetFunctionText(FunctionKey.F3);
            this.F4Text.Value = GetFunctionText(FunctionKey.F4);
            this.F5Text.Value = GetFunctionText(FunctionKey.F5);
            this.F6Text.Value = GetFunctionText(FunctionKey.F6);
            this.F7Text.Value = GetFunctionText(FunctionKey.F7);
            this.F8Text.Value = GetFunctionText(FunctionKey.F8);
            this.F9Text.Value = GetFunctionText(FunctionKey.F9);
            this.F10Text.Value = GetFunctionText(FunctionKey.F10);
            this.F11Text.Value = GetFunctionText(FunctionKey.F11);
            this.F12Text.Value = GetFunctionText(FunctionKey.F12);
            this._logger.LogInformation("Alt が離れました。");
        }

        return;

        // local function
        string GetFunctionText(string key) => this._functions?.GetValueOrDefault(key)?.Name ?? "";
    }

    private void OnKeyDown(KeyEventArgs e)
    {
        if (!e.IsRepeat && (e.SystemKey == Key.LeftAlt || e.Key == Key.RightAlt))
        {
            this.IsAltKeyPressed.Value ^= true;
            e.Handled = true;
        }

        var command = GetCommand(e.Key)
            ?? GetCommand(e.SystemKey);
        if (command?.CanExecute(null) ?? false)
        {
            command.Execute(null);
            e.Handled = true;
        }

        return;

        // local function
        ICommand? GetCommand(Key key) => key switch
        {
            Key.Escape => this.EscCommand,
            Key.F1 => this.F1Command,
            Key.F2 => this.F2Command,
            Key.F3 => this.F3Command,
            Key.F4 => this.F4Command,
            Key.F5 => this.F5Command,
            Key.F6 => this.F6Command,
            Key.F7 => this.F7Command,
            Key.F8 => this.F8Command,
            Key.F9 => this.F9Command,
            Key.F10 => this.F10Command,
            Key.F11 => this.F11Command,
            Key.F12 => this.F12Command,
            _ => null,
        };
    }

    #endregion Function

    #region Common Command Method

    /// <summary>
    /// Windowを開くコマンドです。
    /// </summary>
    /// <typeparam name="TWindow">開きたいWindowの型です。</typeparam>
    /// <typeparam name="TParent">Ownerに指定するWindowの型です。</typeparam>
    /// <param name="isAltCond">Altキーありのコマンドの場合は true を設定します。</param>
    public Task ExecuteOpenCommand<TWindow, TParent>(bool isAltCond = false)
        where TWindow : Window
        where TParent : Window
    {
        try
        {
            //this.SharedCanExecute.Value = false;
            var isAlt = this.IsAltKeyPressed.Value;
            if (isAlt == isAltCond)
            {
                var window = this._windowService.GetOrCreateWindow<TWindow>();

                var parent = this._windowService.GetWindow<TParent>();
                if (parent != null)
                {
                    window.Owner = parent;
                }

                window?.ShowOrActivate();
            }

            return Task.CompletedTask;
        }
        finally
        {
            //this.SharedCanExecute.Value = true;
        }
    }

    public Task ExecuteOpenCommand<TWindow>(bool isAltCond = false)
        where TWindow : Window
    {
        try
        {
            //this.SharedCanExecute.Value = false;
            var isAlt = this.IsAltKeyPressed.Value;
            if (isAlt == isAltCond)
            {
                var window = this._windowService.GetOrCreateWindow<TWindow>();

                window?.ShowOrActivate();
            }

            return Task.CompletedTask;
        }
        finally
        {
            //this.SharedCanExecute.Value = true;
        }
    }

    /// <summary>
    /// Windowを閉じるコマンドです。
    /// </summary>
    /// <typeparam name="TWindow">開きたいWindowの型です。</typeparam>
    /// <param name="isAltCond">Altキーありのコマンドの場合は true を設定します。</param>
    public Task ExecuteCloseCommand<TWindow>(bool isAltCond = false)
        where TWindow : Window
    {
        try
        {
            //this.SharedCanExecute.Value = false;
            var isAlt = this.IsAltKeyPressed.Value;
            if (isAlt == isAltCond)
            {
                var window = this._windowService.GetWindow<TWindow>();
                window?.Close();
            }

            return Task.CompletedTask;
        }
        finally
        {
            //this.SharedCanExecute.Value = true;
        }
    }

    /// <summary>
    /// 特定のプロパティに日付文字列を設定するコマンドです。
    /// </summary>
    public Task ExecuteSetDateCommand(ReactiveProperty<DateOnly> dateProp, DateOnly date, bool isAltCond = false)
    {
        try
        {
            //this.SharedCanExecute.Value = false;
            var isAlt = this.IsAltKeyPressed.Value;
            if (isAlt == isAltCond)
            {
                dateProp.Value = date;
            }

            return Task.CompletedTask;
        }
        finally
        {
            //this.SharedCanExecute.Value = true;
        }
    }

    public Task ExecuteSetTodayCommand(ReactiveProperty<DateOnly> dateProp, bool isAltCond = false)
    {
        return this.ExecuteSetDateCommand(dateProp, DateOnlyHelper.GetToday(), isAltCond);
    }

    public Task ExecuteSetCurrentMonthCommand(ReactiveProperty<DateOnly> dateProp, bool isAltCond = false)
    {
        var date = DateOnlyHelper.GetToday();
        var newDate = date.AddDays(1 - date.Day);
        return this.ExecuteSetDateCommand(dateProp, newDate, isAltCond);
    }

    public Task ExecuteAddDaysCommand(ReactiveProperty<DateOnly> dateProp, int days, bool isAltCond = false)
    {
        var date = dateProp.Value;
        var newDate = date.AddDays(days);
        return this.ExecuteSetDateCommand(dateProp, newDate, isAltCond);
    }

    public Task ExecutePrevDateCommand(ReactiveProperty<DateOnly> dateProp, bool isAltCond = false)
    {
        return this.ExecuteAddDaysCommand(dateProp, -1, isAltCond);
    }

    public Task ExecuteNextDateCommand(ReactiveProperty<DateOnly> dateProp, bool isAltCond = false)
    {
        return this.ExecuteAddDaysCommand(dateProp, 1, isAltCond);
    }

    public Task ExecuteAddMonthsCommand(ReactiveProperty<DateOnly> dateProp, int months, bool isAltCond = false)
    {
        var date = dateProp.Value;
        var newDate = date.AddMonths(months);
        return this.ExecuteSetDateCommand(dateProp, newDate, isAltCond);
    }

    public Task ExecutePrevMonthCommand(ReactiveProperty<DateOnly> dateProp, bool isAltCond = false)
    {
        return this.ExecuteAddMonthsCommand(dateProp, -1, isAltCond);
    }

    public Task ExecuteNextMonthCommand(ReactiveProperty<DateOnly> dateProp, bool isAltCond = false)
    {
        return this.ExecuteAddMonthsCommand(dateProp, 1, isAltCond);
    }

    #endregion Common Command Method
}
