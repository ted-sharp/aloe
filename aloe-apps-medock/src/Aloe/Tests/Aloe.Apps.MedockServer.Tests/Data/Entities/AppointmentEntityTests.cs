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
        var floorId = Guid.CreateVersion7();
        var orgId = Guid.CreateVersion7();
        var ptId = Guid.CreateVersion7();

        // Act
        var appointment = new Appointment
        {
            ApptId = Guid.CreateVersion7(),
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
    public void Appointment_Should_Have_Start_Time_In_Minutes()
    {
        // Arrange
        var startMin = 540; // 9:00 AM

        // Act
        var appointment = new Appointment
        {
            ApptStartMin = startMin
        };

        // Assert
        appointment.ApptStartMin.Should().Be(startMin);
    }

    [Fact]
    public void Appointment_Should_Default_To_9_00_AM()
    {
        // Arrange & Act
        var appointment = new Appointment();

        // Assert
        appointment.ApptStartMin.Should().Be(540); // Default: 9:00 AM
    }
}





