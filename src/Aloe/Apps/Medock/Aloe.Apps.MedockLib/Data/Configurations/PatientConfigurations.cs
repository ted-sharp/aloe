using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// Patient エンティティ設定
/// </summary>
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> entity)
    {
        entity.ToTable("patients");
        entity.HasKey(e => e.PtId);
        entity.Property(e => e.PtId).HasColumnName("pt_id");
        entity.Property(e => e.CanonicalPtId).HasColumnName("canonical_pt_id").HasDefaultValue(Guid.Empty);
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.PrimaryOrgId).HasColumnName("primary_org_id").HasDefaultValue(Guid.Empty);
        entity.Property(e => e.PtCode).HasColumnName("pt_code").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.KarteCode).HasColumnName("karte_code").HasMaxLength(100);
        entity.Property(e => e.PtName).HasColumnName("pt_name").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.PtNameCompat).HasColumnName("pt_name_compat").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.PtNameKatakana).HasColumnName("pt_name_katakana").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.PtNameKatakanaCompat).HasColumnName("pt_name_katakana_compat").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.PtMaidenName).HasColumnName("pt_maiden_name").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.PtAliasName).HasColumnName("pt_alias_name").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.BirthDate).HasColumnName("birth_date").HasDefaultValueSql("CURRENT_DATE");
        entity.Property(e => e.SexCode).HasColumnName("sex_code").HasDefaultValue(0);
        entity.Property(e => e.PtMemo).HasColumnName("pt_memo").HasMaxLength(1000).HasDefaultValue("");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.Patients)
            .HasForeignKey(e => e.FacilityId);

        entity.HasIndex(e => new { e.FacilityId, e.PtCode })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
        entity.HasIndex(e => new { e.FacilityId, e.KarteCode })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
    }
}

/// <summary>
/// Organization エンティティ設定
/// </summary>
public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> entity)
    {
        entity.ToTable("organizations");
        entity.HasKey(e => e.OrgId);
        entity.Property(e => e.OrgId).HasColumnName("org_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.ParentOrgId).HasColumnName("parent_org_id");
        entity.Property(e => e.OrgCode).HasColumnName("org_code").HasMaxLength(13).HasDefaultValue("");
        entity.Property(e => e.OrgName).HasColumnName("org_name").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.OrgNameKatakana).HasColumnName("org_name_katakana").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.OrgNameKatakanaCompat).HasColumnName("org_name_katakana_compat").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.OrgNameDisplay).HasColumnName("org_name_display").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.OrgNamePrint).HasColumnName("org_name_print").HasMaxLength(100).HasDefaultValue("");
        entity.Property(e => e.OrgMemo).HasColumnName("org_memo").HasMaxLength(1000).HasDefaultValue("");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.Organizations)
            .HasForeignKey(e => e.FacilityId);
        entity.HasOne(e => e.ParentOrganization)
            .WithMany()
            .HasForeignKey(e => e.ParentOrgId);

        entity.HasIndex(e => new { e.FacilityId, e.OrgCode })
            .IsUnique()
            .HasFilter("[is_deleted] = 0");
    }
}
