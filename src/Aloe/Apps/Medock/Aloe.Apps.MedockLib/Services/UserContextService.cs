using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// ユーザーコンテキストを管理するサービス
/// </summary>
/// <remarks>
/// 認証クレームからユーザー情報を抽出し、
/// コンポーネント間で共有可能な形式で保持します。
/// </remarks>
public class UserContextService : IUserContextService
{
    private readonly IAuthService _authService;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(IAuthService authService, ILogger<UserContextService> logger)
    {
        this._authService = authService;
        this._logger = logger;
    }

    /// <inheritdoc />
    public UserContextInfo? CurrentUser { get; private set; }

    /// <inheritdoc />
    public Guid? CurrentSessionId { get; private set; }

    /// <summary>
    /// キャッシュされたアクセス可能な施設一覧
    /// </summary>
    private List<FacilityInfo>? _cachedFacilities;

    /// <inheritdoc />
    public void InitializeFromClaims(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            this.CurrentUser = null;
            this.CurrentSessionId = null;
            this._cachedFacilities = null;
            return;
        }

        this.CurrentUser = new UserContextInfo
        {
            UserId = Guid.TryParse(principal.FindFirst("sub")?.Value, out var userId) ? userId : Guid.Empty,
            UserCode = principal.FindFirst("preferred_username")?.Value ?? "",
            Email = principal.FindFirst("email")?.Value ?? "",
            UserDisplayName = principal.FindFirst("user_display_name")?.Value ?? "",
            TenantId = Guid.TryParse(principal.FindFirst("tenant_id")?.Value, out var tenantId) ? tenantId : null,
            TenantName = principal.FindFirst("tenant_name")?.Value ?? "",
            FacilityId = Guid.TryParse(principal.FindFirst("facility_id")?.Value, out var facilityId) ? facilityId : null,
            FacilityName = principal.FindFirst("facility_name")?.Value ?? "",
            IsSystemAdmin = Boolean.TryParse(principal.FindFirst("is_system_admin")?.Value, out var isSysAdmin) && isSysAdmin,
            IsFacilityAdmin = Boolean.TryParse(principal.FindFirst("is_facility_admin")?.Value, out var isFacilityAdmin) && isFacilityAdmin,
            Roles = principal.FindAll("roles").Select(c => c.Value).ToList()
        };

        // セッションIDを取得（クレームに含まれている場合）
        if (Guid.TryParse(principal.FindFirst("session_id")?.Value, out var sessionId))
        {
            this.CurrentSessionId = sessionId;
        }
        else
        {
            this.CurrentSessionId = null;
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// ログインスキップ（前回のセッションが維持されている場合）でも正しく動作するよう、
    /// Cookieにfacility_idが含まれていない場合は、アクセス可能な施設からデフォルトを取得します。
    /// </remarks>
    public async Task<bool> InitializeFromClaimsAsync(ClaimsPrincipal principal)
    {
        // まず同期版でクレームから初期化
        this.InitializeFromClaims(principal);

        // 認証されていない場合は再ログインが必要
        if (this.CurrentUser == null || this.CurrentUser.UserId == Guid.Empty)
        {
            this._logger.LogWarning("User not authenticated, login required");
            this._cachedFacilities = null;
            return false;
        }

        // アクセス可能な施設一覧を取得してキャッシュ
        this._cachedFacilities = await this._authService.GetAccessibleFacilitiesAsync(this.CurrentUser.UserId);

        // アクセス可能な施設がない場合は再ログインが必要
        if (!this._cachedFacilities.Any())
        {
            this._logger.LogWarning("No accessible facilities found for UserId={UserId}, login required", this.CurrentUser.UserId);
            this.CurrentUser = null;
            this.CurrentSessionId = null;
            this._cachedFacilities = null;
            return false;
        }

        // FacilityIdがない場合のフォールバック処理（ログインスキップ時にも対応）
        if (!this.CurrentUser.FacilityId.HasValue)
        {
            this._logger.LogInformation("FacilityId not found in claims, attempting fallback for UserId={UserId}", this.CurrentUser.UserId);
            var defaultFacility = this._cachedFacilities.First();
            this._logger.LogInformation("Fallback facility found: FacilityId={FacilityId}, FacilityName={FacilityName}",
                defaultFacility.FacilityId, defaultFacility.FacilityName);
            this.CurrentUser = this.CurrentUser with
            {
                FacilityId = defaultFacility.FacilityId,
                FacilityName = defaultFacility.FacilityName,
                TenantId = defaultFacility.TenantId,
                TenantName = defaultFacility.TenantName
            };
            return true;
        }

        // FacilityIdがあるが、アクセス可能な施設に含まれていない場合は再ログインが必要
        var facilityExists = this._cachedFacilities.Any(f => f.FacilityId == this.CurrentUser.FacilityId.Value);
        if (!facilityExists)
        {
            this._logger.LogWarning("FacilityId={FacilityId} from claims is not in accessible facilities for UserId={UserId}, login required",
                this.CurrentUser.FacilityId, this.CurrentUser.UserId);
            this.CurrentUser = null;
            this.CurrentSessionId = null;
            this._cachedFacilities = null;
            return false;
        }

        this._logger.LogInformation("FacilityId found in claims and validated: FacilityId={FacilityId}", this.CurrentUser.FacilityId);
        return true;
    }

    /// <inheritdoc />
    public void SetSessionId(Guid sessionId)
    {
        this.CurrentSessionId = sessionId;
    }

    /// <inheritdoc />
    public Task<List<FacilityInfo>> GetAccessibleFacilitiesAsync()
    {
        if (this.CurrentUser == null || this.CurrentUser.UserId == Guid.Empty)
        {
            return Task.FromResult<List<FacilityInfo>>([]);
        }

        // キャッシュがあればそれを返す（InitializeFromClaimsAsyncで取得済み）
        if (this._cachedFacilities != null)
        {
            return Task.FromResult(this._cachedFacilities);
        }

        // キャッシュがない場合はDBから取得（通常はInitializeFromClaimsAsync後に呼ばれるので、ここには来ない）
        return this._authService.GetAccessibleFacilitiesAsync(this.CurrentUser.UserId);
    }

    /// <inheritdoc />
    public async Task<bool> HasMultipleFacilitiesAsync()
    {
        // キャッシュがあればそれを使用
        if (this._cachedFacilities != null)
        {
            return this._cachedFacilities.Count > 1;
        }

        var facilities = await this.GetAccessibleFacilitiesAsync();
        return facilities.Count > 1;
    }

    /// <inheritdoc />
    public (Guid? TenantId, Guid? FacilityId, Guid? UserId) GetTenantContext()
    {
        if (this.CurrentUser == null)
        {
            return (null, null, null);
        }

        return (this.CurrentUser.TenantId, this.CurrentUser.FacilityId, this.CurrentUser.UserId);
    }
}

/// <summary>
/// ユーザーコンテキスト情報
/// </summary>
public record UserContextInfo
{
    public Guid UserId { get; init; }
    public string UserCode { get; init; } = "";
    public string Email { get; init; } = "";
    public string UserDisplayName { get; init; } = "";
    public Guid? TenantId { get; init; }
    public string TenantName { get; init; } = "";
    public Guid? FacilityId { get; init; }
    public string FacilityName { get; init; } = "";
    public bool IsSystemAdmin { get; init; }
    public bool IsFacilityAdmin { get; init; }
    public List<string> Roles { get; init; } = [];

    /// <summary>
    /// ユーザーのイニシャルを取得します（Avatar表示用）。
    /// </summary>
    public string Initial => String.IsNullOrEmpty(this.UserDisplayName)
        ? (String.IsNullOrEmpty(this.UserCode) ? "?" : this.UserCode[..1].ToUpper())
        : this.UserDisplayName[..1].ToUpper();
}
