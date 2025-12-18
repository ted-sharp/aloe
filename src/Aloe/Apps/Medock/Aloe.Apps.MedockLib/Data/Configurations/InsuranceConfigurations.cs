using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// InsuranceProvider エンティティ設定
/// </summary>
public class InsuranceProviderConfiguration : IEntityTypeConfiguration<InsuranceProvider>
{
    public void Configure(EntityTypeBuilder<InsuranceProvider> entity)
    {
        entity.ToTable("insurance_providers");
        entity.HasKey(e => e.InsurerId);
        entity.Property(e => e.InsurerId).HasColumnName("insurer_id");
        entity.Property(e => e.InsurerTypeCode).HasColumnName("insurer_type_code");
        entity.Property(e => e.InsurerCode).HasColumnName("insurer_code");
        entity.Property(e => e.InsurerName).HasColumnName("insurer_name");
        entity.Property(e => e.InsurerShortName).HasColumnName("insurer_short_name");
        entity.Property(e => e.InsurerDesc).HasColumnName("insurer_desc");
        entity.Property(e => e.InsurerSeq).HasColumnName("insurer_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// OrganizationInsurance エンティティ設定
/// </summary>
public class OrganizationInsuranceConfiguration : IEntityTypeConfiguration<OrganizationInsurance>
{
    public void Configure(EntityTypeBuilder<OrganizationInsurance> entity)
    {
        entity.ToTable("organization_insurances");
        entity.HasKey(e => e.OrgInsuranceId);
        entity.Property(e => e.OrgInsuranceId).HasColumnName("org_insurance_id");
        entity.Property(e => e.OrgId).HasColumnName("org_id");
        entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
        entity.Property(e => e.InsurerId).HasColumnName("insurer_id");
        entity.Property(e => e.InsurerTypeCode).HasColumnName("insurer_type_code");
        entity.Property(e => e.InsurerCode).HasColumnName("insurer_code");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.DeactivatedOn).HasColumnName("deactivated_on");
        entity.Property(e => e.OrgInsuranceMemo).HasColumnName("org_insurance_memo");
        entity.Property(e => e.OrgInsuranceSeq).HasColumnName("org_insurance_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Organization)
            .WithMany(o => o.OrganizationInsurances)
            .HasForeignKey(e => e.OrgId);
        entity.HasOne(e => e.InsuranceProvider)
            .WithMany()
            .HasForeignKey(e => e.InsurerId);

        entity.HasIndex(e => e.OrgId);
        entity.HasIndex(e => e.InsurerId);
    }
}

/// <summary>
/// PatientInsuranceCard エンティティ設定
/// </summary>
public class PatientInsuranceCardConfiguration : IEntityTypeConfiguration<PatientInsuranceCard>
{
    public void Configure(EntityTypeBuilder<PatientInsuranceCard> entity)
    {
        entity.ToTable("patient_insurance_cards");
        entity.HasKey(e => e.PtInsurCardId);
        entity.Property(e => e.PtInsurCardId).HasColumnName("pt_insur_card_id");
        entity.Property(e => e.PtId).HasColumnName("pt_id");
        entity.Property(e => e.IsPrimary).HasColumnName("is_primary");
        entity.Property(e => e.InsurerId).HasColumnName("insurer_id");
        entity.Property(e => e.InsurerTypeCode).HasColumnName("insurer_type_code");
        entity.Property(e => e.InsurerCode).HasColumnName("insurer_code");
        entity.Property(e => e.InsurerName).HasColumnName("insurer_name");
        entity.Property(e => e.InsuredCode).HasColumnName("insured_code");
        entity.Property(e => e.InsuredCodeSymbol).HasColumnName("insured_code_symbol");
        entity.Property(e => e.InsuredCodeNumber).HasColumnName("insured_code_number");
        entity.Property(e => e.InsuredCodeBranchNumber).HasColumnName("insured_code_branch_number");
        entity.Property(e => e.InsuredPersonName).HasColumnName("insured_person_name");
        entity.Property(e => e.SelfFamilyRelationshipCode).HasColumnName("self_family_relationship_code");
        entity.Property(e => e.AssistanceCode).HasColumnName("assistance_code");
        entity.Property(e => e.ContinuationCode).HasColumnName("continuation_code");
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.DeactivatedOn).HasColumnName("deactivated_on");
        entity.Property(e => e.PtInsureCardMemo).HasColumnName("pt_insure_card_memo");
        entity.Property(e => e.PtInsureCardSeq).HasColumnName("pt_insure_card_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Patient)
            .WithMany(p => p.PatientInsuranceCards)
            .HasForeignKey(e => e.PtId);
        entity.HasOne(e => e.InsuranceProvider)
            .WithMany()
            .HasForeignKey(e => e.InsurerId);

        entity.HasIndex(e => e.PtId);
        entity.HasIndex(e => e.InsurerId);
    }
}

