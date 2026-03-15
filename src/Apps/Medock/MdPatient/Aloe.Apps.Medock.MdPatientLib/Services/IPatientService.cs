// <copyright file="IPatientService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;

namespace Aloe.Apps.Medock.MdPatientLib.Services;

/// <summary>
/// 患者ビジネスロジックのインターフェース。
/// </summary>
public interface IPatientService
{
    /// <summary>患者を検索する。</summary>
    Task<SearchPatientsResponse> SearchAsync(SearchPatientsRequest request);

    /// <summary>患者を ID で取得する。</summary>
    Task<PatientDetailResponse?> GetByIdAsync(Guid ptId);

    /// <summary>患者を新規作成する。</summary>
    Task<PatientDetailResponse> CreateAsync(CreatePatientRequest request);

    /// <summary>患者を更新する。</summary>
    Task<PatientDetailResponse?> UpdateAsync(UpdatePatientRequest request);

    /// <summary>患者を削除する（論理削除）。</summary>
    Task<bool> DeleteAsync(Guid ptId);
}
