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

namespace Aloe.Medock.Reservation.AloeMedockResvApp.ViewModels;

public class InformationBarViewModel : ViewModelBase, INotifyPropertyChanged, IDisposable
{
    public ReactivePropertySlim<string> Status { get; } = new("");

    public ReactivePropertySlim<string> User { get; } = new(App.Session?.UserDisplayName ?? "");

    public ReactivePropertySlim<string> HostName { get; } = new(App.HostName);

    public ReactivePropertySlim<Visibility> ProgressBarVisibility { get; } = new(Visibility.Collapsed);

    public static readonly int MinScale = 50;
    public static readonly int MaxScale = 200;
    public static readonly int WheelStepScale = 5;

    public ObservableCollection<int> ScaleOptions { get; } =
        [
            50, 75, 100, 125, 150, 175, 200,
        ];

    public ReactivePropertySlim<int> SelectedPercentage { get; } = new(100);

    public ReactivePropertySlim<string> SelectedPercentageText { get; } = new("100");

    public ReactiveCommandSlim<int> ZoomOutCommand { get; } = new();

    public ReactiveCommandSlim<int> ZoomInCommand { get; } = new();

    public ReactiveCommandSlim ShowLogWindowCommand { get; } = new();

    private readonly ILogger _logger;
    private readonly WindowService _windowService;

    private bool _isUpdating = false;

    public InformationBarViewModel(
        ILogger<InformationBarViewModel> logger,
        WindowService windowService,
        IAuthGrpcService authGrpcService)
    {
        this._logger = logger;
        this._windowService = windowService;

        this.ZoomOutCommand.Subscribe(this.ChangeScale);

        this.ZoomInCommand.Subscribe(this.ChangeScale);

        this.ShowLogWindowCommand.Subscribe(this.ShowLogWindow);

        this.SelectedPercentageText.Subscribe(text =>
        {
            if (this._isUpdating)
            {
                return;
            }
            this._isUpdating = true;

            if (Int32.TryParse(text, out var scale))
            {
                var delta = this.SelectedPercentage.Value - scale;
                this.ChangeScale(delta);
            }

            this._isUpdating = false;
        });

        this.SelectedPercentage.Subscribe(x =>
        {
            if (this._isUpdating)
            {
                return;
            }
            this._isUpdating = true;

            this.SelectedPercentageText.Value = x.ToString();

            this._isUpdating = false;
        });
    }

    private void StartProgress(string status)
    {
        this.Status.Value = status;
        this.ProgressBarVisibility.Value = Visibility.Visible;
    }

    private void StopProgress(string status)
    {
        this.Status.Value = status;
        this.ProgressBarVisibility.Value = Visibility.Collapsed;
    }

    /// <summary>
    /// スケールを変更し、MinScaleとMaxScaleを考慮して値を制限する
    /// </summary>
    /// <param name="delta">変更する量（正: 拡大、負: 縮小）</param>
    private void ChangeScale(int delta)
    {
        // 現在の値に delta を加算し、範囲をClampで強制する
        var scale = Math.Clamp(this.SelectedPercentage.Value + delta,
            InformationBarViewModel.MinScale,
            InformationBarViewModel.MaxScale);
        this.SelectedPercentage.Value = scale;
    }

    private void ShowLogWindow()
    {
        this._windowService.GetWindow<LogWindow>()?.ShowOrActivate();
    }
}
