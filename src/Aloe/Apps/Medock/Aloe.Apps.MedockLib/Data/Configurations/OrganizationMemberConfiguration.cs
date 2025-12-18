using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// OrganizationMember エンティティ設定
/// </summary>
public class OrganizationMemberConfiguration : IEntityTypeConfiguration<OrganizationMember>
{
    public void Configure(EntityTypeBuilder<OrganizationMember> entity)
    {
        entity.ToTable("organization_members");
        entity.HasKey(e => e.OrgMemberId);
        entity.Property(e => e.OrgMemberId).HasColumnName("org_member_id");
        entity.Property(e => e.OrgId).HasColumnName("org_id");
        entity.Property(e => e.PtId).HasColumnName("pt_id");
        entity.Property(e => e.PersonalCode).HasColumnName("personal_code").HasMaxLength(100);
        entity.Property(e => e.Department).HasColumnName("department").HasMaxLength(100);
        entity.Property(e => e.IsActive).HasColumnName("is_active");
        entity.Property(e => e.DeactivatedOn).HasColumnName("deactivated_on");
        entity.Property(e => e.OrgMemberMemo).HasColumnName("org_member_memo").HasMaxLength(1000);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Organization)
            .WithMany(o => o.OrganizationMembers)
            .HasForeignKey(e => e.OrgId);
        entity.HasOne(e => e.Patient)
            .WithMany()
            .HasForeignKey(e => e.PtId);

        entity.HasIndex(e => new { e.OrgId, e.PtId });
    }
}

