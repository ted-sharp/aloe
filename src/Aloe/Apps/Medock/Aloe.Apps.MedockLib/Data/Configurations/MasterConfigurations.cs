using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// Holiday エンティティ設定
/// </summary>
public class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> entity)
    {
        entity.ToTable("holidays");
        entity.HasKey(e => e.HolidayDate);
        entity.Property(e => e.HolidayDate).HasColumnName("holiday_date").ValueGeneratedNever();
        entity.Property(e => e.HolidayName).HasColumnName("holiday_name").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// FacilityHoliday エンティティ設定
/// </summary>
public class FacilityHolidayConfiguration : IEntityTypeConfiguration<FacilityHoliday>
{
    public void Configure(EntityTypeBuilder<FacilityHoliday> entity)
    {
        entity.ToTable("facility_holidays");
        entity.HasKey(e => e.HolidayId);
        entity.Property(e => e.HolidayId).HasColumnName("holiday_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.HolidayDate).HasColumnName("holiday_date");
        entity.Property(e => e.HolidayName).HasColumnName("holiday_name").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.HolidayDesc).HasColumnName("holiday_desc").HasMaxLength(1000).HasDefaultValue("");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.FacilityHolidays)
            .HasForeignKey(e => e.FacilityId);

        entity.HasIndex(e => new { e.FacilityId, e.HolidayDate })
            .IsUnique()
            .HasFilter("[is_deleted] = false");
    }
}
