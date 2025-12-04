using Aloe.Apps.MedockLib.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockLib.Data;

/// <summary>
/// Medock アプリケーションのデータベースコンテキスト
/// </summary>
public class MedockDbContext : DbContext
{
    private Guid _currentUserId;
    private Guid _currentSessionId;

    public MedockDbContext(DbContextOptions<MedockDbContext> options)
        : base(options)
    {
    }

    // 認証系
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // 組織系
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<Floor> Floors => Set<Floor>();
    public DbSet<Room> Rooms => Set<Room>();

    // 業務系
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentStats> AppointmentStats => Set<AppointmentStats>();
    public DbSet<RoomAppointmentStats> RoomAppointmentStats => Set<RoomAppointmentStats>();
    public DbSet<AppointmentSlot> AppointmentSlots => Set<AppointmentSlot>();
    public DbSet<RoomSlot> RoomSlots => Set<RoomSlot>();

    // マスタ系
    public DbSet<Holiday> Holidays => Set<Holiday>();

    /// <summary>
    /// 監査情報を設定します。SaveChanges時に自動でCreated/Updatedフィールドに反映されます。
    /// </summary>
    public void SetAuditInfo(Guid userId, Guid sessionId)
    {
        _currentUserId = userId;
        _currentSessionId = sessionId;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserCode).HasColumnName("user_code").HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(254);
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).HasColumnName("password_salt").HasMaxLength(64);
            entity.Property(e => e.ExpiresDate).HasColumnName("expires_date");
            entity.Property(e => e.LoginSuccessCount).HasColumnName("login_success_count");
            entity.Property(e => e.LoginFailureCount).HasColumnName("login_failure_count");
            entity.Property(e => e.LoginFailureAttempts).HasColumnName("login_failure_attempts");
            entity.Property(e => e.LockedUntilAt).HasColumnName("locked_until_at");
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.LastLogoutAt).HasColumnName("last_logout_at");
            entity.Property(e => e.IsSystemAdmin).HasColumnName("is_system_admin");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);
        });

        // Session
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(e => e.SessionId);
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.UserDisplayName).HasColumnName("user_display_name");
            entity.Property(e => e.ClientAppName).HasColumnName("client_app_name");
            entity.Property(e => e.ClientEndpoint).HasColumnName("client_endpoint");
            entity.Property(e => e.LoginAt).HasColumnName("login_at");
            entity.Property(e => e.LogoutAt).HasColumnName("logout_at");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId);
        });

        // Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");
            entity.HasKey(e => e.RoleCode);
            entity.Property(e => e.RoleCode).HasColumnName("role_code").HasMaxLength(10);
            entity.Property(e => e.RoleName).HasColumnName("role_name").HasMaxLength(100);
            entity.Property(e => e.RoleDesc).HasColumnName("role_desc").HasMaxLength(1000);
            entity.Property(e => e.RoleSeq).HasColumnName("role_seq");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);
        });

        // Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("permissions");
            entity.HasKey(e => e.PermissionCode);
            entity.Property(e => e.PermissionCode).HasColumnName("permission_code").HasMaxLength(21);
            entity.Property(e => e.ResourceCode).HasColumnName("resource_code").HasMaxLength(10);
            entity.Property(e => e.OperationCode).HasColumnName("operation_code").HasMaxLength(10);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);
        });

        // UserRole
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_roles");
            entity.HasKey(e => e.UserRoleId);
            entity.Property(e => e.UserRoleId).HasColumnName("user_role_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleCode).HasColumnName("role_code").HasMaxLength(10);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleCode);
        });

        // RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("role_permissions");
            entity.HasKey(e => e.RolePermissionCode);
            entity.Property(e => e.RolePermissionCode).HasColumnName("role_permission_code").HasMaxLength(32);
            entity.Property(e => e.RoleCode).HasColumnName("role_code").HasMaxLength(10);
            entity.Property(e => e.PermissionCode).HasColumnName("permission_code").HasMaxLength(21);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleCode);
            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionCode);
        });

        // Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");
            entity.HasKey(e => e.TenantId);
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.TenantName).HasColumnName("tenant_name").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);
        });

        // TenantUser
        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.ToTable("tenant_users");
            entity.HasKey(e => e.TenantUserId);
            entity.Property(e => e.TenantUserId).HasColumnName("tenant_user_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(100);
            entity.Property(e => e.TenantUserSeq).HasColumnName("tenant_user_seq");
            entity.Property(e => e.IsTenantAdmin).HasColumnName("is_tenant_admin");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.TenantUsers)
                .HasForeignKey(e => e.TenantId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.TenantUsers)
                .HasForeignKey(e => e.UserId);
        });

        // Facility
        modelBuilder.Entity<Facility>(entity =>
        {
            entity.ToTable("facilities");
            entity.HasKey(e => e.FacilityId);
            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.MedicalInstitutionCode).HasColumnName("medical_institution_code").HasMaxLength(10);
            entity.Property(e => e.FacilityName).HasColumnName("facility_name").HasMaxLength(100);
            entity.Property(e => e.FacilityNameDisplay).HasColumnName("facility_name_display").HasMaxLength(100);
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Facilities)
                .HasForeignKey(e => e.TenantId);
        });

        // Floor
        modelBuilder.Entity<Floor>(entity =>
        {
            entity.ToTable("floors");
            entity.HasKey(e => e.FloorId);
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.FloorCode).HasColumnName("floor_code");
            entity.Property(e => e.FloorName).HasColumnName("floor_name");
            entity.Property(e => e.FloorDesc).HasColumnName("floor_desc");
            entity.Property(e => e.FloorSeq).HasColumnName("floor_seq");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Facility)
                .WithMany(f => f.Floors)
                .HasForeignKey(e => e.FacilityId);
        });

        // Room
        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("rooms");
            entity.HasKey(e => e.RoomId);
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.RoomName).HasColumnName("room_name");
            entity.Property(e => e.RoomDesc).HasColumnName("room_desc");
            entity.Property(e => e.RoomSeq).HasColumnName("room_seq");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.Rooms)
                .HasForeignKey(e => e.FloorId);
        });

        // Patient
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients");
            entity.HasKey(e => e.PtId);
            entity.Property(e => e.PtId).HasColumnName("pt_id");
            entity.Property(e => e.CanonicalPtId).HasColumnName("canonical_pt_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.PrimaryOrgId).HasColumnName("primary_org_id");
            entity.Property(e => e.PtCode).HasColumnName("pt_code").HasMaxLength(100);
            entity.Property(e => e.KarteCode).HasColumnName("karte_code").HasMaxLength(100);
            entity.Property(e => e.PtName).HasColumnName("pt_name").HasMaxLength(100);
            entity.Property(e => e.PtNameCompat).HasColumnName("pt_name_compat").HasMaxLength(100);
            entity.Property(e => e.PtNameKatakana).HasColumnName("pt_name_katakana").HasMaxLength(100);
            entity.Property(e => e.PtNameKatakanaCompat).HasColumnName("pt_name_katakana_compat").HasMaxLength(100);
            entity.Property(e => e.PtMadenName).HasColumnName("pt_maden_name").HasMaxLength(100);
            entity.Property(e => e.PtAliasName).HasColumnName("pt_alias_name").HasMaxLength(100);
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.SexCode).HasColumnName("sex_code");
            entity.Property(e => e.PtMemo).HasColumnName("pt_memo").HasMaxLength(1000);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Patients)
                .HasForeignKey(e => e.TenantId);
            entity.HasOne(e => e.Facility)
                .WithMany(f => f.Patients)
                .HasForeignKey(e => e.FacilityId);
        });

        // Organization
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organizations");
            entity.HasKey(e => e.OrgId);
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.ParentOrgId).HasColumnName("parent_org_id");
            entity.Property(e => e.OrgCode).HasColumnName("org_code").HasMaxLength(13);
            entity.Property(e => e.OrgName).HasColumnName("org_name").HasMaxLength(100);
            entity.Property(e => e.OrgNameKatakana).HasColumnName("org_name_katakana").HasMaxLength(100);
            entity.Property(e => e.OrgNameKatakanaCompat).HasColumnName("org_name_katakana_compat").HasMaxLength(100);
            entity.Property(e => e.OrgNameDisplay).HasColumnName("org_name_display").HasMaxLength(100);
            entity.Property(e => e.OrgNamePrint).HasColumnName("org_name_print").HasMaxLength(100);
            entity.Property(e => e.OrgMemo).HasColumnName("org_memo").HasMaxLength(1000);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Organizations)
                .HasForeignKey(e => e.TenantId);
            entity.HasOne(e => e.Facility)
                .WithMany(f => f.Organizations)
                .HasForeignKey(e => e.FacilityId);
            entity.HasOne(e => e.ParentOrganization)
                .WithMany()
                .HasForeignKey(e => e.ParentOrgId);
        });

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(e => e.ApptId);
            entity.Property(e => e.ApptId).HasColumnName("appt_id");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.PtId).HasColumnName("pt_id");
            entity.Property(e => e.ApptDate).HasColumnName("appt_date");
            entity.Property(e => e.ApptStartAt).HasColumnName("appt_start_at");
            entity.Property(e => e.ApptEndAt).HasColumnName("appt_end_at");
            entity.Property(e => e.ApptStatusCode).HasColumnName("appt_status_code");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.Appointments)
                .HasForeignKey(e => e.FloorId);
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.Appointments)
                .HasForeignKey(e => e.OrgId);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PtId);
        });

        // Holiday
        modelBuilder.Entity<Holiday>(entity =>
        {
            entity.ToTable("holidays");
            entity.HasKey(e => e.HolidayDate);
            entity.Property(e => e.HolidayDate).HasColumnName("holiday_date");
            entity.Property(e => e.HolidayName).HasColumnName("holiday_name");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);
        });


        // AppointmentStats
        modelBuilder.Entity<AppointmentStats>(entity =>
        {
            entity.ToTable("appointment_stats");
            entity.HasKey(e => e.ApptStatId);
            entity.Property(e => e.ApptStatId).HasColumnName("appt_stat_id");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.ApptDate).HasColumnName("appt_date");
            entity.Property(e => e.ApptCount).HasColumnName("appt_count");
            entity.Property(e => e.ApptMax).HasColumnName("appt_max");
            entity.Property(e => e.ApptGraph).HasColumnName("appt_graph").HasColumnType("jsonb");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.AppointmentStats)
                .HasForeignKey(e => e.FloorId);
        });

        // RoomAppointmentStats
        modelBuilder.Entity<RoomAppointmentStats>(entity =>
        {
            entity.ToTable("room_appointment_stats");
            entity.HasKey(e => e.ApptStatId);
            entity.Property(e => e.ApptStatId).HasColumnName("appt_stat_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.ApptDate).HasColumnName("appt_date");
            entity.Property(e => e.ApptCount).HasColumnName("appt_count");
            entity.Property(e => e.ApptMax).HasColumnName("appt_max");
            entity.Property(e => e.ApptGraph).HasColumnName("appt_graph").HasColumnType("jsonb");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.RoomAppointmentStats)
                .HasForeignKey(e => e.RoomId);
        });

        // AppointmentSlot
        modelBuilder.Entity<AppointmentSlot>(entity =>
        {
            entity.ToTable("appointment_slots");
            entity.HasKey(e => e.ApptSlotId);
            entity.Property(e => e.ApptSlotId).HasColumnName("appt_slot_id");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.ApptSlots).HasColumnName("appt_slots").HasColumnType("jsonb");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.AppointmentSlots)
                .HasForeignKey(e => e.FloorId);
        });

        // RoomSlot
        modelBuilder.Entity<RoomSlot>(entity =>
        {
            entity.ToTable("room_slots");
            entity.HasKey(e => e.RoomSlotId);
            entity.Property(e => e.RoomSlotId).HasColumnName("room_slot_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.RoomSlots).HasColumnName("room_slots").HasColumnType("jsonb");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.RoomSlots)
                .HasForeignKey(e => e.RoomId);
        });

    }

    private static void ConfigureAuditableEntity<T>(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> entity)
        where T : class, IAuditableEntity
    {
        entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        entity.Property(e => e.CreatedUserId).HasColumnName("created_user_id");
        entity.Property(e => e.CreatedSessionId).HasColumnName("created_session_id");
        entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        entity.Property(e => e.UpdatedUserId).HasColumnName("updated_user_id");
        entity.Property(e => e.UpdatedSessionId).HasColumnName("updated_session_id");
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedUserId = _currentUserId;
                entry.Entity.CreatedSessionId = _currentSessionId;
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedUserId = _currentUserId;
                entry.Entity.UpdatedSessionId = _currentSessionId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedUserId = _currentUserId;
                entry.Entity.UpdatedSessionId = _currentSessionId;
            }
        }
    }
}

