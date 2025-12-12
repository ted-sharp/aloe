using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// Equipment エンティティ設定
/// </summary>
public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> entity)
    {
        entity.ToTable("equipments");
        entity.HasKey(e => e.EquipId);
        entity.Property(e => e.EquipId).HasColumnName("equip_id");
        entity.Property(e => e.FloorId).HasColumnName("floor_id");
        entity.Property(e => e.EquipName).HasColumnName("equip_name").HasMaxLength(100);
        entity.Property(e => e.EquipDesc).HasColumnName("equip_desc").HasMaxLength(1000);
        entity.Property(e => e.EquipSeq).HasColumnName("equip_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Floor)
            .WithMany(f => f.Equipments)
            .HasForeignKey(e => e.FloorId);
    }
}

/// <summary>
/// EquipmentAppointment エンティティ設定
/// </summary>
public class EquipmentAppointmentConfiguration : IEntityTypeConfiguration<EquipmentAppointment>
{
    public void Configure(EntityTypeBuilder<EquipmentAppointment> entity)
    {
        entity.ToTable("equipment_appointments");
        entity.HasKey(e => e.EquipApptId);
        entity.Property(e => e.EquipApptId).HasColumnName("equip_appt_id");
        entity.Property(e => e.EquipId).HasColumnName("equip_id");
        entity.Property(e => e.OrgId).HasColumnName("org_id");
        entity.Property(e => e.PtId).HasColumnName("pt_id");
        entity.Property(e => e.ApptDate).HasColumnName("appt_date");
        entity.Property(e => e.ApptStartAt).HasColumnName("appt_start_at");
        entity.Property(e => e.ApptEndAt).HasColumnName("appt_end_at");
        entity.Property(e => e.ApptStatusCode).HasColumnName("appt_status_code");
        entity.Property(e => e.ApptMemo).HasColumnName("appt_memo").HasMaxLength(1000);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Equipment)
            .WithMany(eq => eq.EquipmentAppointments)
            .HasForeignKey(e => e.EquipId);
        entity.HasOne(e => e.Organization)
            .WithMany(o => o.EquipmentAppointments)
            .HasForeignKey(e => e.OrgId);
        entity.HasOne(e => e.Patient)
            .WithMany(p => p.EquipmentAppointments)
            .HasForeignKey(e => e.PtId);
    }
}

/// <summary>
/// EquipmentAppointmentStats エンティティ設定
/// </summary>
public class EquipmentAppointmentStatsConfiguration : IEntityTypeConfiguration<EquipmentAppointmentStats>
{
    public void Configure(EntityTypeBuilder<EquipmentAppointmentStats> entity)
    {
        entity.ToTable("equipment_appointment_stats");
        entity.HasKey(e => e.ApptStatId);
        entity.Property(e => e.ApptStatId).HasColumnName("equip_appt_stat_id");
        entity.Property(e => e.EquipId).HasColumnName("equip_id");
        entity.Property(e => e.ApptDate).HasColumnName("appt_date");
        entity.Property(e => e.ApptCount).HasColumnName("appt_count");
        entity.Property(e => e.ApptMax).HasColumnName("appt_max");
        entity.Property(e => e.ApptGraph).HasColumnName("appt_graph").HasColumnType("jsonb");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Equipment)
            .WithMany(eq => eq.EquipmentAppointmentStats)
            .HasForeignKey(e => e.EquipId);
    }
}

/// <summary>
/// EquipmentSlot エンティティ設定
/// </summary>
public class EquipmentSlotConfiguration : IEntityTypeConfiguration<EquipmentSlot>
{
    public void Configure(EntityTypeBuilder<EquipmentSlot> entity)
    {
        entity.ToTable("equipment_slots");
        entity.HasKey(e => e.EquipSlotId);
        entity.Property(e => e.EquipSlotId).HasColumnName("equip_slot_id");
        entity.Property(e => e.EquipId).HasColumnName("equip_id");
        entity.Property(e => e.EquipSlots).HasColumnName("equip_slots").HasColumnType("jsonb");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
        entity.Property(e => e.ActiveTo).HasColumnName("active_to");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Equipment)
            .WithMany(eq => eq.EquipmentSlots)
            .HasForeignKey(e => e.EquipId);
    }
}
