using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// AppointmentSchedule エンティティ設定
/// </summary>
public class AppointmentScheduleConfiguration : IEntityTypeConfiguration<AppointmentSchedule>
{
    public void Configure(EntityTypeBuilder<AppointmentSchedule> entity)
    {
        entity.ToTable("appointment_schedules");
        entity.HasKey(e => e.ApptScheduleId);

        entity.Property(e => e.ApptScheduleId).HasColumnName("appt_schedule_id");
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(false);
        entity.Property(e => e.ActiveFrom).HasColumnName("active_from").HasDefaultValueSql("CURRENT_DATE");
        entity.Property(e => e.ActiveTo).HasColumnName("active_to").HasDefaultValueSql("'9999-12-31'::date");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentResource)
            .WithMany(ar => ar.AppointmentSchedules)
            .HasForeignKey(e => e.ApptResId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.AppointmentScheduleSlots)
            .WithOne(s => s.AppointmentSchedule)
            .HasForeignKey(s => s.ApptScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.AppointmentScheduleOverrides)
            .WithOne(o => o.AppointmentSchedule)
            .HasForeignKey(o => o.ApptScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.ApptResId);
    }
}

/// <summary>
/// AppointmentScheduleSlot エンティティ設定
/// </summary>
public class AppointmentScheduleSlotConfiguration : IEntityTypeConfiguration<AppointmentScheduleSlot>
{
    public void Configure(EntityTypeBuilder<AppointmentScheduleSlot> entity)
    {
        entity.ToTable("appointment_schedule_slots");
        entity.HasKey(e => e.ApptScheduleSlotId);

        entity.Property(e => e.ApptScheduleSlotId).HasColumnName("appt_schedule_slot_id");
        entity.Property(e => e.ApptScheduleId).HasColumnName("appt_schedule_id");

        // CRITICAL: Handle INTEGER[] array type for PostgreSQL
        entity.Property(e => e.DaysOfWeek)
            .HasColumnName("days_of_week")
            .IsRequired();

        entity.Property(e => e.SlotStartMin).HasColumnName("slot_start_min").HasDefaultValue(540);
        entity.Property(e => e.SlotEndMin).HasColumnName("slot_end_min").HasDefaultValue(570);
        entity.Property(e => e.SlotCap).HasColumnName("slot_cap").HasDefaultValue(0);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentSchedule)
            .WithMany(s => s.AppointmentScheduleSlots)
            .HasForeignKey(e => e.ApptScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.CapOverrides)
            .WithOne(co => co.AppointmentScheduleSlot)
            .HasForeignKey(co => co.ApptScheduleSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.UpdatedAt);
    }
}

/// <summary>
/// AppointmentScheduleOverride エンティティ設定
/// </summary>
public class AppointmentScheduleOverrideConfiguration : IEntityTypeConfiguration<AppointmentScheduleOverride>
{
    public void Configure(EntityTypeBuilder<AppointmentScheduleOverride> entity)
    {
        entity.ToTable("appointment_schedule_overrides");
        entity.HasKey(e => e.ApptScheduleOverrideId);

        entity.Property(e => e.ApptScheduleOverrideId).HasColumnName("appt_schedule_override_id");
        entity.Property(e => e.ApptScheduleId).HasColumnName("appt_schedule_id");
        entity.Property(e => e.ApptDate).HasColumnName("appt_date").HasDefaultValueSql("CURRENT_DATE");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentSchedule)
            .WithMany(s => s.AppointmentScheduleOverrides)
            .HasForeignKey(e => e.ApptScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasMany(e => e.AppointmentScheduleSlotOverrides)
            .WithOne(so => so.AppointmentScheduleOverride)
            .HasForeignKey(so => so.ApptScheduleOverrideId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.ApptDate, e.ApptScheduleId })
            .IsUnique()
            .HasFilter("is_deleted = false");
    }
}

/// <summary>
/// AppointmentScheduleSlotOverride エンティティ設定
/// </summary>
public class AppointmentScheduleSlotOverrideConfiguration : IEntityTypeConfiguration<AppointmentScheduleSlotOverride>
{
    public void Configure(EntityTypeBuilder<AppointmentScheduleSlotOverride> entity)
    {
        entity.ToTable("appointment_schedule_slot_overrides");
        entity.HasKey(e => e.ApptScheduleSlotOverrideId);

        entity.Property(e => e.ApptScheduleSlotOverrideId).HasColumnName("appt_schedule_slot_override_id");
        entity.Property(e => e.ApptScheduleOverrideId).HasColumnName("appt_schedule_override_id");

        entity.Property(e => e.SlotStartMin).HasColumnName("slot_start_min").HasDefaultValue(540);
        entity.Property(e => e.SlotEndMin).HasColumnName("slot_end_min").HasDefaultValue(570);
        entity.Property(e => e.SlotCap).HasColumnName("slot_cap").HasDefaultValue(0);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentScheduleOverride)
            .WithMany(so => so.AppointmentScheduleSlotOverrides)
            .HasForeignKey(e => e.ApptScheduleOverrideId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.UpdatedAt);
    }
}

/// <summary>
/// AppointmentScheduleSlotCapOverride エンティティ設定
/// </summary>
public class AppointmentScheduleSlotCapOverrideConfiguration : IEntityTypeConfiguration<AppointmentScheduleSlotCapOverride>
{
    public void Configure(EntityTypeBuilder<AppointmentScheduleSlotCapOverride> entity)
    {
        entity.ToTable("appointment_schedule_slot_cap_overrides");
        entity.HasKey(e => e.ApptScheduleSlotCapOverrideId);

        entity.Property(e => e.ApptScheduleSlotCapOverrideId).HasColumnName("appt_schedule_slot_cap_override_id");
        entity.Property(e => e.ApptScheduleSlotId).HasColumnName("appt_schedule_slot_id");
        entity.Property(e => e.ApptDate).HasColumnName("appt_date").HasDefaultValueSql("CURRENT_DATE");
        entity.Property(e => e.SlotCap).HasColumnName("slot_cap").HasDefaultValue(0);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentScheduleSlot)
            .WithMany(s => s.CapOverrides)
            .HasForeignKey(e => e.ApptScheduleSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.ApptDate, e.ApptScheduleSlotId })
            .IsUnique()
            .HasFilter("is_deleted = false");
    }
}
