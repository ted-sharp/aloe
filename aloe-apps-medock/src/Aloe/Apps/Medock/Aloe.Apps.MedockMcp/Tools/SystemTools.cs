using System.ComponentModel;
using System.Text.Json;
using Aloe.Apps.MedockLib.Data;
using Aloe.Apps.MedockLib.Services;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;

namespace Aloe.Apps.MedockMcp.Tools;

/// <summary>
/// システム確認ツール
/// </summary>
[McpServerToolType]
internal class SystemTools
{
    private readonly IUserContextService _userContextService;
    private readonly IDbContextFactory<MedockDbContext> _dbContextFactory;

    public SystemTools(
        IUserContextService userContextService,
        IDbContextFactory<MedockDbContext> dbContextFactory)
    {
        this._userContextService = userContextService;
        this._dbContextFactory = dbContextFactory;
    }

    [McpServerTool(Name = "GetCurrentContext")]
    [Description("現在のMCPサーバーのテナント・施設コンテキスト情報を確認します。")]
    public string GetCurrentContext()
    {
        var user = this._userContextService.CurrentUser;
        if (user == null)
            return JsonSerializer.Serialize(new { error = "コンテキストが初期化されていません" });

        return JsonSerializer.Serialize(new
        {
            userId = user.UserId,
            userCode = user.UserCode,
            tenantId = user.TenantId,
            tenantName = user.TenantName,
            facilityId = user.FacilityId,
            facilityName = user.FacilityName,
            isSystemAdmin = user.IsSystemAdmin,
            roles = user.Roles
        });
    }

    [McpServerTool(Name = "CheckDatabaseConnection")]
    [Description("データベースへの接続を確認します。")]
    public async Task<string> CheckDatabaseConnection()
    {
        try
        {
            await using var context = await this._dbContextFactory.CreateDbContextAsync();
            var canConnect = await context.Database.CanConnectAsync();

            if (!canConnect)
                return JsonSerializer.Serialize(new { status = "error", message = "データベースに接続できません" });

            var tenantCount = await context.Tenants.AsNoTracking().CountAsync(t => !t.IsDeleted);
            var facilityCount = await context.Facilities.AsNoTracking().CountAsync(f => !f.IsDeleted);

            return JsonSerializer.Serialize(new
            {
                status = "ok",
                tenantCount,
                facilityCount
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { status = "error", message = ex.Message });
        }
    }
}
