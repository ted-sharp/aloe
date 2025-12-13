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
    public DbSet<Feature> Features => this.Set<Feature>();
    public DbSet<Operation> Operations => this.Set<Operation>();
    public DbSet<FacilityUserRole> UserRoles => this.Set<FacilityUserRole>();
    public DbSet<RolePermission> RolePermissions => this.Set<RolePermission>();

    // 組織系
    public DbSet<Tenant> Tenants => this.Set<Tenant>();
    public DbSet<Facility> Facilities => this.Set<Facility>();
    public DbSet<FacilityUser> FacilityUsers => this.Set<FacilityUser>();
    public DbSet<FacilityBusinessHours> FacilityBusinessHours => this.Set<FacilityBusinessHours>();
    public DbSet<Floor> Floors => this.Set<Floor>();

    // 業務系
    public DbSet<Patient> Patients => this.Set<Patient>();
    public DbSet<Organization> Organizations => this.Set<Organization>();
    public DbSet<Appointment> Appointments => this.Set<Appointment>();
    public DbSet<AppointmentResource> AppointmentResources => this.Set<AppointmentResource>();
    public DbSet<AppointmentSlot> AppointmentSlots => this.Set<AppointmentSlot>();
    public DbSet<AppointmentSlotOverride> AppointmentSlotOverrides => this.Set<AppointmentSlotOverride>();
    public DbSet<AppointmentResourceAssignment> AppointmentResourceReservations => this.Set<AppointmentResourceAssignment>();
    public DbSet<AppointmentStats> AppointmentStats => this.Set<AppointmentStats>();

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

        // IEntityTypeConfiguration を実装した設定クラスを自動登録
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MedockDbContext).Assembly);
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
