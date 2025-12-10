using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.MedockLib.Data;

/// <summary>
/// Medock アプリケーションのデータベースコンテキスト
/// </summary>
public class MedockDbContext : DbContext
{
    private Guid _currentUserId;
    private Guid _currentSessionId;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public MedockDbContext(DbContextOptions<MedockDbContext> options, IServiceProvider? serviceProvider = null)
        : base(options)
    {
        this._dateTimeProvider = serviceProvider?.GetService<IDateTimeProvider>();
    }

    // 認証系
    public DbSet<User> Users => this.Set<User>();
    public DbSet<Session> Sessions => this.Set<Session>();
    public DbSet<Role> Roles => this.Set<Role>();
    public DbSet<Permission> Permissions => this.Set<Permission>();
    public DbSet<Resource> Resources => this.Set<Resource>();
    public DbSet<Operation> Operations => this.Set<Operation>();
    public DbSet<FacilityUserRole> UserRoles => this.Set<FacilityUserRole>();
    public DbSet<RolePermission> RolePermissions => this.Set<RolePermission>();

    // 組織系
    public DbSet<Tenant> Tenants => this.Set<Tenant>();
    public DbSet<Facility> Facilities => this.Set<Facility>();
    public DbSet<FacilityUser> FacilityUsers => this.Set<FacilityUser>();
    public DbSet<FacilityBusinessHours> FacilityBusinessHours => this.Set<FacilityBusinessHours>();
    public DbSet<Floor> Floors => this.Set<Floor>();
    public DbSet<Equipment> Equipments => this.Set<Equipment>();

    // 業務系
    public DbSet<Patient> Patients => this.Set<Patient>();
    public DbSet<Organization> Organizations => this.Set<Organization>();
    public DbSet<Appointment> Appointments => this.Set<Appointment>();
    public DbSet<AppointmentStats> AppointmentStats => this.Set<AppointmentStats>();
    public DbSet<EquipmentAppointment> EquipmentAppointments => this.Set<EquipmentAppointment>();
    public DbSet<EquipmentAppointmentStats> EquipmentAppointmentStats => this.Set<EquipmentAppointmentStats>();
    public DbSet<AppointmentSlot> AppointmentSlots => this.Set<AppointmentSlot>();
    public DbSet<EquipmentSlot> EquipmentSlots => this.Set<EquipmentSlot>();

    // マスタ系
    public DbSet<Holiday> Holidays => this.Set<Holiday>();

    /// <summary>
    /// 監査情報を設定します。SaveChanges時に自動でCreated/Updatedフィールドに反映されます。
    /// </summary>
    public void SetAuditInfo(Guid userId, Guid sessionId)
    {
        this._currentUserId = userId;
        this._currentSessionId = sessionId;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
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
            entity.Property(e => e.IssuedAt).HasColumnName("issued_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at");
            entity.Property(e => e.IpAddress).HasColumnName("ip_address").HasMaxLength(256);
            entity.Property(e => e.UserAgent).HasColumnName("user_agent").HasMaxLength(512);
            entity.Property(e => e.AppName).HasColumnName("app_name").HasMaxLength(100);

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

        // Resource
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.ToTable("resources");
            entity.HasKey(e => e.ResourceCode);
            entity.Property(e => e.ResourceCode).HasColumnName("resource_code").HasMaxLength(10);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);
        });

        // Operation
        modelBuilder.Entity<Operation>(entity =>
        {
            entity.ToTable("operations");
            entity.HasKey(e => e.OperationCode);
            entity.Property(e => e.OperationCode).HasColumnName("operation_code").HasMaxLength(10);
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

            entity.HasOne<Resource>()
                .WithMany(r => r.Permissions)
                .HasForeignKey(e => e.ResourceCode);
            entity.HasOne<Operation>()
                .WithMany(o => o.Permissions)
                .HasForeignKey(e => e.OperationCode);
        });

        // FacilityUserRole
        modelBuilder.Entity<FacilityUserRole>(entity =>
        {
            entity.ToTable("facility_user_roles");
            entity.HasKey(e => e.FacilityUserRoleId);
            entity.Property(e => e.FacilityUserRoleId).HasColumnName("user_role_id");
            entity.Property(e => e.FacilityUserId).HasColumnName("facility_user_id");
            entity.Property(e => e.RoleCode).HasColumnName("role_code").HasMaxLength(10);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.FacilityUser)
                .WithMany(u => u.FacilityUserRoles)
                .HasForeignKey(e => e.FacilityUserId);
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

        // FacilityUser
        modelBuilder.Entity<FacilityUser>(entity =>
        {
            entity.ToTable("facility_users");
            entity.HasKey(e => e.FacilityUserId);
            entity.Property(e => e.FacilityUserId).HasColumnName("facility_user_id");
            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FacilityUserSeq).HasColumnName("facility_user_seq");
            entity.Property(e => e.IsFacilityAdmin).HasColumnName("is_facility_admin");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Facility)
                .WithMany(f => f.FacilityUsers)
                .HasForeignKey(e => e.FacilityId);
            entity.HasOne(e => e.User)
                .WithMany(u => u.FacilityUsers)
                .HasForeignKey(e => e.UserId);
        });

        // FacilityBusinessHours
        modelBuilder.Entity<FacilityBusinessHours>(entity =>
        {
            entity.ToTable("facility_business_hours");
            entity.HasKey(e => e.FacilityBusinessHoursId);
            entity.Property(e => e.FacilityBusinessHoursId).HasColumnName("facility_business_hours_id");
            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.BusinessHours).HasColumnName("business_hours").HasColumnType("jsonb");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Facility)
                .WithMany(f => f.FacilityBusinessHours)
                .HasForeignKey(e => e.FacilityId);
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

        // Equipment
        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.ToTable("equipments");
            entity.HasKey(e => e.EquipId);
            entity.Property(e => e.EquipId).HasColumnName("equip_id");
            entity.Property(e => e.FloorId).HasColumnName("floor_id");
            entity.Property(e => e.EquipName).HasColumnName("equip_name").HasMaxLength(100);
            entity.Property(e => e.EquipDesc).HasColumnName("equip_desc").HasMaxLength(1000);
            entity.Property(e => e.EquipSeq).HasColumnName("equip_seq");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Floor)
                .WithMany(f => f.Equipments)
                .HasForeignKey(e => e.FloorId);
        });

        // Patient
        modelBuilder.Entity<Patient>(entity =>
        {
            entity.ToTable("patients");
            entity.HasKey(e => e.PtId);
            entity.Property(e => e.PtId).HasColumnName("pt_id");
            entity.Property(e => e.CanonicalPtId).HasColumnName("canonical_pt_id");
            entity.Property(e => e.FacilityId).HasColumnName("facility_id");
            entity.Property(e => e.PrimaryOrgId).HasColumnName("primary_org_id");
            entity.Property(e => e.PtCode).HasColumnName("pt_code").HasMaxLength(100);
            entity.Property(e => e.KarteCode).HasColumnName("karte_code").HasMaxLength(100);
            entity.Property(e => e.PtName).HasColumnName("pt_name").HasMaxLength(100);
            entity.Property(e => e.PtNameCompat).HasColumnName("pt_name_compat").HasMaxLength(100);
            entity.Property(e => e.PtNameKatakana).HasColumnName("pt_name_katakana").HasMaxLength(100);
            entity.Property(e => e.PtNameKatakanaCompat).HasColumnName("pt_name_katakana_compat").HasMaxLength(100);
            entity.Property(e => e.PtMaidenName).HasColumnName("pt_maiden_name").HasMaxLength(100);
            entity.Property(e => e.PtAliasName).HasColumnName("pt_alias_name").HasMaxLength(100);
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.SexCode).HasColumnName("sex_code");
            entity.Property(e => e.PtMemo).HasColumnName("pt_memo").HasMaxLength(1000);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

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
            entity.Property(e => e.ApptMemo).HasColumnName("appt_memo").HasMaxLength(1000);
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

        // EquipmentAppointmentStats
        modelBuilder.Entity<EquipmentAppointmentStats>(entity =>
        {
            entity.ToTable("equipment_appointment_stats");
            entity.HasKey(e => e.ApptStatId);
            entity.Property(e => e.ApptStatId).HasColumnName("equip_appt_stat_id");
            entity.Property(e => e.EquipId).HasColumnName("equip_id");
            entity.Property(e => e.ApptDate).HasColumnName("appt_date");
            entity.Property(e => e.ApptCount).HasColumnName("appt_count");
            entity.Property(e => e.ApptMax).HasColumnName("appt_max");
            entity.Property(e => e.ApptGraph).HasColumnName("appt_graph").HasColumnType("jsonb");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Equipment)
                .WithMany(eq => eq.EquipmentAppointmentStats)
                .HasForeignKey(e => e.EquipId);
        });

        // EquipmentAppointment
        modelBuilder.Entity<EquipmentAppointment>(entity =>
        {
            entity.ToTable("equipment_appointments");
            entity.HasKey(e => e.EquipApptId);
            entity.Property(e => e.EquipApptId).HasColumnName("equip_appt_id");
            entity.Property(e => e.EquipId).HasColumnName("equip_id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.PtId).HasColumnName("pt_id");
            entity.Property(e => e.ApptDate).HasColumnName("appt_date");
            entity.Property(e => e.ApptStartAt).HasColumnName("appt_start_at");
            entity.Property(e => e.ApptEndAt).HasColumnName("appt_end_at");
            entity.Property(e => e.ApptStatusCode).HasColumnName("appt_status_code");
            entity.Property(e => e.ApptMemo).HasColumnName("appt_memo").HasMaxLength(1000);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Equipment)
                .WithMany(eq => eq.EquipmentAppointments)
                .HasForeignKey(e => e.EquipId);
            entity.HasOne(e => e.Organization)
                .WithMany(o => o.EquipmentAppointments)
                .HasForeignKey(e => e.OrgId);
            entity.HasOne(e => e.Patient)
                .WithMany(p => p.EquipmentAppointments)
                .HasForeignKey(e => e.PtId);
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

        // EquipmentSlot
        modelBuilder.Entity<EquipmentSlot>(entity =>
        {
            entity.ToTable("equipment_slots");
            entity.HasKey(e => e.EquipSlotId);
            entity.Property(e => e.EquipSlotId).HasColumnName("equip_slot_id");
            entity.Property(e => e.EquipId).HasColumnName("equip_id");
            entity.Property(e => e.EquipSlots).HasColumnName("equip_slots").HasColumnType("jsonb");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            ConfigureAuditableEntity(entity);

            entity.HasOne(e => e.Equipment)
                .WithMany(eq => eq.EquipmentSlots)
                .HasForeignKey(e => e.EquipId);
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
        this.UpdateAuditFields();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditFields()
    {
        var now = this._dateTimeProvider?.Now ?? DateTime.Now;

        foreach (var entry in this.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedUserId = this._currentUserId;
                entry.Entity.CreatedSessionId = this._currentSessionId;
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedUserId = this._currentUserId;
                entry.Entity.UpdatedSessionId = this._currentSessionId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedUserId = this._currentUserId;
                entry.Entity.UpdatedSessionId = this._currentSessionId;
            }
        }
    }
}

