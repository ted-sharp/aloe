using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// Appointment エンティティ設定
/// </summary>
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> entity)
    {
        entity.ToTable("appointments");
        entity.HasKey(e => e.ApptId);
        entity.Property(e => e.ApptId).HasColumnName("appt_id");
        entity.Property(e => e.FloorId).HasColumnName("floor_id");
        entity.Property(e => e.OrgId).HasColumnName("org_id");
        entity.Property(e => e.PtId).HasColumnName("pt_id");
        entity.Property(e => e.ApptDate).HasColumnName("appt_date");
        entity.Property(e => e.ApptStartAt).HasColumnName("appt_start_at");
        entity.Property(e => e.ApptEndAt).HasColumnName("appt_end_at");
        entity.Property(e => e.ApptStatusCode).HasColumnName("appt_status_code");
        entity.Property(e => e.ApptMemo).HasColumnName("appt_memo").HasMaxLength(1000);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Floor)
            .WithMany(f => f.Appointments)
            .HasForeignKey(e => e.FloorId);
        entity.HasOne(e => e.Organization)
            .WithMany(o => o.Appointments)
            .HasForeignKey(e => e.OrgId);
        entity.HasOne(e => e.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(e => e.PtId);
    }
}

/// <summary>
/// AppointmentStats エンティティ設定
/// </summary>
public class AppointmentStatsConfiguration : IEntityTypeConfiguration<AppointmentStats>
{
    public void Configure(EntityTypeBuilder<AppointmentStats> entity)
    {
        entity.ToTable("appointment_stats");
        entity.HasKey(e => e.ApptStatId);
        entity.Property(e => e.ApptStatId).HasColumnName("appt_stat_id");
        entity.Property(e => e.FloorId).HasColumnName("floor_id");
        entity.Property(e => e.ApptDate).HasColumnName("appt_date");
        entity.Property(e => e.ApptCount).HasColumnName("appt_count");
        entity.Property(e => e.ApptMax).HasColumnName("appt_max");
        entity.Property(e => e.ApptGraph).HasColumnName("appt_graph").HasColumnType("jsonb");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Floor)
            .WithMany(f => f.AppointmentStats)
            .HasForeignKey(e => e.FloorId);
    }
}

/// <summary>
/// AppointmentSlot エンティティ設定
/// </summary>
public class AppointmentSlotConfiguration : IEntityTypeConfiguration<AppointmentSlot>
{
    public void Configure(EntityTypeBuilder<AppointmentSlot> entity)
    {
        entity.ToTable("appointment_slots");
        entity.HasKey(e => e.ApptSlotId);
        entity.Property(e => e.ApptSlotId).HasColumnName("appt_slot_id");
        entity.Property(e => e.FloorId).HasColumnName("floor_id");
        entity.Property(e => e.ApptSlots).HasColumnName("appt_slots").HasColumnType("jsonb");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
        entity.Property(e => e.ActiveTo).HasColumnName("active_to");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Floor)
            .WithMany(f => f.AppointmentSlots)
            .HasForeignKey(e => e.FloorId);
    }
}
