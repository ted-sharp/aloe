using Aloe.Apps.MedockLib.Data.Entities;
using FluentAssertions;

namespace Aloe.Apps.MedockServer.Tests.Data.Entities;

/// <summary>
/// Tenantエンティティのテスト
/// </summary>
public class TenantEntityTests
{
    [Fact]
    public void Tenant_Should_Have_Required_Properties()
    {
        // Arrange & Act
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            TenantName = "Test Tenant"
        };

        // Assert
        tenant.TenantId.Should().NotBeEmpty();
        tenant.TenantName.Should().Be("Test Tenant");
    }

    [Fact]
    public void Tenant_Should_Have_Default_Values()
    {
        // Arrange & Act
        var tenant = new Tenant();

        // Assert
        tenant.IsActive.Should().BeFalse();
        tenant.IsDeleted.Should().BeFalse();
        tenant.ActiveFrom.Should().Be(default);
    }

    [Fact]
    public void Tenant_Should_Have_Navigation_Properties()
    {
        // Arrange & Act
        var tenant = new Tenant();

        // Assert
        tenant.Facilities.Should().NotBeNull();
    }
}


