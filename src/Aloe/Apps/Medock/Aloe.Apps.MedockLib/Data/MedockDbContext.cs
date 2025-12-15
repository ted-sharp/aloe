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
    private readonly IDateTimeProvider? _dateTimeProvider;
    private readonly IServiceProvider? _serviceProvider;

    public MedockDbContext(
        DbContextOptions<MedockDbContext> options,
        IDateTimeProvider? dateTimeProvider = null,
        IServiceProvider? serviceProvider = null)
        : base(options)
    {
        this._dateTimeProvider = dateTimeProvider;
        this._serviceProvider = serviceProvider;
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
    public DbSet<AppointmentResourceAssignment> AppointmentResourceAssignments => this.Set<AppointmentResourceAssignment>();
    public DbSet<AppointmentStats> AppointmentStats => this.Set<AppointmentStats>();

    // マスタ系
    public DbSet<Holiday> Holidays => this.Set<Holiday>();

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

        // SaveChanges時にIUserContextServiceを取得（遅延評価）
        // Blazor Serverでは、ScopedサービスのライフタイムがSignalR接続に一致するため、
        // コンストラクタ時点では初期化されていない可能性がある
        // IServiceProviderを使用して、現在のスコープからIUserContextServiceを取得
        var userContextService = this._serviceProvider?.GetService<IUserContextService>();
        var userId = userContextService?.CurrentUser?.UserId ?? Guid.Empty;
        var sessionId = userContextService?.CurrentSessionId ?? Guid.Empty;

        foreach (var entry in this.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedUserId = userId;
                entry.Entity.CreatedSessionId = sessionId;
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedUserId = userId;
                entry.Entity.UpdatedSessionId = sessionId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedUserId = userId;
                entry.Entity.UpdatedSessionId = sessionId;
            }
        }
    }
}
