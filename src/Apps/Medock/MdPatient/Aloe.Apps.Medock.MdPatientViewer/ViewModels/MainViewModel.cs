// <copyright file="MainViewModel.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aloe.Apps.Medock.MdPatientViewer.ViewModels;

/// <summary>
/// 患者閲覧画面の ViewModel。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly IPatientService _patientService;

    /// <summary>
    /// <see cref="MainViewModel"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public MainViewModel(IPatientService patientService)
    {
        _patientService = patientService;
    }

    /// <summary>患者詳細。</summary>
    [ObservableProperty]
    private PatientDetailResponse? _patientDetail;

    /// <summary>読み込み中フラグ。</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>エラーメッセージ。</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>患者を ID で読み込む。</summary>
    [RelayCommand]
    public async Task LoadPatientAsync(Guid patientId)
    {
        this.IsLoading = true;
        this.ErrorMessage = null;

        try
        {
            var response = await _patientService.GetByIdAsync(new GetPatientRequest { PtId = patientId });
            this.PatientDetail = response;
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
}
