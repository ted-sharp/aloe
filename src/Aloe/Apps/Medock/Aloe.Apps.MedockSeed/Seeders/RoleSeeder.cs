using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockSeed.Seeders;

internal static class RoleSeeder
{
    public static async Task SeedAsync(MedockDbContext context, IDateTimeProvider dateTimeProvider)
    {
        var existingRoles = await context.Roles.AnyAsync();
        if (existingRoles)
        {
            Console.WriteLine("[SKIP] Roles already exist.");
        }

        Console.WriteLine("[INFO] Creating role, permission, and RBAC seed data...");

        var roles = new List<Role>
        {
            new() { RoleCode = "ADMIN", RoleName = "管理者", RoleDesc = "システム管理者", RoleSeq = 1 },
            new() { RoleCode = "DOCTOR", RoleName = "医師", RoleDesc = "医師ユーザー", RoleSeq = 2 },
            new() { RoleCode = "NURSE", RoleName = "看護師", RoleDesc = "看護師ユーザー", RoleSeq = 3 },
            new() { RoleCode = "RECEPTION", RoleName = "受付", RoleDesc = "受付ユーザー", RoleSeq = 4 },
            new() { RoleCode = "VIEWER", RoleName = "閲覧", RoleDesc = "データ閲覧ユーザー", RoleSeq = 5 },
        };

        foreach (var role in roles)
        {
            role.IsDeleted = false;
            role.CreatedAt = dateTimeProvider.Now;
            role.UpdatedAt = dateTimeProvider.Now;
            role.CreatedUserId = Guid.Empty;
            role.CreatedSessionId = Guid.Empty;
            role.UpdatedUserId = Guid.Empty;
            role.UpdatedSessionId = Guid.Empty;
        }

        context.Roles.AddRange(roles);
        Console.WriteLine($"  [+] Roles: {roles.Count} entries");

        var permissions = new List<Permission>
        {
            new() { PermissionCode = "APPT_CREATE", FeatureCode = "APPT", OperationCode = "CREATE" },
            new() { PermissionCode = "APPT_READ", FeatureCode = "APPT", OperationCode = "READ" },
            new() { PermissionCode = "APPT_UPDATE", FeatureCode = "APPT", OperationCode = "UPDATE" },
            new() { PermissionCode = "APPT_DELETE", FeatureCode = "APPT", OperationCode = "DELETE" },
            new() { PermissionCode = "PATIENT_CREATE", FeatureCode = "PATIENT", OperationCode = "CREATE" },
            new() { PermissionCode = "PATIENT_READ", FeatureCode = "PATIENT", OperationCode = "READ" },
            new() { PermissionCode = "PATIENT_UPDATE", FeatureCode = "PATIENT", OperationCode = "UPDATE" },
            new() { PermissionCode = "PATIENT_DELETE", FeatureCode = "PATIENT", OperationCode = "DELETE" },
            new() { PermissionCode = "CALENDAR_VIEW", FeatureCode = "CALENDAR", OperationCode = "READ" },
            new() { PermissionCode = "USER_ADMIN", FeatureCode = "USER", OperationCode = "ADMIN" },
        };

        foreach (var permission in permissions)
        {
            permission.IsDeleted = false;
            permission.CreatedAt = dateTimeProvider.Now;
            permission.UpdatedAt = dateTimeProvider.Now;
            permission.CreatedUserId = Guid.Empty;
            permission.CreatedSessionId = Guid.Empty;
            permission.UpdatedUserId = Guid.Empty;
            permission.UpdatedSessionId = Guid.Empty;
        }

        context.Permissions.AddRange(permissions);
        Console.WriteLine($"  [+] Permissions: {permissions.Count} entries");

        var rolePermissions = new List<RolePermission>
        {
            new() { RolePermissionCode = "ADMIN_APPT_CREATE", RoleCode = "ADMIN", PermissionCode = "APPT_CREATE" },
            new() { RolePermissionCode = "ADMIN_APPT_READ", RoleCode = "ADMIN", PermissionCode = "APPT_READ" },
            new() { RolePermissionCode = "ADMIN_APPT_UPDATE", RoleCode = "ADMIN", PermissionCode = "APPT_UPDATE" },
            new() { RolePermissionCode = "ADMIN_APPT_DELETE", RoleCode = "ADMIN", PermissionCode = "APPT_DELETE" },
            new() { RolePermissionCode = "ADMIN_PATIENT_CREATE", RoleCode = "ADMIN", PermissionCode = "PATIENT_CREATE" },
            new() { RolePermissionCode = "ADMIN_PATIENT_READ", RoleCode = "ADMIN", PermissionCode = "PATIENT_READ" },
            new() { RolePermissionCode = "ADMIN_PATIENT_UPDATE", RoleCode = "ADMIN", PermissionCode = "PATIENT_UPDATE" },
            new() { RolePermissionCode = "ADMIN_PATIENT_DELETE", RoleCode = "ADMIN", PermissionCode = "PATIENT_DELETE" },
            new() { RolePermissionCode = "ADMIN_CALENDAR_VIEW", RoleCode = "ADMIN", PermissionCode = "CALENDAR_VIEW" },
            new() { RolePermissionCode = "ADMIN_USER_ADMIN", RoleCode = "ADMIN", PermissionCode = "USER_ADMIN" },
            new() { RolePermissionCode = "DOCTOR_APPT_CREATE", RoleCode = "DOCTOR", PermissionCode = "APPT_CREATE" },
            new() { RolePermissionCode = "DOCTOR_APPT_READ", RoleCode = "DOCTOR", PermissionCode = "APPT_READ" },
            new() { RolePermissionCode = "DOCTOR_APPT_UPDATE", RoleCode = "DOCTOR", PermissionCode = "APPT_UPDATE" },
            new() { RolePermissionCode = "DOCTOR_PATIENT_READ", RoleCode = "DOCTOR", PermissionCode = "PATIENT_READ" },
            new() { RolePermissionCode = "DOCTOR_PATIENT_UPDATE", RoleCode = "DOCTOR", PermissionCode = "PATIENT_UPDATE" },
            new() { RolePermissionCode = "DOCTOR_CALENDAR_VIEW", RoleCode = "DOCTOR", PermissionCode = "CALENDAR_VIEW" },
            new() { RolePermissionCode = "NURSE_APPT_READ", RoleCode = "NURSE", PermissionCode = "APPT_READ" },
            new() { RolePermissionCode = "NURSE_PATIENT_READ", RoleCode = "NURSE", PermissionCode = "PATIENT_READ" },
            new() { RolePermissionCode = "NURSE_CALENDAR_VIEW", RoleCode = "NURSE", PermissionCode = "CALENDAR_VIEW" },
            new() { RolePermissionCode = "RECEPTION_APPT_CREATE", RoleCode = "RECEPTION", PermissionCode = "APPT_CREATE" },
            new() { RolePermissionCode = "RECEPTION_APPT_READ", RoleCode = "RECEPTION", PermissionCode = "APPT_READ" },
            new() { RolePermissionCode = "RECEPTION_APPT_UPDATE", RoleCode = "RECEPTION", PermissionCode = "APPT_UPDATE" },
            new() { RolePermissionCode = "RECEPTION_PATIENT_READ", RoleCode = "RECEPTION", PermissionCode = "PATIENT_READ" },
            new() { RolePermissionCode = "RECEPTION_CALENDAR_VIEW", RoleCode = "RECEPTION", PermissionCode = "CALENDAR_VIEW" },
            new() { RolePermissionCode = "VIEWER_CALENDAR_VIEW", RoleCode = "VIEWER", PermissionCode = "CALENDAR_VIEW" },
        };

        foreach (var rolePermission in rolePermissions)
        {
            rolePermission.IsDeleted = false;
            rolePermission.CreatedAt = dateTimeProvider.Now;
            rolePermission.UpdatedAt = dateTimeProvider.Now;
            rolePermission.CreatedUserId = Guid.Empty;
            rolePermission.CreatedSessionId = Guid.Empty;
            rolePermission.UpdatedUserId = Guid.Empty;
            rolePermission.UpdatedSessionId = Guid.Empty;
        }

        context.RolePermissions.AddRange(rolePermissions);
        Console.WriteLine($"  [+] RolePermissions: {rolePermissions.Count} entries ({roles.Count} roles × permissions)");
    }
}
