using AloeReservationGrid.Lib.CoreLib.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AloeReservationGrid.App.ReservationApp.Views.Login;
using AloeReservationGrid.Lib.ReservationLib.Grpc.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using AloeReservationGrid.App.ReservationApp.Views.Resv;
using System.Collections.ObjectModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Windows.Input;
using System.Reactive.Linq;
using AloeReservationGrid.Lib.CoreLib.Util;
using AloeReservationGrid.Lib.ReservationLib.Domain.Constants;
using System.DirectoryServices.ActiveDirectory;
using AloeReservationGrid.App.ReservationApp.Views.Maint;

namespace AloeReservationGrid.App.ReservationApp.ViewModels;

/// <summary>
/// FunctionBar の各コマンドで実行するための Function を定義します。
/// </summary>
/// <param name="Key"><see cref="FunctionKey"/>を使います。</param>
/// <param name="Name">ボタンに表示するテキストです。</param>
/// <param name="Action">コマンドで実行する処理です。コマンド自体はF1とAltF1で共通ですので、メソッド内でガード節を使ってください。</param>
public record Function(string Key, string Name, Action Action);

// バインドするだけで、実行の定義などは親側でやる
public class FunctionBarViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    private Dictionary<string /* key */, Function>? _functions;

    /// <summary>
    /// <see cref="Views.Behaviors.KeyInputBehavior"/> で Window の KeyDown にバインドします。
    /// </summary>
    public ReactiveCommandSlim<KeyEventArgs> KeyDownCommand { get; } = new();

    /// <summary>
    /// Altキーが押されるたびにトグルします。
    /// </summary>
    /// <remarks>
    /// Alt + F1 などの同時押しを想定していましたが、
    /// キーリピートでイベントが上手く処理できないため、トグルにしています。
    /// ひとまず名前はそのままです。
    /// </remarks>
    public ReactivePropertySlim<bool> IsAltKeyPressed { get; } = new ();

    public ReactivePropertySlim<string> EscText { get; } = new();
    public ReactivePropertySlim<string> F1Text { get; } = new();
    public ReactivePropertySlim<string> F2Text { get; } = new();
    public ReactivePropertySlim<string> F3Text { get; } = new();
    public ReactivePropertySlim<string> F4Text { get; } = new();
    public ReactivePropertySlim<string> F5Text { get; } = new();
    public ReactivePropertySlim<string> F6Text { get; } = new();
    public ReactivePropertySlim<string> F7Text { get; } = new();
    public ReactivePropertySlim<string> F8Text { get; } = new();
    public ReactivePropertySlim<string> F9Text { get; } = new();
    public ReactivePropertySlim<string> F10Text { get; } = new();
    public ReactivePropertySlim<string> F11Text { get; } = new();
    public ReactivePropertySlim<string> F12Text { get; } = new();

    public ReactivePropertySlim<bool> EscCanExecute { get; } = new(false);

    public ReactivePropertySlim<bool> SharedCanExecute { get; } = new(true);

    public ReactiveCommandSlim EscCommand { get; }
    public ReactiveCommandSlim F1Command { get; }
    public ReactiveCommandSlim F2Command { get; }
    public ReactiveCommandSlim F3Command { get; }
    public ReactiveCommandSlim F4Command { get; }
    public ReactiveCommandSlim F5Command { get; }
    public ReactiveCommandSlim F6Command { get; }
    public ReactiveCommandSlim F7Command { get; }
    public ReactiveCommandSlim F8Command { get; }
    public ReactiveCommandSlim F9Command { get; }
    public ReactiveCommandSlim F10Command { get; }
    public ReactiveCommandSlim F11Command { get; }
    public ReactiveCommandSlim F12Command { get; }

    private readonly ILogger _logger;

    public FunctionBarViewModel(
        ILogger<FunctionBarViewModel> logger)
    {
        this._logger = logger;

        this.KeyDownCommand
            .Subscribe(this.OnKeyDown)
            .AddTo(this.Disposables);

        this.IsAltKeyPressed
            .Subscribe(this.RefreshFunctionText)
            .AddTo(this.Disposables);

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
        ReactiveCommandSlim InitializeCommand(
            ReactivePropertySlim<bool> sharedCanExecute,
            ReactivePropertySlim<string> functionTextProp)
        {
            // sharedCanExecute で他のコマンドを実行時に、実行できないようにします
            return sharedCanExecute
                .CombineLatest(
                    // ボタンのテキストが空であれば、実行できないようにします
                    functionTextProp.Select(text => !String.IsNullOrWhiteSpace(text)),
                    (canExecute, isNotEmpty) => canExecute && isNotEmpty)
                .Catch(Observable.Return(false))
                .ToReactiveCommandSlim()
                .AddTo(this.Disposables);
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
        void SubscribeFunction(string key, ReactiveCommandSlim command)
        {
            if (this._functions?.TryGetValue(key, out var function) ?? false)
            {
                command.Subscribe(function.Action).AddTo(this.Disposables);
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

    // よく使う共通コマンドはこちらで定義しておく

    /// <summary>
    /// Windowを開くコマンドです。
    /// </summary>
    /// <typeparam name="TWindow">開きたいWindowの型です。</typeparam>
    /// <typeparam name="TParent">Ownerに指定するWindowの型です。</typeparam>
    /// <param name="isAltCond">Altキーありのコマンドの場合は true を設定します。</param>
    public void ExecuteOpenCommand<TWindow, TParent>(bool isAltCond = false)
        where TWindow : Window
        where TParent : Window
    {
        try
        {
            this.SharedCanExecute.Value = false;
            var isAlt = this.IsAltKeyPressed.Value;
            if (isAlt == isAltCond)
            {
                var window = App.GetOrCreateWindow<TWindow>();

                var parent = App.GetWindow<TParent>();
                if (parent != null)
                {
                    window.Owner = parent;
                }

                window?.ActivateOrShow();
            }
        }
        finally
        {
            this.SharedCanExecute.Value = true;
        }
    }

    public void ExecuteOpenCommand<TWindow>(bool isAltCond = false)
        where TWindow : Window
    {
        try
        {
            this.SharedCanExecute.Value = false;
            var isAlt = this.IsAltKeyPressed.Value;
            if (isAlt == isAltCond)
            {
                var window = App.GetOrCreateWindow<TWindow>();

                window?.ActivateOrShow();
            }
        }
        finally
        {
            this.SharedCanExecute.Value = true;
        }
    }

    /// <summary>
    /// Windowを閉じるコマンドです。
    /// </summary>
    /// <typeparam name="TWindow">開きたいWindowの型です。</typeparam>
    /// <param name="isAltCond">Altキーありのコマンドの場合は true を設定します。</param>
    public void ExecuteCloseCommand<TWindow>(bool isAltCond = false)
        where TWindow : Window
    {
        try
        {
            this.SharedCanExecute.Value = false;
            var isAlt = this.IsAltKeyPressed.Value;
            if (isAlt == isAltCond)
            {
                var window = App.GetWindow<TWindow>();
                window?.Close();
            }
        }
        finally
        {
            this.SharedCanExecute.Value = true;
        }
    }

    /// <summary>
    /// 特定のプロパティに日付文字列を設定するコマンドです。
    /// </summary>
    public void ExecuteSetDateTimeCommand(ReactivePropertySlim<string> dateProp, DateTime date, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        try
        {
            this.SharedCanExecute.Value = false;
            var isAlt = this.IsAltKeyPressed.Value;
            if (isAlt == isAltCond)
            {
                dateProp.Value = date.ToString(format);
            }
        }
        finally
        {
            this.SharedCanExecute.Value = true;
        }
    }

    public void ExecuteSetTodayCommand(ReactivePropertySlim<string> dateProp, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        this.ExecuteSetDateTimeCommand(dateProp, DateTime.Today, format, isAltCond);
    }

    public void ExecuteSetCurrentMonthCommand(ReactivePropertySlim<string> dateProp, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        var date = DateTime.Today;
        var newDate = date.AddDays(1 - date.Day);
        this.ExecuteSetDateTimeCommand(dateProp, newDate, format, isAltCond);
    }

    public void ExecuteAddDateCommand(ReactivePropertySlim<string> dateProp, TimeSpan span, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        var date = dateProp.Value.ToDateOrToday();
        var newDate = date.Add(span);
        this.ExecuteSetDateTimeCommand(dateProp, newDate, format, isAltCond);
    }

    public void ExecutePrevDateCommand(ReactivePropertySlim<string> dateProp, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        this.ExecuteAddDateCommand(dateProp, TimeSpan.FromDays(-1), format);
    }

    public void ExecuteNextDateCommand(ReactivePropertySlim<string> dateProp, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        this.ExecuteAddDateCommand(dateProp, TimeSpan.FromDays(1), format);
    }

    public void ExecuteAddMonthCommand(ReactivePropertySlim<string> dateProp, int monthSpan, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        var date = dateProp.Value.ToDateOrToday();
        var newDate = date.AddMonths(monthSpan);
        this.ExecuteSetDateTimeCommand(dateProp, newDate, format, isAltCond);
    }

    public void ExecutePrevMonthCommand(ReactivePropertySlim<string> dateProp, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        this.ExecuteAddMonthCommand(dateProp, -1, format, isAltCond);
    }

    public void ExecuteNextMonthCommand(ReactivePropertySlim<string> dateProp, string format = "yyyy.MM.dd", bool isAltCond = false)
    {
        this.ExecuteAddMonthCommand(dateProp, 1, format, isAltCond);
    }

    #endregion Common Command Method
}
