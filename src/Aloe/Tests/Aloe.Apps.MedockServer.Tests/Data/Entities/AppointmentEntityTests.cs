using Aloe.Apps.MedockLib.Data.Entities;
using FluentAssertions;

namespace Aloe.Apps.MedockServer.Tests.Data.Entities;

/// <summary>
/// Appointmentエンティティのテスト
/// </summary>
public class AppointmentEntityTests
{
    [Fact]
    public void Appointment_Should_Have_Required_Properties()
    {
        // Arrange
        var floorId = Guid.NewGuid();
        var orgId = Guid.NewGuid();
        var ptId = Guid.NewGuid();

        // Act
        var appointment = new Appointment
        {
            ApptId = Guid.NewGuid(),
            FloorId = floorId,
            OrgId = orgId,
            PtId = ptId,
            ApptDate = new DateOnly(2025, 12, 15)
        };

        // Assert
        appointment.ApptId.Should().NotBeEmpty();
        appointment.FloorId.Should().Be(floorId);
        appointment.OrgId.Should().Be(orgId);
        appointment.PtId.Should().Be(ptId);
        appointment.ApptDate.Should().Be(new DateOnly(2025, 12, 15));
    }

    [Fact]
    public void Appointment_Should_Have_Default_Status()
    {
        // Arrange & Act
        var appointment = new Appointment();

        // Assert
        appointment.ApptStatusCode.Should().Be(0);
        appointment.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Appointment_Should_Have_Time_Range()
    {
        // Arrange
        var startTime = new TimeOnly(9, 0);
        var durationMinutes = 60;

        // Act
        var appointment = new Appointment
        {
            ApptStartTime = startTime,
            ApptDurationMin = durationMinutes
        };

        // Assert
        appointment.ApptStartTime.Should().Be(startTime);
        appointment.ApptDurationMin.Should().Be(durationMinutes);
        // ApptEndTime is a GENERATED column, so it will be calculated by the database
    }
}





