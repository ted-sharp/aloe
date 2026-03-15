// <copyright file="MainViewModel.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aloe.Apps.Medock.MdPatientEditor.ViewModels;

/// <summary>
/// 患者編集画面の ViewModel。
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

    /// <summary>患者 ID。</summary>
    [ObservableProperty]
    private Guid _ptId;

    /// <summary>患者コード。</summary>
    [ObservableProperty]
    private string _ptCode = string.Empty;

    /// <summary>カルテコード。</summary>
    [ObservableProperty]
    private string? _karteCode;

    /// <summary>患者名。</summary>
    [ObservableProperty]
    private string _ptName = string.Empty;

    /// <summary>患者名カタカナ。</summary>
    [ObservableProperty]
    private string _ptNameKatakana = string.Empty;

    /// <summary>旧姓名。</summary>
    [ObservableProperty]
    private string _ptMaidenName = string.Empty;

    /// <summary>別名・芸名。</summary>
    [ObservableProperty]
    private string _ptAliasName = string.Empty;

    /// <summary>生年月日。</summary>
    [ObservableProperty]
    private string _birthDate = string.Empty;

    /// <summary>性別コード。</summary>
    [ObservableProperty]
    private int _sexCode;

    /// <summary>メモ。</summary>
    [ObservableProperty]
    private string _ptMemo = string.Empty;

    /// <summary>新規作成モードかどうか。</summary>
    [ObservableProperty]
    private bool _isNewMode;

    /// <summary>読み込み中フラグ。</summary>
    [ObservableProperty]
    private bool _isLoading;

    /// <summary>エラーメッセージ。</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>ステータスメッセージ。</summary>
    [ObservableProperty]
    private string? _statusMessage;

    /// <summary>患者を ID で読み込む。</summary>
    [RelayCommand]
    public async Task LoadPatientAsync(Guid patientId)
    {
        this.IsLoading = true;
        this.ErrorMessage = null;
        this.IsNewMode = false;

        try
        {
            var response = await _patientService.GetByIdAsync(new GetPatientRequest { PtId = patientId });
            this.PtId = response.PtId;
            this.PtCode = response.PtCode;
            this.KarteCode = response.KarteCode;
            this.PtName = response.PtName;
            this.PtNameKatakana = response.PtNameKatakana;
            this.PtMaidenName = response.PtMaidenName;
            this.PtAliasName = response.PtAliasName;
            this.BirthDate = response.BirthDate;
            this.SexCode = response.SexCode;
            this.PtMemo = response.PtMemo;
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

    /// <summary>患者を保存する（新規作成または更新）。</summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        this.IsLoading = true;
        this.ErrorMessage = null;
        this.StatusMessage = null;

        try
        {
            if (this.IsNewMode)
            {
                var response = await _patientService.CreateAsync(new CreatePatientRequest
                {
                    FacilityId = Guid.Empty,
                    PrimaryOrgId = Guid.Empty,
                    PtCode = this.PtCode,
                    KarteCode = this.KarteCode,
                    PtName = this.PtName,
                    PtNameKatakana = this.PtNameKatakana,
                    PtMaidenName = this.PtMaidenName,
                    PtAliasName = this.PtAliasName,
                    BirthDate = this.BirthDate,
                    SexCode = this.SexCode,
                    PtMemo = this.PtMemo,
                });

                this.PtId = response.PtId;
                this.IsNewMode = false;
                this.StatusMessage = "新規作成しました。";
            }
            else
            {
                await _patientService.UpdateAsync(new UpdatePatientRequest
                {
                    PtId = this.PtId,
                    PtCode = this.PtCode,
                    KarteCode = this.KarteCode,
                    PtName = this.PtName,
                    PtNameKatakana = this.PtNameKatakana,
                    PtMaidenName = this.PtMaidenName,
                    PtAliasName = this.PtAliasName,
                    BirthDate = this.BirthDate,
                    SexCode = this.SexCode,
                    PtMemo = this.PtMemo,
                });

                this.StatusMessage = "保存しました。";
            }
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
