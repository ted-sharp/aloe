// <copyright file="MainWindow.xaml.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Aloe.Apps.Medock.MdLauncher.Models;
using Aloe.Apps.Medock.MdLauncher.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Aloe.Apps.Medock.MdLauncher;

/// <summary>
/// フルスクリーンオーバーレイメニューウィンドウ。
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly BehaviorOptions _behaviorOptions;

    /// <summary>
    /// <see cref="MainWindow"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public MainWindow(MainViewModel viewModel, BehaviorOptions behaviorOptions)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _behaviorOptions = behaviorOptions;
        this.DataContext = viewModel;

        viewModel.PropertyChanged += this.ViewModel_PropertyChanged;
        viewModel.AppLaunched += this.ViewModel_AppLaunched;
    }

    /// <summary>
    /// オーバーレイを表示する。
    /// </summary>
    public void ShowOverlay()
    {
        this.Show();
        this.Opacity = 0;
        var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(_behaviorOptions.ShowAnimationDurationMs));
        this.BeginAnimation(OpacityProperty, animation);
        SearchBox.Focus();
    }

    /// <summary>
    /// オーバーレイを非表示にする。
    /// </summary>
    public void HideOverlay()
    {
        var animation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(_behaviorOptions.ShowAnimationDurationMs));
        animation.Completed += (s, e) => this.Hide();
        this.BeginAnimation(OpacityProperty, animation);
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.IsVisible))
        {
            if (_viewModel.IsVisible)
            {
                ShowOverlay();
            }
            else
            {
                HideOverlay();
            }
        }
    }

    private void ViewModel_AppLaunched(object? sender, EventArgs e)
    {
        if (_behaviorOptions.AutoMinimizeAfterLaunch)
        {
            _viewModel.HideMenuCommand.Execute(null);
        }
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            _viewModel.HideMenuCommand.Execute(null);
        }
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource == sender || e.OriginalSource is System.Windows.Controls.Grid)
        {
            _viewModel.HideMenuCommand.Execute(null);
        }
    }
}
