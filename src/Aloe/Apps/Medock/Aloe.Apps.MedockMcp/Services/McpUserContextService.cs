using System.Security.Claims;
using Aloe.Apps.MedockLib.Common;
using Aloe.Apps.MedockLib.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aloe.Apps.MedockMcp.Services;

/// <summary>
/// MCP用のユーザーコンテキストサービス実装
/// 起動時にDBログインを行い、セッションIDを取得する
/// </summary>
public class McpUserContextService : IUserContextService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private UserContextInfo? _currentUser;
    private Guid? _sessionId;
    private bool _initialized;

    public McpUserContextService(
        IConfiguration configuration,
        IServiceScopeFactory serviceScopeFactory)
    {
        this._configuration = configuration;
        this._serviceScopeFactory = serviceScopeFactory;
    }

    public UserContextInfo? CurrentUser
    {
        get
        {
            this.EnsureInitialized();
            return this._currentUser;
        }
    }

    public Guid? CurrentSessionId
    {
        get
        {
            this.EnsureInitialized();
            return this._sessionId;
        }
    }

    public void InitializeFromClaims(ClaimsPrincipal principal)
    {
        // MCPでは未サポート
    }

    public Task<bool> InitializeFromClaimsAsync(ClaimsPrincipal principal)
    {
        // MCPでは未サポート
        return Task.FromResult(false);
    }

    public void SetSessionId(Guid sessionId)
    {
        // MCPでは未サポート
    }

    public Task<List<FacilityInfo>> GetAccessibleFacilitiesAsync()
    {
        return Task.FromResult<List<FacilityInfo>>([]);
    }

    public Task<bool> HasMultipleFacilitiesAsync()
    {
        return Task.FromResult(false);
    }

    public (Guid? TenantId, Guid? FacilityId, Guid? UserId) GetTenantContext()
    {
        this.EnsureInitialized();
        return (this._currentUser?.TenantId, this._currentUser?.FacilityId, this._currentUser?.UserId);
    }

    public TenantContext GetTenantContextValue()
    {
        this.EnsureInitialized();
        if (this._currentUser == null)
        {
            return new TenantContext(null, null, null);
        }
        return new TenantContext(this._currentUser.TenantId, this._currentUser.FacilityId, this._currentUser.UserId);
    }

    private void EnsureInitialized()
    {
        if (this._initialized) return;
        this._initialized = true;

        var userCode = this._configuration["McpAuth:UserCode"] ?? "mcp-system";
        var password = this._configuration["McpAuth:Password"] ?? "";

        using var scope = this._serviceScopeFactory.CreateScope();
        var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
        var result = authService.LoginAsync(userCode, password, "MedockMcp", "", "")
            .GetAwaiter().GetResult();

        if (!result.IsSuccess)
            throw new InvalidOperationException($"MCP認証失敗: {result.ErrorMessage}");

        this._sessionId = result.SessionId;
        this._currentUser = new UserContextInfo
        {
            UserId = result.UserId!.Value,
            UserCode = result.UserCode!,
            Email = result.Email ?? "",
            UserDisplayName = result.DisplayName ?? "MCPシステム",
            TenantId = result.TenantId,
            FacilityId = result.FacilityId,
            IsSystemAdmin = result.IsSystemAdmin,
            Roles = result.Roles?.ToList() ?? []
        };
    }
}
