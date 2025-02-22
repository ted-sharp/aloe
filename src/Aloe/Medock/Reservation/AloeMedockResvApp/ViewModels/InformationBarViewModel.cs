using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Aloe.Medock.Reservation.AloeMedockResvApp.Services;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Login;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using Microsoft.Extensions.Logging;
using Reactive.Bindings.Extensions;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Resv;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Services;
using System.Collections.ObjectModel;
using Aloe.Common.AloeCoreLib.Client.Mvvm;
using Aloe.Medock.Reservation.AloeMedockResvApp.Utils;
using Aloe.Medock.Reservation.AloeMedockResvApp.Views.Maint;
using Serilog.Events;
using Grpc.Core;
using System.Diagnostics;
using Aloe.Common.AloeCoreLib.Wpf.Extensions;
using ControlzEx.Standard;

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

public class InformationBarViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactivePropertySlim<string> Status { get; } = new("");

    public ReactivePropertySlim<Visibility> ProgressBarVisibility { get; } = new(Visibility.Collapsed);

    public static readonly int MinScale = 50;
    public static readonly int MaxScale = 200;
    public static readonly int WheelStepScale = 5;

    public ReactivePropertySlim<string> User { get; } = new(App.Session?.UserDisplayName ?? "");

    public ReactivePropertySlim<string> HostName { get; } = new(App.HostName);

    public ReactivePropertySlim<string> DatabaseName { get; } = new(App.DatabaseName);

    public ReactivePropertySlim<Visibility> DatabaseNameVisibility { get; } = new(Visibility.Collapsed);

    public ObservableCollection<int> ScaleOptions { get; } =
        [
            50, 75, 100, 125, 150, 175, 200,
        ];

    public ReactivePropertySlim<int> SelectedScalePercentage { get; } = new(100);

    public ReactivePropertySlim<string> SelectedScalePercentageText { get; } = new("100");

    public ReactiveCommandSlim<int> ZoomOutCommand { get; } = new();

    public ReactiveCommandSlim<int> ZoomInCommand { get; } = new();

    public ReactiveCommandSlim ShowLogWindowCommand { get; } = new();

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

        this.DatabaseName.Subscribe(x =>
        {
            this.DatabaseNameVisibility.Value =
                String.IsNullOrWhiteSpace(x) ? Visibility.Collapsed : Visibility.Visible;
        }).AddTo(this.Disposables);

        this.ZoomOutCommand.Subscribe(this.ChangeScale).AddTo(this.Disposables);

        this.ZoomInCommand.Subscribe(this.ChangeScale).AddTo(this.Disposables);

        this.ShowLogWindowCommand.Subscribe(this.ShowLogWindow).AddTo(this.Disposables);

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
        }).AddTo(this.Disposables);

        this.SelectedScalePercentage.Subscribe(x =>
        {
            if (this._isUpdating)
            {
                return;
            }
            this._isUpdating = true;

            this.SelectedScalePercentageText.Value = x.ToString();

            this._isUpdating = false;
        }).AddTo(this.Disposables);
    }

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

    private void ShowLogWindow()
    {
        this._windowService.GetWindow<LogWindow>()?.ShowOrActivate();
    }
}
