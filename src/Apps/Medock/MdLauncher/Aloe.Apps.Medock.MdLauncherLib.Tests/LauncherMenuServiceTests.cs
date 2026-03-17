// <copyright file="LauncherMenuServiceTests.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Data.Entities;
using Aloe.Apps.Medock.MdLauncherLib.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdLauncherLib.Tests;

public class LauncherMenuServiceTests : IDisposable
{
    private readonly MdLauncherDbContext _dbContext;
    private readonly LauncherMenuService _service;

    public LauncherMenuServiceTests()
    {
        var options = new DbContextOptionsBuilder<MdLauncherDbContext>()
            .UseInMemoryDatabase(databaseName: $"MdLauncherTest_{Guid.NewGuid():N}")
            .Options;

        _dbContext = new MdLauncherDbContext(options);
        _service = new LauncherMenuService(_dbContext);
    }

    [Fact]
    public async Task GetMenusByUserCodeAsync_FullJoinChain_ReturnsMenus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var facilityUserId = Guid.NewGuid();

        _dbContext.Users.Add(new UserEntity { UserId = userId, UserCode = "U001" });
        _dbContext.FacilityUsers.Add(new FacilityUserEntity { FacilityUserId = facilityUserId, FacilityId = Guid.NewGuid(), UserId = userId });
        _dbContext.FacilityUserRoles.Add(new FacilityUserRoleEntity { FacilityUserRoleId = Guid.NewGuid(), FacilityUserId = facilityUserId, RoleCode = "ADMIN" });
        _dbContext.RoleMenus.Add(new RoleMenuEntity { RoleMenuId = Guid.NewGuid(), RoleCode = "ADMIN", MenuCode = "M001" });
        _dbContext.Apps.Add(new AppEntity { AppCode = "APP1", AppName = "テストアプリ", AppDesc = "説明", AppPath = "C:/test.exe", IsActive = true });
        _dbContext.Menus.Add(new MenuEntity { MenuCode = "M001", MenuName = "患者検索", MenuDesc = "患者マスター検索", MenuSeq = 1, AppCode = "APP1", IsActive = true });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetMenusByUserCodeAsync("U001");

        // Assert
        result.Should().HaveCount(1);
        result[0].MenuCode.Should().Be("M001");
        result[0].MenuName.Should().Be("患者検索");
        result[0].AppCode.Should().Be("APP1");
        result[0].AppName.Should().Be("テストアプリ");
        result[0].AppPath.Should().Be("C:/test.exe");
    }

    [Fact]
    public async Task GetMenusByUserCodeAsync_NonExistentUser_ReturnsEmptyList()
    {
        // Act
        var result = await _service.GetMenusByUserCodeAsync("NOUSER");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMenusByUserCodeAsync_InactiveApp_ExcludedByFilter()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var facilityUserId = Guid.NewGuid();

        _dbContext.Users.Add(new UserEntity { UserId = userId, UserCode = "U002" });
        _dbContext.FacilityUsers.Add(new FacilityUserEntity { FacilityUserId = facilityUserId, FacilityId = Guid.NewGuid(), UserId = userId });
        _dbContext.FacilityUserRoles.Add(new FacilityUserRoleEntity { FacilityUserRoleId = Guid.NewGuid(), FacilityUserId = facilityUserId, RoleCode = "USER" });
        _dbContext.RoleMenus.Add(new RoleMenuEntity { RoleMenuId = Guid.NewGuid(), RoleCode = "USER", MenuCode = "M002" });
        _dbContext.Apps.Add(new AppEntity { AppCode = "APP2", AppName = "無効アプリ", IsActive = false });
        _dbContext.Menus.Add(new MenuEntity { MenuCode = "M002", MenuName = "無効メニュー", MenuSeq = 1, AppCode = "APP2", IsActive = true });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetMenusByUserCodeAsync("U002");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMenusByUserCodeAsync_InactiveMenu_ExcludedByFilter()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var facilityUserId = Guid.NewGuid();

        _dbContext.Users.Add(new UserEntity { UserId = userId, UserCode = "U003" });
        _dbContext.FacilityUsers.Add(new FacilityUserEntity { FacilityUserId = facilityUserId, FacilityId = Guid.NewGuid(), UserId = userId });
        _dbContext.FacilityUserRoles.Add(new FacilityUserRoleEntity { FacilityUserRoleId = Guid.NewGuid(), FacilityUserId = facilityUserId, RoleCode = "USER" });
        _dbContext.RoleMenus.Add(new RoleMenuEntity { RoleMenuId = Guid.NewGuid(), RoleCode = "USER", MenuCode = "M003" });
        _dbContext.Apps.Add(new AppEntity { AppCode = "APP3", AppName = "有効アプリ", IsActive = true });
        _dbContext.Menus.Add(new MenuEntity { MenuCode = "M003", MenuName = "無効メニュー", MenuSeq = 1, AppCode = "APP3", IsActive = false });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetMenusByUserCodeAsync("U003");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMenusByUserCodeAsync_DeletedUser_ExcludedByFilter()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var facilityUserId = Guid.NewGuid();

        _dbContext.Users.Add(new UserEntity { UserId = userId, UserCode = "U004", IsDeleted = true });
        _dbContext.FacilityUsers.Add(new FacilityUserEntity { FacilityUserId = facilityUserId, FacilityId = Guid.NewGuid(), UserId = userId });
        _dbContext.FacilityUserRoles.Add(new FacilityUserRoleEntity { FacilityUserRoleId = Guid.NewGuid(), FacilityUserId = facilityUserId, RoleCode = "ADMIN" });
        _dbContext.RoleMenus.Add(new RoleMenuEntity { RoleMenuId = Guid.NewGuid(), RoleCode = "ADMIN", MenuCode = "M004" });
        _dbContext.Apps.Add(new AppEntity { AppCode = "APP4", AppName = "アプリ4", IsActive = true });
        _dbContext.Menus.Add(new MenuEntity { MenuCode = "M004", MenuName = "メニュー4", MenuSeq = 1, AppCode = "APP4", IsActive = true });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetMenusByUserCodeAsync("U004");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMenusByUserCodeAsync_SortedByMenuSeq()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var facilityUserId = Guid.NewGuid();

        _dbContext.Users.Add(new UserEntity { UserId = userId, UserCode = "U005" });
        _dbContext.FacilityUsers.Add(new FacilityUserEntity { FacilityUserId = facilityUserId, FacilityId = Guid.NewGuid(), UserId = userId });
        _dbContext.FacilityUserRoles.Add(new FacilityUserRoleEntity { FacilityUserRoleId = Guid.NewGuid(), FacilityUserId = facilityUserId, RoleCode = "ADMIN" });
        _dbContext.RoleMenus.Add(new RoleMenuEntity { RoleMenuId = Guid.NewGuid(), RoleCode = "ADMIN", MenuCode = "M010" });
        _dbContext.RoleMenus.Add(new RoleMenuEntity { RoleMenuId = Guid.NewGuid(), RoleCode = "ADMIN", MenuCode = "M011" });
        _dbContext.Apps.Add(new AppEntity { AppCode = "APP10", AppName = "アプリ10", IsActive = true });
        _dbContext.Apps.Add(new AppEntity { AppCode = "APP11", AppName = "アプリ11", IsActive = true });
        _dbContext.Menus.Add(new MenuEntity { MenuCode = "M010", MenuName = "後のメニュー", MenuSeq = 2, AppCode = "APP10", IsActive = true });
        _dbContext.Menus.Add(new MenuEntity { MenuCode = "M011", MenuName = "先のメニュー", MenuSeq = 1, AppCode = "APP11", IsActive = true });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetMenusByUserCodeAsync("U005");

        // Assert
        result.Should().HaveCount(2);
        result[0].MenuCode.Should().Be("M011");
        result[1].MenuCode.Should().Be("M010");
    }

    [Fact]
    public async Task GetMenusByUserCodeAsync_DuplicateMenuFromMultipleRoles_Deduplicated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var facilityUserId = Guid.NewGuid();

        _dbContext.Users.Add(new UserEntity { UserId = userId, UserCode = "U006" });
        _dbContext.FacilityUsers.Add(new FacilityUserEntity { FacilityUserId = facilityUserId, FacilityId = Guid.NewGuid(), UserId = userId });
        _dbContext.FacilityUserRoles.Add(new FacilityUserRoleEntity { FacilityUserRoleId = Guid.NewGuid(), FacilityUserId = facilityUserId, RoleCode = "ADMIN" });
        _dbContext.FacilityUserRoles.Add(new FacilityUserRoleEntity { FacilityUserRoleId = Guid.NewGuid(), FacilityUserId = facilityUserId, RoleCode = "USER" });
        _dbContext.RoleMenus.Add(new RoleMenuEntity { RoleMenuId = Guid.NewGuid(), RoleCode = "ADMIN", MenuCode = "M020" });
        _dbContext.RoleMenus.Add(new RoleMenuEntity { RoleMenuId = Guid.NewGuid(), RoleCode = "USER", MenuCode = "M020" });
        _dbContext.Apps.Add(new AppEntity { AppCode = "APP20", AppName = "共有アプリ", IsActive = true });
        _dbContext.Menus.Add(new MenuEntity { MenuCode = "M020", MenuName = "共有メニュー", MenuSeq = 1, AppCode = "APP20", IsActive = true });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.GetMenusByUserCodeAsync("U006");

        // Assert
        result.Should().HaveCount(1);
        result[0].MenuCode.Should().Be("M020");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
