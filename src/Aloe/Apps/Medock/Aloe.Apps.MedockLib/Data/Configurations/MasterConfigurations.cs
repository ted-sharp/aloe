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
