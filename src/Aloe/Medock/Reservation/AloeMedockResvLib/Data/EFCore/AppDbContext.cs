using System.Data.Common;
using System.Diagnostics;
using Aloe.Common.AloeCoreLib.Security;
using Aloe.Common.AloeCoreLib.Util;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Entities;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Constants;
using Aloe.Medock.Reservation.AloeMedockResvLib.Domain.Services;
using Aloe.Medock.Reservation.AloeMedockResvLib.Data.Dto;
using Microsoft.EntityFrameworkCore;

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

    #region Customer

    #region Customer/Organization

    public DbSet<InsuranceProvider> InsuranceProviders { get; set; } = null!;

    public DbSet<Organization> Organizations { get; set; } = null!;

    public DbSet<OrganizationMember> OrganizationMembers { get; set; } = null!;

    #endregion Customer/Organization

    #region Customer/Patient

    public DbSet<Patient> Patients { get; set; } = null!;

    public DbSet<PatientInsuranceCard> PatientInsuranceCards { get; set; } = null!;

    #endregion Customer/Patient

    public DbSet<CustomerLocation> CustomerLocations { get; set; } = null!;

    public DbSet<CustomerMark> CustomerMarks { get; set; } = null!;

    public DbSet<CustomerNote> CustomerNotes { get; set; } = null!;

    public DbSet<CustomerFile> CustomerFiles { get; set; } = null!;

    #endregion Customer

    #region Plan

    public DbSet<CheckupPlanCategory> CheckupPlanCategories { get; set; } = null!;

    public DbSet<CheckupPlan> CheckupPlans { get; set; } = null!;

    public DbSet<CheckupPlanDetail> CheckupPlanDetails { get; set; } = null!;

    public DbSet<CheckupOption> CheckupOptions { get; set; } = null!;

    public DbSet<CheckupOptionDetail> CheckupOptionDetails { get; set; } = null!;

    public DbSet<ExamCategory> ExamCategories { get; set; } = null!;

    public DbSet<Exam> Exams { get; set; } = null!;

    public DbSet<ExamDetail> ExamDetails { get; set; } = null!;

    public DbSet<ExamObservationCategory> ExamObservationCategories { get; set; } = null!;

    public DbSet<ExamObservation> ExamObservations { get; set; } = null!;

    #endregion Plan

    #region Contract

    public DbSet<DefaultTaxRate> DefaultTaxRates { get; set; } = null!;

    public DbSet<Contract> Contracts { get; set; } = null!;

    public DbSet<ContractPlan> ContractPlans { get; set; } = null!;

    public DbSet<ContractOption> ContractOptions { get; set; } = null!;

    public DbSet<ContractPrice> ContractPrices { get; set; } = null!;

    public DbSet<ContractCap> ContractCaps { get; set; } = null!;

    public DbSet<ContractCapDetail> ContractCapDetails { get; set; } = null!;

    #endregion Contract

    #region ResvEquipService

    public DbSet<ReservationEquipment> Equipments { get; set; } = null!;

    public DbSet<ReservationEquipmentSlot> EquipmentSlots { get; set; } = null!;

    public DbSet<ReservationEquipmentBooking> EquipmentBookings { get; set; } = null!;

    #endregion ResvEquipService

    #region ResvDailyService

    public DbSet<ReservationFloor> Floors { get; set; } = null!;

    public DbSet<ReservationRoom> Rooms { get; set; } = null!;

    public DbSet<ReservationRoomDetail> RoomDetails { get; set; } = null!;

    public DbSet<Holiday> Holidays { get; set; } = null!;

    public DbSet<ReservationDailySlot> DailySlots { get; set; } = null!;

    public DbSet<ReservationDailyCap> DailyCaps { get; set; } = null!;

    public DbSet<ReservationDailySlotCap> DailySlotCaps { get; set; } = null!;

    public DbSet<ReservationDailyNote> DailyNotes { get; set; } = null!;

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
