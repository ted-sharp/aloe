using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockServer.Tests.Data;

/// <summary>
/// MedockDbContextのテスト
/// </summary>
public class MedockDbContextTests
{
    private static DbContextOptions<MedockDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<MedockDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.CreateVersion7().ToString())
            .Options;
    }

    [Fact]
    public async Task Can_Add_And_Retrieve_User()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var userId = Guid.CreateVersion7();

        // Act
        await using (var context = new MedockDbContext(options))
        {
            var user = new User
            {
                UserId = userId,
                UserCode = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new MedockDbContext(options))
        {
            var user = await context.Users.FindAsync(userId);
            user.Should().NotBeNull();
            user!.UserCode.Should().Be("testuser");
        }
    }

    [Fact]
    public async Task Can_Add_And_Retrieve_Tenant()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var tenantId = Guid.CreateVersion7();

        // Act
        await using (var context = new MedockDbContext(options))
        {
            var tenant = new Tenant
            {
                TenantId = tenantId,
                TenantName = "Test Tenant",
                IsActive = true,
                ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
            };
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new MedockDbContext(options))
        {
            var tenant = await context.Tenants.FindAsync(tenantId);
            tenant.Should().NotBeNull();
            tenant!.TenantName.Should().Be("Test Tenant");
        }
    }

    [Fact]
    public async Task Can_Add_Appointment_With_Relations()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var tenantId = Guid.CreateVersion7();
        var facilityId = Guid.CreateVersion7();
        var floorId = Guid.CreateVersion7();
        var orgId = Guid.CreateVersion7();
        var ptId = Guid.CreateVersion7();
        var apptId = Guid.CreateVersion7();

        // Act
        await using (var context = new MedockDbContext(options))
        {
            // テナント作成
            context.Tenants.Add(new Tenant
            {
                TenantId = tenantId,
                TenantName = "Test Tenant",
                ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
            });

            // 施設作成
            context.Facilities.Add(new Facility
            {
                FacilityId = facilityId,
                TenantId = tenantId,
                FacilityName = "Test Facility",
                ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
            });

            // フロア作成
            context.Floors.Add(new Floor
            {
                FloorId = floorId,
                FacilityId = facilityId,
                FloorCode = "1F",
                FloorName = "1階"
            });

            // 団体作成
            context.Organizations.Add(new Organization
            {
                OrgId = orgId,
                FacilityId = facilityId,
                OrgCode = "ORG001",
                OrgName = "Test Organization"
            });

            // 患者作成
            context.Patients.Add(new Patient
            {
                PtId = ptId,
                FacilityId = facilityId,
                PtCode = "PT001",
                PtName = "Test Patient",
                BirthDate = new DateOnly(1990, 1, 1)
            });

            // 予約作成
            context.Appointments.Add(new Appointment
            {
                ApptId = apptId,
                FloorId = floorId,
                OrgId = orgId,
                PtId = ptId,
                ApptDate = new DateOnly(2025, 12, 15),
                ApptStatusCode = 1
            });

            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new MedockDbContext(options))
        {
            var appointment = await context.Appointments
                .Include(a => a.Floor)
                .Include(a => a.Organization)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.ApptId == apptId);

            appointment.Should().NotBeNull();
            appointment!.Floor.FloorName.Should().Be("1階");
            appointment.Organization.OrgName.Should().Be("Test Organization");
            appointment.Patient.PtName.Should().Be("Test Patient");
        }
    }

    [Fact]
    public async Task Auditable_Fields_Are_Set_On_Add()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var userId = Guid.CreateVersion7();
        var sessionId = Guid.CreateVersion7();
        var actingUserId = Guid.CreateVersion7();

        // Act
        await using (var context = new MedockDbContext(options))
        {
            context.SetAuditInfo(actingUserId, sessionId);

            var user = new User
            {
                UserId = userId,
                UserCode = "audituser",
                Email = "audit@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = new MedockDbContext(options))
        {
            var user = await context.Users.FindAsync(userId);
            user.Should().NotBeNull();
            user!.CreatedUserId.Should().Be(actingUserId);
            user.CreatedSessionId.Should().Be(sessionId);
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}



