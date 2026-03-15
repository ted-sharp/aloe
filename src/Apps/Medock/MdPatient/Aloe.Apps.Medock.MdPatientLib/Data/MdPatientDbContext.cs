// <copyright file="MdPatientDbContext.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdPatientLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdPatientLib.Data;

/// <summary>
/// MdPatient 用 DbContext。
/// </summary>
public class MdPatientDbContext : DbContext
{
    /// <summary>
    /// <see cref="MdPatientDbContext"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public MdPatientDbContext(DbContextOptions<MdPatientDbContext> options)
        : base(options)
    {
    }

    /// <summary>患者テーブル。</summary>
    public DbSet<Patient> Patients => Set<Patient>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.PtId);
            entity.HasIndex(e => e.FacilityId);
            entity.HasIndex(e => e.PtCode);
            entity.HasIndex(e => e.PtNameKatakanaCompat);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
