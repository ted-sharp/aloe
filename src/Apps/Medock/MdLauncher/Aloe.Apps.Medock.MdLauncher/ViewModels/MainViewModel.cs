// <copyright file="MainViewModel.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Aloe.Apps.Medock.MdLauncher.Services;
using Aloe.Apps.Medock.MdLauncherLib.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aloe.Apps.Medock.MdLauncher.ViewModels;

/// <summary>
/// メインウィンドウの ViewModel。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly LauncherGrpcClient _grpcClient;
    private readonly AppLauncher _appLauncher;
    private List<LauncherCategoryItem> _allCategories = [];

    /// <summary>カテゴリ一覧（フィルタ適用後）。</summary>
    [ObservableProperty]
    private ObservableCollection<LauncherCategoryItem> _categories = [];

    /// <summary>検索フィルタ。</summary>
    [ObservableProperty]
    private string _searchFilter = string.Empty;

    /// <summary>オーバーレイの表示状態。</summary>
    [ObservableProperty]
    private bool _isVisible;

    /// <summary>アプリ起動後イベント。</summary>
    public event EventHandler? AppLaunched;

    /// <summary>
    /// <see cref="MainViewModel"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public MainViewModel(LauncherGrpcClient grpcClient, AppLauncher appLauncher)
    {
        _grpcClient = grpcClient;
        _appLauncher = appLauncher;
    }

    partial void OnSearchFilterChanged(string value)
    {
        ApplyFilter();
    }

    /// <summary>メニューを表示する。</summary>
    [RelayCommand]
    private async Task ShowMenuAsync()
    {
        await RefreshConfigAsync();
        this.IsVisible = true;
    }

    /// <summary>メニューを非表示にする。</summary>
    [RelayCommand]
    private void HideMenu()
    {
        this.IsVisible = false;
        this.SearchFilter = string.Empty;
    }

    /// <summary>アプリを起動する。</summary>
    [RelayCommand]
    private async Task LaunchAppAsync(LauncherAppItem app)
    {
        await _appLauncher.LaunchAsync(app);
        AppLaunched?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>設定を再取得する。</summary>
    [RelayCommand]
    private async Task RefreshConfigAsync()
    {
        var config = await _grpcClient.GetConfigAsync();
        _allCategories = config.Categories;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(this.SearchFilter))
        {
            this.Categories = new ObservableCollection<LauncherCategoryItem>(_allCategories);
            return;
        }

        var filter = this.SearchFilter;
        var filtered = _allCategories
            .Select(c => new LauncherCategoryItem
            {
                Id = c.Id,
                Name = c.Name,
                SortOrder = c.SortOrder,
                Apps = c.Apps
                    .Where(a =>
                        a.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        a.Description.Contains(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList(),
            })
            .Where(c => c.Apps.Count > 0)
            .ToList();

        this.Categories = new ObservableCollection<LauncherCategoryItem>(filtered);
    }
}
