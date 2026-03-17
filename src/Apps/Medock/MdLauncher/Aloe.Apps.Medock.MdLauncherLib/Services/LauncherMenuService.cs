// <copyright file="LauncherMenuService.cs" company="ted-sharp">
// Copyright (c) ted-sharp. All rights reserved.
// </copyright>

using Aloe.Apps.Medock.MdLauncherLib.Data;
using Aloe.Apps.Medock.MdLauncherLib.Models;
using Microsoft.EntityFrameworkCore;

namespace Aloe.Apps.Medock.MdLauncherLib.Services;

/// <summary>
/// DB からユーザーのロールに基づいてランチャーメニューを取得するサービス。
/// </summary>
public class LauncherMenuService : ILauncherMenuService
{
    private readonly MdLauncherDbContext _dbContext;

    /// <summary>
    /// <see cref="LauncherMenuService"/> クラスの新しいインスタンスを初期化する。
    /// </summary>
    public LauncherMenuService(MdLauncherDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public async Task<List<MenuAppInfo>> GetMenusByUserCodeAsync(string userCode)
    {
        // user_code → users → facility_users → facility_user_roles
        //   → role_code → role_menus → menu_code → menus → app_code → apps
        var query =
            from u in _dbContext.Users
            where u.UserCode == userCode
            join fu in _dbContext.FacilityUsers on u.UserId equals fu.UserId
            join fur in _dbContext.FacilityUserRoles on fu.FacilityUserId equals fur.FacilityUserId
            join rm in _dbContext.RoleMenus on fur.RoleCode equals rm.RoleCode
            join m in _dbContext.Menus on rm.MenuCode equals m.MenuCode
            join a in _dbContext.Apps on m.AppCode equals a.AppCode
            orderby m.MenuSeq
            select new MenuAppInfo
            {
                MenuCode = m.MenuCode,
                MenuName = m.MenuName,
                MenuDesc = m.MenuDesc,
                MenuSeq = m.MenuSeq,
                AppCode = a.AppCode,
                AppName = a.AppName,
                AppDesc = a.AppDesc,
                AppPath = a.AppPath,
                AppArgs = a.AppArgs,
                AppDir = a.AppDir,
                AppIcon = a.AppIcon,
                PipeName = a.PipeName,
            };

        var results = await query.ToListAsync();

        // 複数ロール経由で同じメニューが重複する場合を排除
        return results
            .DistinctBy(x => x.MenuCode)
            .ToList();
    }
}
