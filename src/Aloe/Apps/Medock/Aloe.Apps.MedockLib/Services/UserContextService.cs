using System.Security.Claims;

namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// ユーザーコンテキストを管理するサービス
/// </summary>
/// <remarks>
/// JWTトークンのクレームからユーザー情報を抽出し、
/// コンポーネント間で共有可能な形式で保持します。
/// </remarks>
public class UserContextService
{
    private readonly AuthService _authService;

    public UserContextService(AuthService authService)
    {
        this._authService = authService;
    }

    /// <summary>
    /// 現在のユーザー情報
    /// </summary>
    public UserContextInfo? CurrentUser { get; private set; }

    /// <summary>
    /// 現在のセッションID
    /// </summary>
    public Guid? CurrentSessionId { get; private set; }

    /// <summary>
    /// JWTクレームからユーザー情報を初期化します。
    /// </summary>
    public void InitializeFromClaims(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            this.CurrentUser = null;
            this.CurrentSessionId = null;
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

        // セッションIDを取得（JWTトークンに含まれている場合）
        if (Guid.TryParse(principal.FindFirst("session_id")?.Value, out var sessionId))
        {
            this.CurrentSessionId = sessionId;
        }
        else
        {
            this.CurrentSessionId = null;
        }
    }

    /// <summary>
    /// セッションIDを設定します（ログイン時に呼び出し）。
    /// </summary>
    public void SetSessionId(Guid sessionId)
    {
        this.CurrentSessionId = sessionId;
    }

    /// <summary>
    /// 切替可能な施設一覧を取得します（DBアクセス）。
    /// </summary>
    public async Task<List<FacilityInfo>> GetAccessibleFacilitiesAsync()
    {
        if (this.CurrentUser == null || this.CurrentUser.UserId == Guid.Empty)
        {
            return [];
        }

        return await this._authService.GetAccessibleFacilitiesAsync(this.CurrentUser.UserId);
    }

    /// <summary>
    /// ユーザーが複数の施設にアクセス可能かどうかを取得します。
    /// </summary>
    public async Task<bool> HasMultipleFacilitiesAsync()
    {
        var facilities = await this.GetAccessibleFacilitiesAsync();
        return facilities.Count > 1;
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
