using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// Tenant エンティティ設定
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> entity)
    {
        entity.ToTable("tenants");
        entity.HasKey(e => e.TenantId);
        entity.Property(e => e.TenantId).HasColumnName("tenant_id");
        entity.Property(e => e.TenantName).HasColumnName("tenant_name").HasMaxLength(100);
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
        entity.Property(e => e.ActiveTo).HasColumnName("active_to");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// Facility エンティティ設定
/// </summary>
public class FacilityConfiguration : IEntityTypeConfiguration<Facility>
{
    public void Configure(EntityTypeBuilder<Facility> entity)
    {
        entity.ToTable("facilities");
        entity.HasKey(e => e.FacilityId);
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.TenantId).HasColumnName("tenant_id");
        entity.Property(e => e.MedicalInstitutionCode).HasColumnName("medical_institution_code").HasMaxLength(10);
        entity.Property(e => e.FacilityName).HasColumnName("facility_name").HasMaxLength(100);
        entity.Property(e => e.FacilityNameDisplay).HasColumnName("facility_name_display").HasMaxLength(100);
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
        entity.Property(e => e.ActiveTo).HasColumnName("active_to");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Tenant)
            .WithMany(t => t.Facilities)
            .HasForeignKey(e => e.TenantId);
    }
}

/// <summary>
/// FacilityUser エンティティ設定
/// </summary>
public class FacilityUserConfiguration : IEntityTypeConfiguration<FacilityUser>
{
    public void Configure(EntityTypeBuilder<FacilityUser> entity)
    {
        entity.ToTable("facility_users");
        entity.HasKey(e => e.FacilityUserId);
        entity.Property(e => e.FacilityUserId).HasColumnName("facility_user_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.FacilityUserSeq).HasColumnName("facility_user_seq");
        entity.Property(e => e.IsFacilityAdmin).HasColumnName("is_facility_admin");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.FacilityUsers)
            .HasForeignKey(e => e.FacilityId);
        entity.HasOne(e => e.User)
            .WithMany(u => u.FacilityUsers)
            .HasForeignKey(e => e.UserId);

        entity.HasIndex(e => new { e.FacilityId, e.UserId })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
    }
}

/// <summary>
/// FacilityBusinessHours エンティティ設定
/// </summary>
public class FacilityBusinessHoursConfiguration : IEntityTypeConfiguration<FacilityBusinessHours>
{
    public void Configure(EntityTypeBuilder<FacilityBusinessHours> entity)
    {
        entity.ToTable("facility_business_hours");
        entity.HasKey(e => e.FacilityBusinessHoursId);
        entity.Property(e => e.FacilityBusinessHoursId).HasColumnName("facility_business_hours_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.BusinessHours).HasColumnName("business_hours").HasColumnType("jsonb");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
        entity.Property(e => e.ActiveTo).HasColumnName("active_to");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.FacilityBusinessHours)
            .HasForeignKey(e => e.FacilityId);
    }
}

/// <summary>
/// Floor エンティティ設定
/// </summary>
public class FloorConfiguration : IEntityTypeConfiguration<Floor>
{
    public void Configure(EntityTypeBuilder<Floor> entity)
    {
        entity.ToTable("floors");
        entity.HasKey(e => e.FloorId);
        entity.Property(e => e.FloorId).HasColumnName("floor_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.FloorCode).HasColumnName("floor_code");
        entity.Property(e => e.FloorName).HasColumnName("floor_name");
        entity.Property(e => e.FloorDesc).HasColumnName("floor_desc");
        entity.Property(e => e.FloorSeq).HasColumnName("floor_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.Floors)
            .HasForeignKey(e => e.FacilityId);

        entity.HasIndex(e => new { e.FacilityId, e.FloorCode })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
    }
}
