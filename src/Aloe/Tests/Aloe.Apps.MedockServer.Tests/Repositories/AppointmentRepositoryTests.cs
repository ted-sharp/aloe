using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Data.Entities;
using Aloe.Apps.MedockLib.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.MedockServer.Tests.Repositories;

/// <summary>
/// AppointmentRepositoryのテスト
/// </summary>
public class AppointmentRepositoryTests
{
    private static DbContextOptions<MedockDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<MedockDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private static async Task<(Guid TenantId, Guid FacilityId, Guid FloorId, Guid OrgId, Guid PtId)> SeedTestDataAsync(MedockDbContext context)
    {
        var tenantId = Guid.NewGuid();
        var facilityId = Guid.NewGuid();
        var floorId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var ptId = Guid.NewGuid();

        context.Tenants.Add(new Tenant
        {
            TenantId = tenantId,
            TenantName = "Test Tenant",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        });

        context.Facilities.Add(new Facility
        {
            FacilityId = facilityId,
            TenantId = tenantId,
            FacilityName = "Test Facility",
            ActiveFrom = DateOnly.FromDateTime(DateTime.Today)
        });

        context.Floors.Add(new Floor
        {
            FloorId = floorId,
            FacilityId = facilityId,
            FloorCode = "1F",
            FloorName = "1階"
        });

        context.Organizations.Add(new Organization
        {
            OrgId = orgId,
            FacilityId = facilityId,
            OrgCode = "ORG001",
            OrgName = "Test Organization"
        });

        context.Patients.Add(new Patient
        {
            PtId = ptId,
            FacilityId = facilityId,
            PtCode = "PT001",
            PtName = "Test Patient",
            BirthDate = new DateOnly(1990, 1, 1)
        });

        await context.SaveChangesAsync();

        return (tenantId, facilityId, floorId, orgId, ptId);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Appointment_When_Exists()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var apptId = Guid.NewGuid();

        await using (var context = new MedockDbContext(options))
        {
            var (_, _, floorId, orgId, ptId) = await SeedTestDataAsync(context);

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

        // Act
        await using (var context = new MedockDbContext(options))
        {
            var repository = new AppointmentRepository(context);
            var result = await repository.GetByIdAsync(apptId);

            // Assert
            result.Should().NotBeNull();
            result!.ApptId.Should().Be(apptId);
        }
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Null_When_Not_Exists()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var nonExistentId = Guid.NewGuid();

        // Act
        await using var context = new MedockDbContext(options);
        var repository = new AppointmentRepository(context);
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByDateRangeAsync_Should_Return_Appointments_In_Range()
    {
        // Arrange
        var options = CreateInMemoryOptions();

        await using (var context = new MedockDbContext(options))
        {
            var (_, _, floorId, orgId, ptId) = await SeedTestDataAsync(context);

            // 範囲内の予約
            context.Appointments.Add(new Appointment
            {
                ApptId = Guid.NewGuid(),
                FloorId = floorId,
                OrgId = orgId,
                PtId = ptId,
                ApptDate = new DateOnly(2025, 12, 10)
            });
            context.Appointments.Add(new Appointment
            {
                ApptId = Guid.NewGuid(),
                FloorId = floorId,
                OrgId = orgId,
                PtId = ptId,
                ApptDate = new DateOnly(2025, 12, 15)
            });

            // 範囲外の予約
            context.Appointments.Add(new Appointment
            {
                ApptId = Guid.NewGuid(),
                FloorId = floorId,
                OrgId = orgId,
                PtId = ptId,
                ApptDate = new DateOnly(2025, 12, 25)
            });

            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new MedockDbContext(options))
        {
            var repository = new AppointmentRepository(context);
            var startDate = new DateOnly(2025, 12, 1);
            var endDate = new DateOnly(2025, 12, 20);
            var result = await repository.GetByDateRangeAsync(startDate, endDate);

            // Assert
            result.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetByFloorAndDateAsync_Should_Return_Appointments_For_Floor()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        Guid targetFloorId;
        var targetDate = new DateOnly(2025, 12, 15);

        await using (var context = new MedockDbContext(options))
        {
            var (tenantId, facilityId, floorId, orgId, ptId) = await SeedTestDataAsync(context);
            targetFloorId = floorId;

            // 別のフロアを作成
            var otherFloorId = Guid.NewGuid();
            context.Floors.Add(new Floor
            {
                FloorId = otherFloorId,
                FacilityId = facilityId,
                FloorCode = "2F",
                FloorName = "2階"
            });

            // ターゲットフロアの予約
            context.Appointments.Add(new Appointment
            {
                ApptId = Guid.NewGuid(),
                FloorId = floorId,
                OrgId = orgId,
                PtId = ptId,
                ApptDate = targetDate
            });

            // 別のフロアの予約（同日）
            context.Appointments.Add(new Appointment
            {
                ApptId = Guid.NewGuid(),
                FloorId = otherFloorId,
                OrgId = orgId,
                PtId = ptId,
                ApptDate = targetDate
            });

            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new MedockDbContext(options))
        {
            var repository = new AppointmentRepository(context);
            var result = await repository.GetByFloorAndDateAsync(targetFloorId, targetDate);

            // Assert
            result.Should().HaveCount(1);
            result.First().FloorId.Should().Be(targetFloorId);
        }
    }

    [Fact]
    public async Task AddAsync_Should_Add_New_Appointment()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var apptId = Guid.NewGuid();
        Guid floorId, orgId, ptId;

        await using (var context = new MedockDbContext(options))
        {
            (_, _, floorId, orgId, ptId) = await SeedTestDataAsync(context);
        }

        // Act
        await using (var context = new MedockDbContext(options))
        {
            var repository = new AppointmentRepository(context);
            var appointment = new Appointment
            {
                ApptId = apptId,
                FloorId = floorId,
                OrgId = orgId,
                PtId = ptId,
                ApptDate = new DateOnly(2025, 12, 15),
                ApptStatusCode = 1
            };
            await repository.AddAsync(appointment);
        }

        // Assert
        await using (var context = new MedockDbContext(options))
        {
            var saved = await context.Appointments.FindAsync(apptId);
            saved.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Existing_Appointment()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var apptId = Guid.NewGuid();

        await using (var context = new MedockDbContext(options))
        {
            var (_, _, floorId, orgId, ptId) = await SeedTestDataAsync(context);

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

        // Act
        await using (var context = new MedockDbContext(options))
        {
            var repository = new AppointmentRepository(context);
            var appointment = await context.Appointments.FindAsync(apptId);
            appointment!.ApptStatusCode = 2;
            await repository.UpdateAsync(appointment);
        }

        // Assert
        await using (var context = new MedockDbContext(options))
        {
            var updated = await context.Appointments.FindAsync(apptId);
            updated!.ApptStatusCode.Should().Be(2);
        }
    }

    [Fact]
    public async Task DeleteAsync_Should_Soft_Delete_Appointment()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        var apptId = Guid.NewGuid();

        await using (var context = new MedockDbContext(options))
        {
            var (_, _, floorId, orgId, ptId) = await SeedTestDataAsync(context);

            context.Appointments.Add(new Appointment
            {
                ApptId = apptId,
                FloorId = floorId,
                OrgId = orgId,
                PtId = ptId,
                ApptDate = new DateOnly(2025, 12, 15)
            });
            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = new MedockDbContext(options))
        {
            var repository = new AppointmentRepository(context);
            await repository.DeleteAsync(apptId);
        }

        // Assert
        await using (var context = new MedockDbContext(options))
        {
            var deleted = await context.Appointments.FindAsync(apptId);
            deleted!.IsDeleted.Should().BeTrue();
        }
    }
}





