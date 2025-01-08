using System.Data.Common;
using System.Diagnostics;
using Aloe.Common.AloeCoreLib.Security;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Aloe.Medock.Reservation.AloeMedockResvLib.Grpc.Dto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Aloe.Medock.Reservation.AloeMedockResvLib.Data.EFCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{

    /// <summary>
    /// コード記述の参考用です。
    /// 実際のテーブルなどは作成していないため動作させようとすると例外が発生します。
    /// </summary>
    public DbSet<Sample> Samples { get; set; } = null!;

    #region AuthService

    public DbSet<Session> Sessions { get; set; } = null!;

    public DbSet<User> Users { get; set; } = null!;

    public DbSet<Role> Roles { get; set; } = null!;

    public DbSet<UserRole> UserRoles { get; set; } = null!;

    public DbSet<Permission> Permissions { get; set; } = null!;

    public DbSet<RolePermission> RolePermissions { get; set; } = null!;

    public DbSet<Policy> Policies { get; set; } = null!;

    public DbSet<Preference> Preferences { get; set; } = null!;
    public DbSet<UserPreference> UserPreferences { get; set; } = null!;

    #endregion AuthService

    #region Organization

    public DbSet<InsuranceProvider> InsuranceProviders { get; set; } = null!;

    public DbSet<Organization> Organizations { get; set; } = null!;

    public DbSet<OrganizationContact> OrganizationContacts { get; set; } = null!;

    public DbSet<OrganizationRemark> OrganizationRemarks { get; set; } = null!;

    #endregion Organization

    #region Patient

    public DbSet<Patient> Patients { get; set; } = null!;

    public DbSet<PatientContact> PatientContacts { get; set; } = null!;

    public DbSet<PatientRemark> PatientRemarks { get; set; } = null!;

    public DbSet<PatientInsuranceCard> PatientInsuranceCards { get; set; } = null!;

    #endregion Patient

    #region ResvEquipService

    public DbSet<ReservationEquipment> Equipments { get; set; } = null!;

    public DbSet<ReservationEquipmentSlot> EquipmentSlots { get; set; } = null!;

    public DbSet<ReservationEquipmentBooking> EquipmentBookings { get; set; } = null!;

    #endregion ResvEquipService

    #region ResvDailyService

    public DbSet<ReservationFloor> Floors { get; set; } = null!;

    public DbSet<ReservationRoom> Rooms { get; set; } = null!;

    public DbSet<ReservationDailySlot> DailySlots { get; set; } = null!;

    public DbSet<ReservationDailyBooking> DailyBookings { get; set; } = null!;

    #endregion ResvDailyService

    /// <summary>
    /// 接続文字列からホスト名を取得します。
    /// </summary>
    public string GetHost()
    {
        // 現在の接続文字列を取得
        var connectionString = this.Database.GetDbConnection().ConnectionString;

        // 接続文字列を解析して Host を取り出す
        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        // Npgsql（PostgreSQL）向けの解析 (例: Host=localhost;Port=5432;...)
        if (builder.ContainsKey("Host"))
        {
            return builder["Host"].ToString() ?? "";
        }

        // SQL Server の場合 (例: Server=localhost;Database=SampleDB;...)
        if (builder.ContainsKey("Server"))
        {
            return builder["Server"].ToString() ?? "";
        }

        return "Host information not found";
    }
}
