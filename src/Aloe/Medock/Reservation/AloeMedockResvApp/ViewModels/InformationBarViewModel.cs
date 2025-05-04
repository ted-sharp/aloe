using R3;
using System.ComponentModel;
using System.Windows;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Aloe.Common.AloeCoreLib.Wpf.Extensions;
using Aloe.Common.AloeCoreLib.Mvvm;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

public class InformationBarViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public BindableReactiveProperty<string> Status { get; } = new("");

    public BindableReactiveProperty<Visibility> ProgressBarVisibility { get; } = new(Visibility.Collapsed);

    public static readonly int MinScale = 50;
    public static readonly int MaxScale = 200;
    public static readonly int WheelStepScale = 5;

    public BindableReactiveProperty<string> AppName { get; } = new(App.AppName);

    public BindableReactiveProperty<string> AppVersion { get; } = new(App.AppVersion);

    public BindableReactiveProperty<string> HostName { get; } = new(App.HostName);

    public BindableReactiveProperty<string> DatabaseName { get; } = new(App.DatabaseName);

    public BindableReactiveProperty<Visibility> DatabaseNameVisibility { get; } = new(Visibility.Collapsed);

    public BindableReactiveProperty<string> User { get; } = new(App.User?.DisplayName ?? "");

    public BindableReactiveProperty<string> Role { get; } = new("Administrator");

    public BindableReactiveProperty<string> LoginCount { get; } = new(App.User?.LoginSuccessCount.ToString() ?? "---");

    public BindableReactiveProperty<string> FailureCount { get; } = new(App.User?.LoginFailureCount.ToString() ?? "---");

    public BindableReactiveProperty<string> LoginAt { get; } = new(App.Session?.LoginAt.ToString("HH:mm:ss") ?? "--:--");

    public ObservableCollection<int> ScaleOptions { get; } =
        [
            50, 75, 100, 125, 150, 175, 200,
        ];

    public BindableReactiveProperty<int> SelectedScalePercentage { get; } = new(100);

    public BindableReactiveProperty<string> SelectedScalePercentageText { get; } = new("100");

    public ReactiveCommand<int> ZoomOutCommand { get; } = new();

    public ReactiveCommand<int> ZoomInCommand { get; } = new();

    //public ObservableCollection<string> Themes { get; } =
    //[
    //    "None",
    //    "System",
    //    "Light",
    //    "Dark",
    //];

    //public BindableReactiveProperty<string> SelectedTheme { get; } = new("System");

    public ReactiveCommand ShowLogWindowCommand { get; } = new();

    private readonly ILogger _logger;
    private readonly WindowService _windowService;

    private bool _isUpdating = false;
    private long _timestamp = 0;

    public InformationBarViewModel(
        ILogger<InformationBarViewModel> logger,
        WindowService windowService,
        IAuthGrpcService authGrpcService)
    {
        this._logger = logger;
        this._windowService = windowService;

        var d = R3.Disposable.CreateBuilder();

        this.DatabaseName.Subscribe(x =>
        {
            this.DatabaseNameVisibility.Value =
                String.IsNullOrWhiteSpace(x) ? Visibility.Collapsed : Visibility.Visible;
        }).AddTo(ref d);

        this.ZoomOutCommand.Subscribe(this.ChangeScale).AddTo(ref d);

        this.ZoomInCommand.Subscribe(this.ChangeScale).AddTo(ref d);

        this.ShowLogWindowCommand.Subscribe(this.ShowLogWindow).AddTo(ref d);

        this.SelectedScalePercentageText.Subscribe(text =>
        {
            if (this._isUpdating)
            {
                return;
            }
            this._isUpdating = true;

            if (Int32.TryParse(text, out var scale))
            {
                var delta = this.SelectedScalePercentage.Value - scale;
                this.ChangeScale(delta);
            }

            this._isUpdating = false;
        }).AddTo(ref d);

        this.SelectedScalePercentage.Subscribe(x =>
        {
            if (this._isUpdating)
            {
                return;
            }
            this._isUpdating = true;

            this.SelectedScalePercentageText.Value = x.ToString();

            this._isUpdating = false;
        }).AddTo(ref d);

        //this.SelectedTheme.Subscribe(this.SetTheme).AddTo(ref d);

        this.Disposable = d.Build();
    }

    //public void SetTheme(string userChoice)
    //{
    //    #pragma warning disable WPF0001
    //    Application.Current.ThemeMode = userChoice switch
    //    {
    //        "Light" => ThemeMode.Light,
    //        "Dark" => ThemeMode.Dark,
    //        "System" => ThemeMode.System,
    //        _ => ThemeMode.None,
    //    };
    //    #pragma warning restore WPF0001
    //}

    public void SetStatus(string status)
    {
        this.Status.Value = status;

        this._logger.LogTrace(status);
    }

    public void StartProgress(string status = "loading...")
    {
        this._timestamp = Stopwatch.GetTimestamp();
        this.ProgressBarVisibility.Value = Visibility.Visible;
        this.Status.Value = status;

        this._logger.LogTrace(status);
    }

    public void StopProgress(string status = "done.")
    {
        if (this._timestamp > 0)
        {
            var elapsedTime = Stopwatch.GetElapsedTime(this._timestamp);
            status = $"{status} ({elapsedTime.TotalMilliseconds:N0} ms)";
            this._timestamp = 0;
        }

        this.ProgressBarVisibility.Value = Visibility.Collapsed;
        this.Status.Value = status;

        this._logger.LogTrace(status);
    }

    /// <summary>
    /// スケールを変更し、MinScaleとMaxScaleを考慮して値を制限する
    /// </summary>
    /// <param name="delta">変更する量（正: 拡大、負: 縮小）</param>
    private void ChangeScale(int delta)
    {
        // 現在の値に delta を加算し、範囲をClampで強制する
        var scale = Math.Clamp(this.SelectedScalePercentage.Value + delta,
            InformationBarViewModel.MinScale,
            InformationBarViewModel.MaxScale);
        this.SelectedScalePercentage.Value = scale;

        this._logger.LogTrace($"Scale: {scale}");
    }

    private void ShowLogWindow(Unit _)
    {
        this._windowService.GetWindow<LogWindow>()?.ShowOrActivate();
    }
}
