// <copyright file="MainViewModel.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Aloe.Apps.Medock.MdPatientFinder.Services;
using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aloe.Apps.Medock.MdPatientFinder.ViewModels;

/// <summary>
/// 患者検索画面の ViewModel。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IPatientService _patientService;
    private readonly ViewerLauncher _viewerLauncher;

    /// <summary>
    /// <see cref="MainViewModel"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public MainViewModel(IPatientService patientService, ViewerLauncher viewerLauncher)
    {
        _patientService = patientService;
        _viewerLauncher = viewerLauncher;
    }

    /// <summary>検索キーワード。</summary>
    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    /// <summary>検索結果一覧。</summary>
    [ObservableProperty]
    private ObservableCollection<PatientSummary> _patients = [];

    /// <summary>選択中の患者。</summary>
    [ObservableProperty]
    private PatientSummary? _selectedPatient;

    /// <summary>検索結果の総件数。</summary>
    [ObservableProperty]
    private int _totalCount;

    /// <summary>読み込み中フラグ。</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>エラーメッセージ。</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>患者を検索する。</summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        this.IsLoading = true;
        this.ErrorMessage = null;

        try
        {
            var response = await _patientService.SearchAsync(new SearchPatientsRequest
            {
                Keyword = this.SearchKeyword,
                Page = 1,
                PageSize = 50,
            });

            this.Patients = new ObservableCollection<PatientSummary>(response.Items);
            this.TotalCount = response.TotalCount;
        }
        catch (Exception ex)
        {
            this.ErrorMessage = ex.Message;
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>選択中の患者を Viewer で開く。</summary>
    [RelayCommand]
    private async Task OpenViewerAsync()
    {
        if (this.SelectedPatient == null)
        {
            return;
        }

        await _viewerLauncher.LaunchAsync("MdPatient_Viewer", this.SelectedPatient.PtId);
    }

    /// <summary>選択中の患者を Editor で開く。</summary>
    [RelayCommand]
    private async Task OpenEditorAsync()
    {
        if (this.SelectedPatient == null)
        {
            return;
        }

        await _viewerLauncher.LaunchAsync("MdPatient_Editor", this.SelectedPatient.PtId);
    }
}
