using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// AppointmentResourceGroup エンティティ設定
/// </summary>
public class AppointmentResourceGroupConfiguration : IEntityTypeConfiguration<AppointmentResourceGroup>
{
    public void Configure(EntityTypeBuilder<AppointmentResourceGroup> entity)
    {
        entity.ToTable("appointment_resource_groups");
        entity.HasKey(e => e.ApptResGroupId);
        entity.Property(e => e.ApptResGroupId).HasColumnName("appt_res_group_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.ResGroupCode).HasColumnName("res_group_code").HasMaxLength(20).HasDefaultValue("");
        entity.Property(e => e.ResGroupName).HasColumnName("res_group_name").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.ResGroupDesc).HasColumnName("res_group_desc").HasMaxLength(1000).HasDefaultValue("");
        entity.Property(e => e.ResGroupSeq).HasColumnName("res_group_seq").HasDefaultValue(0);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.AppointmentResourceGroups)
            .HasForeignKey(e => e.FacilityId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.FacilityId);
    }
}

/// <summary>
/// AppointmentResourceGroupMember エンティティ設定
/// </summary>
public class AppointmentResourceGroupMemberConfiguration : IEntityTypeConfiguration<AppointmentResourceGroupMember>
{
    public void Configure(EntityTypeBuilder<AppointmentResourceGroupMember> entity)
    {
        entity.ToTable("appointment_resource_group_members");
        entity.HasKey(e => e.ApptResGroupMemberId);
        entity.Property(e => e.ApptResGroupMemberId).HasColumnName("appt_res_group_member_id");
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.ApptResGroupId).HasColumnName("appt_res_group_id");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.AppointmentResource)
            .WithMany(ar => ar.AppointmentResourceGroupMembers)
            .HasForeignKey(e => e.ApptResId);
        entity.HasOne(e => e.AppointmentResourceGroup)
            .WithMany(arg => arg.AppointmentResourceGroupMembers)
            .HasForeignKey(e => e.ApptResGroupId);

        entity.HasIndex(e => e.ApptResId);
        entity.HasIndex(e => e.ApptResGroupId);
    }
}

