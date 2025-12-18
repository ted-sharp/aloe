using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// Policy エンティティ設定
/// </summary>
public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> entity)
    {
        entity.ToTable("policies");
        entity.HasKey(e => e.PolicyCode);
        entity.Property(e => e.PolicyCode).HasColumnName("policy_code").HasMaxLength(100);
        entity.Property(e => e.PolicyName).HasColumnName("policy_name").HasMaxLength(100);
        entity.Property(e => e.PolicyDesc).HasColumnName("policy_desc").HasMaxLength(1000);
        entity.Property(e => e.DataType).HasColumnName("data_type").HasMaxLength(10);
        entity.Property(e => e.PolicyValue).HasColumnName("policy_value").HasMaxLength(10);
        entity.Property(e => e.PolicySeq).HasColumnName("policy_seq");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// Preference エンティティ設定
/// </summary>
public class PreferenceConfiguration : IEntityTypeConfiguration<Preference>
{
    public void Configure(EntityTypeBuilder<Preference> entity)
    {
        entity.ToTable("preferences");
        entity.HasKey(e => e.PreferenceCode);
        entity.Property(e => e.PreferenceCode).HasColumnName("preference_code").HasMaxLength(100);
        entity.Property(e => e.PreferenceName).HasColumnName("preference_name").HasMaxLength(100);
        entity.Property(e => e.PreferenceDesc).HasColumnName("preference_desc").HasMaxLength(1000);
        entity.Property(e => e.DataType).HasColumnName("data_type").HasMaxLength(10);
        entity.Property(e => e.PreferenceValue).HasColumnName("preference_value").HasMaxLength(10);
        entity.Property(e => e.PreferenceSeq).HasColumnName("preference_seq");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// FacilityPolicy エンティティ設定
/// </summary>
public class FacilityPolicyConfiguration : IEntityTypeConfiguration<FacilityPolicy>
{
    public void Configure(EntityTypeBuilder<FacilityPolicy> entity)
    {
        entity.ToTable("facility_policies");
        entity.HasKey(e => e.FacilityPolicyId);
        entity.Property(e => e.FacilityPolicyId).HasColumnName("facility_policy_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.PolicyCode).HasColumnName("policy_code").HasMaxLength(100);
        entity.Property(e => e.PolicyValue).HasColumnName("policy_value").HasMaxLength(10);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.FacilityPolicies)
            .HasForeignKey(e => e.FacilityId);
        entity.HasOne(e => e.Policy)
            .WithMany(p => p.FacilityPolicies)
            .HasForeignKey(e => e.PolicyCode);

        entity.HasIndex(e => new { e.FacilityId, e.PolicyCode })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
    }
}

/// <summary>
/// UserPreference エンティティ設定
/// </summary>
public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> entity)
    {
        entity.ToTable("user_preferences");
        entity.HasKey(e => e.UserPreferenceId);
        entity.Property(e => e.UserPreferenceId).HasColumnName("user_preference_id");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.PreferenceCode).HasColumnName("preference_code").HasMaxLength(100);
        entity.Property(e => e.PreferenceValue).HasColumnName("preference_value").HasMaxLength(10);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.User)
            .WithMany(u => u.UserPreferences)
            .HasForeignKey(e => e.UserId);
        entity.HasOne(e => e.Preference)
            .WithMany(p => p.UserPreferences)
            .HasForeignKey(e => e.PreferenceCode);

        entity.HasIndex(e => new { e.UserId, e.PreferenceCode })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
    }
}

