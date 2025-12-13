using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aloe.Apps.MedockLib.Data.Configurations;

/// <summary>
/// User エンティティ設定
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// Session エンティティ設定
/// </summary>
public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> entity)
    {
        entity.ToTable("sessions");
        entity.HasKey(e => e.SessionId);
        entity.Property(e => e.SessionId).HasColumnName("session_id");
        entity.Property(e => e.UserId).HasColumnName("user_id");
        entity.Property(e => e.UserDisplayName).HasColumnName("user_display_name");
        entity.Property(e => e.IssuedAt).HasColumnName("issued_at");
        entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
        entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
        entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(256);
        entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(512);
        entity.Property(e => e.AppName).HasColumnName("app_name").HasMaxLength(100);

        entity.HasOne(e => e.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(e => e.UserId);
    }
}

/// <summary>
/// Role エンティティ設定
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("roles");
        entity.HasKey(e => e.RoleCode);
        entity.Property(e => e.RoleCode).HasColumnName("role_code").HasMaxLength(10);
        entity.Property(e => e.RoleName).HasColumnName("role_name").HasMaxLength(100);
        entity.Property(e => e.RoleDesc).HasColumnName("role_desc").HasMaxLength(1000);
        entity.Property(e => e.RoleSeq).HasColumnName("role_seq");
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// Resource エンティティ設定
/// </summary>
public class ResourceConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> entity)
    {
        entity.ToTable("features");
        entity.HasKey(e => e.FeatureCode);
        entity.Property(e => e.FeatureCode).HasColumnName("feature_code").HasMaxLength(10);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// Operation エンティティ設定
/// </summary>
public class OperationConfiguration : IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> entity)
    {
        entity.ToTable("operations");
        entity.HasKey(e => e.OperationCode);
        entity.Property(e => e.OperationCode).HasColumnName("operation_code").HasMaxLength(10);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);
    }
}

/// <summary>
/// Permission エンティティ設定
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> entity)
    {
        entity.ToTable("permissions");
        entity.HasKey(e => e.PermissionCode);
        entity.Property(e => e.PermissionCode).HasColumnName("permission_code").HasMaxLength(21);
        entity.Property(e => e.FeatureCode).HasColumnName("feature_code").HasMaxLength(10);
        entity.Property(e => e.OperationCode).HasColumnName("operation_code").HasMaxLength(10);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne<Feature>()
            .WithMany(r => r.Permissions)
            .HasForeignKey(e => e.FeatureCode);
        entity.HasOne<Operation>()
            .WithMany(o => o.Permissions)
            .HasForeignKey(e => e.OperationCode);
    }
}

/// <summary>
/// FacilityUserRole エンティティ設定
/// </summary>
public class FacilityUserRoleConfiguration : IEntityTypeConfiguration<FacilityUserRole>
{
    public void Configure(EntityTypeBuilder<FacilityUserRole> entity)
    {
        entity.ToTable("facility_user_roles");
        entity.HasKey(e => e.FacilityUserRoleId);
        entity.Property(e => e.FacilityUserRoleId).HasColumnName("user_role_id");
        entity.Property(e => e.FacilityUserId).HasColumnName("facility_user_id");
        entity.Property(e => e.RoleCode).HasColumnName("role_code").HasMaxLength(10);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.FacilityUser)
            .WithMany(u => u.FacilityUserRoles)
            .HasForeignKey(e => e.FacilityUserId);
        entity.HasOne(e => e.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(e => e.RoleCode);
    }
}

/// <summary>
/// RolePermission エンティティ設定
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> entity)
    {
        entity.ToTable("role_permissions");
        entity.HasKey(e => e.RolePermissionCode);
        entity.Property(e => e.RolePermissionCode).HasColumnName("role_permission_code").HasMaxLength(32);
        entity.Property(e => e.RoleCode).HasColumnName("role_code").HasMaxLength(10);
        entity.Property(e => e.PermissionCode).HasColumnName("permission_code").HasMaxLength(21);
        entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
        ConfigurationHelper.ConfigureAuditableEntity(entity);

        entity.HasOne(e => e.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(e => e.RoleCode);
        entity.HasOne(e => e.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(e => e.PermissionCode);
    }
}
