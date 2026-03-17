// <copyright file="MdLauncherDbContext.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdLauncherLib.Data;

/// <summary>
/// MdLauncher 用 DbContext。
/// </summary>
public class MdLauncherDbContext : DbContext
{
    /// <summary>
    /// <see cref="MdLauncherDbContext"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public MdLauncherDbContext(DbContextOptions<MdLauncherDbContext> options)
        : base(options)
    {
    }

    /// <summary>アプリテーブル。</summary>
    public DbSet<AppEntity> Apps => Set<AppEntity>();

    /// <summary>メニューテーブル。</summary>
    public DbSet<MenuEntity> Menus => Set<MenuEntity>();

    /// <summary>ロールメニューテーブル。</summary>
    public DbSet<RoleMenuEntity> RoleMenus => Set<RoleMenuEntity>();

    /// <summary>ユーザーテーブル。</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>施設ユーザーテーブル。</summary>
    public DbSet<FacilityUserEntity> FacilityUsers => Set<FacilityUserEntity>();

    /// <summary>施設ユーザーロールテーブル。</summary>
    public DbSet<FacilityUserRoleEntity> FacilityUserRoles => Set<FacilityUserRoleEntity>();

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppEntity>(entity =>
        {
            entity.HasKey(e => e.AppCode);
            entity.HasQueryFilter(e => !e.IsDeleted && e.IsActive);
        });

        modelBuilder.Entity<MenuEntity>(entity =>
        {
            entity.HasKey(e => e.MenuCode);
            entity.HasQueryFilter(e => !e.IsDeleted && e.IsActive);
        });

        modelBuilder.Entity<RoleMenuEntity>(entity =>
        {
            entity.HasKey(e => e.RoleMenuId);
            entity.HasIndex(e => e.RoleCode);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.UserCode);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<FacilityUserEntity>(entity =>
        {
            entity.HasKey(e => e.FacilityUserId);
            entity.HasIndex(e => e.UserId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        modelBuilder.Entity<FacilityUserRoleEntity>(entity =>
        {
            entity.HasKey(e => e.FacilityUserRoleId);
            entity.HasIndex(e => e.FacilityUserId);
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}
