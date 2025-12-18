using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// FacilityUserPermissionsCache エンティティ設定
/// </summary>
public class FacilityUserPermissionsCacheConfiguration : IEntityTypeConfiguration<FacilityUserPermissionsCache>
{
    public void Configure(EntityTypeBuilder<FacilityUserPermissionsCache> entity)
    {
        entity.ToTable("facility_user_permissions_cache");
        entity.HasKey(e => e.FacilityUserId);
        entity.Property(e => e.FacilityUserId).HasColumnName("facility_user_id");
        entity.Property(e => e.PermissionCodes).HasColumnName("permission_codes");
        entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");

        entity.HasOne(e => e.FacilityUser)
            .WithOne(fu => fu.FacilityUserPermissionsCache)
            .HasForeignKey<FacilityUserPermissionsCache>(e => e.FacilityUserId);
    }
}

/// <summary>
/// UserToken エンティティ設定
/// </summary>
public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> entity)
    {
        entity.ToTable("user_tokens");
        entity.HasKey(e => e.TokenId);
        entity.Property(e => e.TokenId).HasColumnName("token_id");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.TokenProvider).HasColumnName("token_provider");
        entity.Property(e => e.TokenName).HasColumnName("token_name").HasMaxLength(64);
        entity.Property(e => e.TokenValue).HasColumnName("token_value");

        entity.HasOne(e => e.User)
            .WithMany(u => u.UserTokens)
            .HasForeignKey(e => e.UserId);

        entity.HasIndex(e => new { e.UserId, e.TokenProvider, e.TokenName })
            .IsUnique();
    }
}

