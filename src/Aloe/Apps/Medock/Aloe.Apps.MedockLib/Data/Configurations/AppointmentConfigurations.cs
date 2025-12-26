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
        entity.Property(e => e.ApptStartTime).HasColumnName("appt_start_time").HasColumnType("time");
        entity.Property(e => e.ApptDurationMin).HasColumnName("appt_duration_min");
        entity.Property(e => e.ApptStatusCode).HasColumnName("appt_status_code").HasDefaultValue(0);
        entity.Property(e => e.ApptMemo).HasColumnName("appt_memo").HasMaxLength(1000).HasDefaultValue("");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
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

        entity.HasIndex(e => new { e.FloorId, e.ApptDate });
        entity.HasIndex(e => e.OrgId);
        entity.HasIndex(e => e.PtId);
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
        entity.Property(e => e.ApptDate).HasColumnName("appt_date");
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.ApptCap).HasColumnName("appt_cap").HasDefaultValue(0);
        entity.Property(e => e.ApptCount).HasColumnName("appt_count").HasDefaultValue(0);
        entity.Property(e => e.ApptAvailable)
            .HasColumnName("appt_available")
            .HasComputedColumnSql("appt_cap - appt_count", stored: true);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentResource)
            .WithMany(ar => ar.AppointmentStats)
            .HasForeignKey(e => e.ApptResId);

        entity.HasMany(e => e.AppointmentStatSlots)
            .WithOne(s => s.AppointmentStats)
            .HasForeignKey(s => s.ApptStatId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => new { e.ApptDate, e.ApptResId })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
    }
}

/// <summary>
/// AppointmentStatSlots エンティティ設定
/// </summary>
public class AppointmentStatSlotsConfiguration : IEntityTypeConfiguration<AppointmentStatSlots>
{
    public void Configure(EntityTypeBuilder<AppointmentStatSlots> entity)
    {
        entity.ToTable("appointment_stat_slots");
        entity.HasKey(e => e.ApptStatSlotId);
        entity.Property(e => e.ApptStatSlotId).HasColumnName("appt_stat_slot_id");
        entity.Property(e => e.ApptStatId).HasColumnName("appt_stat_id");
        entity.Property(e => e.ApptDate).HasColumnName("appt_date");
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.SlotStart).HasColumnName("slot_start").HasDefaultValue(0);
        entity.Property(e => e.SlotEnd).HasColumnName("slot_end").HasDefaultValue(0);
        entity.Property(e => e.SlotCap).HasColumnName("slot_cap").HasDefaultValue(0);
        entity.Property(e => e.SlotCount).HasColumnName("slot_count").HasDefaultValue(0);
        entity.Property(e => e.SlotAvailable)
            .HasColumnName("slot_available")
            .HasComputedColumnSql("slot_cap - slot_count", stored: true);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentStats)
            .WithMany(s => s.AppointmentStatSlots)
            .HasForeignKey(e => e.ApptStatId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(e => e.AppointmentResource)
            .WithMany()
            .HasForeignKey(e => e.ApptResId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasIndex(e => new { e.ApptStatId, e.ApptDate, e.ApptResId });
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
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.ApptSlots).HasColumnName("appt_slots").HasColumnType("jsonb").HasDefaultValue("{}");
        entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(false);
        entity.Property(e => e.ActiveFrom).HasColumnName("active_from").HasDefaultValueSql("CURRENT_DATE");
        entity.Property(e => e.ActiveTo).HasColumnName("active_to").HasDefaultValueSql("'9999-12-31'::date");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentResource)
            .WithMany(ar => ar.AppointmentSlots)
            .HasForeignKey(e => e.ApptResId);

        entity.HasIndex(e => e.ApptResId);
    }
}

/// <summary>
/// AppointmentResource エンティティ設定
/// </summary>
/// <remarks>
/// Mainリソース（appt_res_type_code = 1）は、各フロアに1つだけという制約が設定されています。
/// この制約は、floor_idに対するユニークインデックス（WHERE is_deleted = FALSE AND appt_res_type_code = 1）により実現されています。
/// </remarks>
public class AppointmentResourceConfiguration : IEntityTypeConfiguration<AppointmentResource>
{
    public void Configure(EntityTypeBuilder<AppointmentResource> entity)
    {
        entity.ToTable("appointment_resources");
        entity.HasKey(e => e.ApptResId);
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.FloorId).HasColumnName("floor_id");
        // appt_res_type_code: 1=Main(棒グラフ), 2=Equipment(折れ線グラフ), 3=Environment(背景積み立て), 99=Others(非表示)
        entity.Property(e => e.ApptResTypeCode).HasColumnName("appt_res_type_code").HasDefaultValue(0);
        entity.Property(e => e.ApptResName).HasColumnName("appt_res_name").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.ApptResDesc).HasColumnName("appt_res_desc").HasMaxLength(1000).HasDefaultValue("");
        entity.Property(e => e.ApptResSeq).HasColumnName("appt_res_seq").HasDefaultValue(0);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Floor)
            .WithMany(f => f.AppointmentResources)
            .HasForeignKey(e => e.FloorId);

        // Mainリソース（appt_res_type_code = 1）は、各フロアに1つだけという制約
        entity.HasIndex(e => e.FloorId)
            .IsUnique()
            .HasFilter("[is_deleted] = 0 AND [appt_res_type_code] = 1");
    }
}

/// <summary>
/// AppointmentSlotOverride エンティティ設定
/// </summary>
public class AppointmentSlotOverrideConfiguration : IEntityTypeConfiguration<AppointmentSlotOverride>
{
    public void Configure(EntityTypeBuilder<AppointmentSlotOverride> entity)
    {
        entity.ToTable("appointment_slot_overrides");
        entity.HasKey(e => e.ApptSlotOverrideId);
        entity.Property(e => e.ApptSlotOverrideId).HasColumnName("appt_slot_override_id");
        entity.Property(e => e.ApptDate).HasColumnName("appt_date").HasDefaultValueSql("CURRENT_DATE");
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.ApptSlots).HasColumnName("appt_slots").HasColumnType("jsonb").HasDefaultValue("{}");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentResource)
            .WithMany(ar => ar.AppointmentSlotOverrides)
            .HasForeignKey(e => e.ApptResId);

        entity.HasIndex(e => new { e.ApptDate, e.ApptResId })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
    }
}

/// <summary>
/// AppointmentResourceReservation エンティティ設定
/// </summary>
public class AppointmentResourceReservationConfiguration : IEntityTypeConfiguration<AppointmentResourceAssignment>
{
    public void Configure(EntityTypeBuilder<AppointmentResourceAssignment> entity)
    {
        entity.ToTable("appointment_resource_assignments");
        entity.HasKey(e => e.ApptResAssignId);
        entity.Property(e => e.ApptResAssignId).HasColumnName("appt_res_assign_id");
        entity.Property(e => e.ApptId).HasColumnName("appt_id");
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.ApptStartTime).HasColumnName("appt_start_time").HasColumnType("time");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Appointment)
            .WithMany(a => a.AppointmentResourceReservations)
            .HasForeignKey(e => e.ApptId);
        entity.HasOne(e => e.AppointmentResource)
            .WithMany(ar => ar.AppointmentResourceReservations)
            .HasForeignKey(e => e.ApptResId);

        entity.HasIndex(e => e.ApptId);
        entity.HasIndex(e => e.ApptResId);
    }
}
