using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// Plan エンティティ設定
/// </summary>
public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> entity)
    {
        entity.ToTable("plans");
        entity.HasKey(e => e.PlanId);
        entity.Property(e => e.PlanId).HasColumnName("plan_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.PlanCode).HasColumnName("plan_code").HasMaxLength(20);
        entity.Property(e => e.PlanName).HasColumnName("plan_name").HasMaxLength(100);
        entity.Property(e => e.PlanShortName).HasColumnName("plan_short_name").HasMaxLength(100);
        entity.Property(e => e.PlanAbbrName).HasColumnName("plan_abbr_name").HasMaxLength(100);
        entity.Property(e => e.PlanDesc).HasColumnName("plan_desc").HasMaxLength(1000);
        entity.Property(e => e.PlanKindCode).HasColumnName("plan_kind_code");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
        entity.Property(e => e.ActiveTo).HasColumnName("active_to");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany()
            .HasForeignKey(e => e.FacilityId);
    }
}

/// <summary>
/// PlanCondition エンティティ設定
/// </summary>
public class PlanConditionConfiguration : IEntityTypeConfiguration<PlanCondition>
{
    public void Configure(EntityTypeBuilder<PlanCondition> entity)
    {
        entity.ToTable("plan_conditions");
        entity.HasKey(e => e.PlanCondId);
        entity.Property(e => e.PlanCondId).HasColumnName("plan_cond_id");
        entity.Property(e => e.ConditionName).HasColumnName("condition_name").HasMaxLength(100);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// PlanConditionMember エンティティ設定
/// </summary>
public class PlanConditionMemberConfiguration : IEntityTypeConfiguration<PlanConditionMember>
{
    public void Configure(EntityTypeBuilder<PlanConditionMember> entity)
    {
        entity.ToTable("plan_condition_members");
        entity.HasKey(e => e.PlanCondMemberId);
        entity.Property(e => e.PlanCondMemberId).HasColumnName("plan_cond_member_id");
        entity.Property(e => e.PlanId).HasColumnName("plan_id");
        entity.Property(e => e.PlanCondId).HasColumnName("plan_cond_id");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Plan)
            .WithMany(p => p.PlanConditionMembers)
            .HasForeignKey(e => e.PlanId);
        entity.HasOne(e => e.PlanCondition)
            .WithMany(pc => pc.PlanConditionMembers)
            .HasForeignKey(e => e.PlanCondId);

        entity.HasIndex(e => e.PlanId);
        entity.HasIndex(e => e.PlanCondId);
    }
}

/// <summary>
/// PlanOption エンティティ設定
/// </summary>
public class PlanOptionConfiguration : IEntityTypeConfiguration<PlanOption>
{
    public void Configure(EntityTypeBuilder<PlanOption> entity)
    {
        entity.ToTable("plan_options");
        entity.HasKey(e => e.PlanOptionId);
        entity.Property(e => e.PlanOptionId).HasColumnName("plan_option_id");
        entity.Property(e => e.PlanId).HasColumnName("plan_id");
        entity.Property(e => e.OptionPlanId).HasColumnName("option_plan_id");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Plan)
            .WithMany(p => p.PlanOptions)
            .HasForeignKey(e => e.PlanId);
        entity.HasOne(e => e.OptionPlan)
            .WithMany()
            .HasForeignKey(e => e.OptionPlanId);

        entity.HasIndex(e => e.PlanId);
    }
}

/// <summary>
/// PlanResourceRequirement エンティティ設定
/// </summary>
public class PlanResourceRequirementConfiguration : IEntityTypeConfiguration<PlanResourceRequirement>
{
    public void Configure(EntityTypeBuilder<PlanResourceRequirement> entity)
    {
        entity.ToTable("plan_resource_requirements");
        entity.HasKey(e => e.PlanResReqId);
        entity.Property(e => e.PlanResReqId).HasColumnName("plan_res_req_id");
        entity.Property(e => e.PlanId).HasColumnName("plan_id");
        entity.Property(e => e.ApptResId).HasColumnName("appt_res_id");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Plan)
            .WithMany(p => p.PlanResourceRequirements)
            .HasForeignKey(e => e.PlanId);
        entity.HasOne(e => e.AppointmentResource)
            .WithMany()
            .HasForeignKey(e => e.ApptResId);

        entity.HasIndex(e => e.PlanId);
        entity.HasIndex(e => e.ApptResId);
    }
}

