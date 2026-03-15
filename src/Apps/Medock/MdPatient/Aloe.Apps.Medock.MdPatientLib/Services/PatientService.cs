// <copyright file="PatientService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Contracts.Services;
using Aloe.Apps.Medock.MdPatientLib.Data;
using Aloe.Apps.Medock.MdPatientLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdPatientLib.Services;

/// <summary>
/// 患者 CRUD サービスの実装。
/// </summary>
public class PatientService : IPatientService
{
    private readonly MdPatientDbContext _db;

    /// <summary>
    /// <see cref="PatientService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public PatientService(MdPatientDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public async Task<SearchPatientsResponse> SearchAsync(SearchPatientsRequest request)
    {
        var query = _db.Patients.AsQueryable();

        if (request.FacilityId.HasValue)
        {
            query = query.Where(p => p.FacilityId == request.FacilityId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var keyword = request.Keyword.Trim();
            query = query.Where(p =>
                p.PtName.Contains(keyword) ||
                p.PtNameKatakana.Contains(keyword) ||
                p.PtNameKatakanaCompat.Contains(keyword) ||
                p.PtCode.Contains(keyword) ||
                (p.KarteCode != null && p.KarteCode.Contains(keyword)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.PtCode)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientSummary
            {
                PtId = p.PtId,
                PtCode = p.PtCode,
                PtName = p.PtName,
                PtNameKatakana = p.PtNameKatakana,
                BirthDate = p.BirthDate.ToString("yyyy-MM-dd"),
                SexCode = p.SexCode,
            })
            .ToListAsync();

        return new SearchPatientsResponse
        {
            Items = items,
            TotalCount = totalCount,
        };
    }

    /// <inheritdoc/>
    public async Task<PatientDetailResponse?> GetByIdAsync(Guid ptId)
    {
        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.PtId == ptId);
        if (patient == null)
        {
            return null;
        }

        return ToDetailResponse(patient);
    }

    /// <inheritdoc/>
    public async Task<PatientDetailResponse> CreateAsync(CreatePatientRequest request)
    {
        var now = DateTime.UtcNow;
        var patient = new Patient
        {
            PtId = Guid.NewGuid(),
            CanonicalPtId = Guid.NewGuid(),
            FacilityId = request.FacilityId,
            PrimaryOrgId = request.PrimaryOrgId,
            PtCode = request.PtCode,
            KarteCode = request.KarteCode,
            PtName = request.PtName,
            PtNameCompat = request.PtName,
            PtNameKatakana = request.PtNameKatakana,
            PtNameKatakanaCompat = request.PtNameKatakana,
            PtMaidenName = request.PtMaidenName,
            PtAliasName = request.PtAliasName,
            BirthDate = DateOnly.Parse(request.BirthDate),
            SexCode = request.SexCode,
            PtMemo = request.PtMemo,
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();

        return ToDetailResponse(patient);
    }

    /// <inheritdoc/>
    public async Task<PatientDetailResponse?> UpdateAsync(UpdatePatientRequest request)
    {
        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.PtId == request.PtId);
        if (patient == null)
        {
            return null;
        }

        patient.PtCode = request.PtCode;
        patient.KarteCode = request.KarteCode;
        patient.PtName = request.PtName;
        patient.PtNameCompat = request.PtName;
        patient.PtNameKatakana = request.PtNameKatakana;
        patient.PtNameKatakanaCompat = request.PtNameKatakana;
        patient.PtMaidenName = request.PtMaidenName;
        patient.PtAliasName = request.PtAliasName;
        patient.BirthDate = DateOnly.Parse(request.BirthDate);
        patient.SexCode = request.SexCode;
        patient.PtMemo = request.PtMemo;
        patient.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ToDetailResponse(patient);
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(Guid ptId)
    {
        var patient = await _db.Patients
            .FirstOrDefaultAsync(p => p.PtId == ptId);
        if (patient == null)
        {
            return false;
        }

        patient.IsDeleted = true;
        patient.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    private static PatientDetailResponse ToDetailResponse(Patient patient)
    {
        return new PatientDetailResponse
        {
            PtId = patient.PtId,
            CanonicalPtId = patient.CanonicalPtId,
            FacilityId = patient.FacilityId,
            PrimaryOrgId = patient.PrimaryOrgId,
            PtCode = patient.PtCode,
            KarteCode = patient.KarteCode,
            PtName = patient.PtName,
            PtNameKatakana = patient.PtNameKatakana,
            PtMaidenName = patient.PtMaidenName,
            PtAliasName = patient.PtAliasName,
            BirthDate = patient.BirthDate.ToString("yyyy-MM-dd"),
            SexCode = patient.SexCode,
            PtMemo = patient.PtMemo,
            IsDeleted = patient.IsDeleted,
        };
    }
}
