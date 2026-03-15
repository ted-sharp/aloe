namespace Aloe.Apps.MedockLib.Services;

/// <summary>
/// 認証結果
/// </summary>
public class AuthResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? RefreshTokenExpiration { get; init; }
    public Guid? SessionId { get; init; }
    public Guid? UserId { get; init; }
    public string? UserCode { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public Guid? TenantId { get; init; }
    public string? TenantName { get; init; }
    public Guid? FacilityId { get; init; }
    public string? FacilityName { get; init; }
    public bool IsSystemAdmin { get; init; }
    public bool IsFacilityAdmin { get; init; }
    public string[]? Roles { get; init; }

    public static AuthResult Success(
        DateTime refreshTokenExpiration,
        Guid? sessionId,
        Guid userId,
        string userCode,
        string email,
        string userDisplayName,
        Guid? tenantId,
        string? tenantName,
        Guid? facilityId,
        string? facilityName,
        bool isSystemAdmin,
        bool isFacilityAdmin,
        string[] roles)
    {
        return new AuthResult
        {
            IsSuccess = true,
            RefreshTokenExpiration = refreshTokenExpiration,
            SessionId = sessionId,
            UserId = userId,
            UserCode = userCode,
            Email = email,
            DisplayName = userDisplayName,
            TenantId = tenantId,
            TenantName = tenantName ?? "",
            FacilityId = facilityId,
            FacilityName = facilityName ?? "",
            IsSystemAdmin = isSystemAdmin,
            IsFacilityAdmin = isFacilityAdmin,
            Roles = roles
        };
    }

    public static AuthResult Failed(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// 施設情報
/// </summary>
public class FacilityInfo
{
    public Guid FacilityId { get; init; }
    public string FacilityName { get; init; } = "";
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = "";
    public bool IsFacilityAdmin { get; init; }
}

/// <summary>
/// セッション検証結果
/// </summary>
public class SessionValidationResult
{
    public bool IsValid { get; init; }
    public string? ErrorMessage { get; init; }

    private SessionValidationResult(bool isValid, string? errorMessage = null)
    {
        this.IsValid = isValid;
        this.ErrorMessage = errorMessage;
    }

    public static SessionValidationResult Valid()
    {
        return new SessionValidationResult(true);
    }

    public static SessionValidationResult Invalid(string errorMessage)
    {
        return new SessionValidationResult(false, errorMessage);
    }
}
