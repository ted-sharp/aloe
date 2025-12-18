using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// FacilityAddress エンティティ設定
/// </summary>
public class FacilityAddressConfiguration : IEntityTypeConfiguration<FacilityAddress>
{
    public void Configure(EntityTypeBuilder<FacilityAddress> entity)
    {
        entity.ToTable("facility_addresses");
        entity.HasKey(e => e.FacilityAdrId);
        entity.Property(e => e.FacilityAdrId).HasColumnName("facility_adr_id");
        entity.Property(e => e.FacilityId).HasColumnName("facility_id");
        entity.Property(e => e.AdrTypeCode).HasColumnName("adr_type_code");
        entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasMaxLength(7);
        entity.Property(e => e.Adr1).HasColumnName("adr1").HasMaxLength(100);
        entity.Property(e => e.Adr2).HasColumnName("adr2").HasMaxLength(100);
        entity.Property(e => e.Adr3).HasColumnName("adr3").HasMaxLength(100);
        entity.Property(e => e.AttentionName).HasColumnName("attention_name").HasMaxLength(100);
        entity.Property(e => e.Tel).HasColumnName("tel").HasMaxLength(20);
        entity.Property(e => e.Tel2).HasColumnName("tel2").HasMaxLength(20);
        entity.Property(e => e.Fax).HasColumnName("fax").HasMaxLength(20);
        entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
        entity.Property(e => e.AdrMemo).HasColumnName("adr_memo").HasMaxLength(1000);
        entity.Property(e => e.AdrSeq).HasColumnName("adr_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Facility)
            .WithMany(f => f.FacilityAddresses)
            .HasForeignKey(e => e.FacilityId);

        entity.HasIndex(e => e.Tel).HasFilter("[tel] <> ''");
        entity.HasIndex(e => e.Tel2).HasFilter("[tel2] <> ''");
        entity.HasIndex(e => e.Email).HasFilter("[email] <> ''");
    }
}

/// <summary>
/// OrganizationAddress エンティティ設定
/// </summary>
public class OrganizationAddressConfiguration : IEntityTypeConfiguration<OrganizationAddress>
{
    public void Configure(EntityTypeBuilder<OrganizationAddress> entity)
    {
        entity.ToTable("organization_addresses");
        entity.HasKey(e => e.OrgAdrId);
        entity.Property(e => e.OrgAdrId).HasColumnName("org_adr_id");
        entity.Property(e => e.OrgId).HasColumnName("org_id");
        entity.Property(e => e.AdrTypeCode).HasColumnName("adr_type_code");
        entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasMaxLength(7);
        entity.Property(e => e.Adr1).HasColumnName("adr1").HasMaxLength(100);
        entity.Property(e => e.Adr2).HasColumnName("adr2").HasMaxLength(100);
        entity.Property(e => e.Adr3).HasColumnName("adr3").HasMaxLength(100);
        entity.Property(e => e.AttentionName).HasColumnName("attention_name").HasMaxLength(100);
        entity.Property(e => e.Tel).HasColumnName("tel").HasMaxLength(20);
        entity.Property(e => e.Tel2).HasColumnName("tel2").HasMaxLength(20);
        entity.Property(e => e.Fax).HasColumnName("fax").HasMaxLength(20);
        entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
        entity.Property(e => e.AdrMemo).HasColumnName("adr_memo").HasMaxLength(1000);
        entity.Property(e => e.AdrSeq).HasColumnName("adr_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Organization)
            .WithMany(o => o.OrganizationAddresses)
            .HasForeignKey(e => e.OrgId);

        entity.HasIndex(e => e.Tel).HasFilter("[tel] <> ''");
        entity.HasIndex(e => e.Tel2).HasFilter("[tel2] <> ''");
        entity.HasIndex(e => e.Email).HasFilter("[email] <> ''");
    }
}

/// <summary>
/// PatientAddress エンティティ設定
/// </summary>
public class PatientAddressConfiguration : IEntityTypeConfiguration<PatientAddress>
{
    public void Configure(EntityTypeBuilder<PatientAddress> entity)
    {
        entity.ToTable("patient_addresses");
        entity.HasKey(e => e.PtAdrId);
        entity.Property(e => e.PtAdrId).HasColumnName("pt_adr_id");
        entity.Property(e => e.PtId).HasColumnName("pt_id");
        entity.Property(e => e.AdrTypeCode).HasColumnName("adr_type_code");
        entity.Property(e => e.PostalCode).HasColumnName("postal_code").HasMaxLength(7);
        entity.Property(e => e.Adr1).HasColumnName("adr1").HasMaxLength(100);
        entity.Property(e => e.Adr2).HasColumnName("adr2").HasMaxLength(100);
        entity.Property(e => e.Adr3).HasColumnName("adr3").HasMaxLength(100);
        entity.Property(e => e.AttentionName).HasColumnName("attention_name").HasMaxLength(100);
        entity.Property(e => e.Tel).HasColumnName("tel").HasMaxLength(20);
        entity.Property(e => e.Tel2).HasColumnName("tel2").HasMaxLength(20);
        entity.Property(e => e.Fax).HasColumnName("fax").HasMaxLength(20);
        entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
        entity.Property(e => e.AdrMemo).HasColumnName("adr_memo").HasMaxLength(1000);
        entity.Property(e => e.AdrSeq).HasColumnName("adr_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Patient)
            .WithMany(p => p.PatientAddresses)
            .HasForeignKey(e => e.PtId);

        entity.HasIndex(e => e.Tel).HasFilter("[tel] <> ''");
        entity.HasIndex(e => e.Tel2).HasFilter("[tel2] <> ''");
        entity.HasIndex(e => e.Email).HasFilter("[email] <> ''");
    }
}

