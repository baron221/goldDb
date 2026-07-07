using System.Security.Claims;

namespace GoldbApi.Services;

public interface ICurrentUserService
{

    int? UserId { get; }
    string? IpAddress { get; }

    IEnumerable<string> Roles { get; }

    bool IsAdmin { get; }

    int? CompanyId { get; }
}

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    public string? IpAddress
    {
        get
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? ipAddress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (string.IsNullOrEmpty(ipAddress))
                {
                    ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                }
                return ipAddress;
            }
            return null;
        }
    }

    public IEnumerable<string> Roles
    {
        get
        {
            var rolesClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return rolesClaim?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
        }
    }

    public bool IsAdmin => Roles.Any(r => r.ToLower() == "admin");

    public int? CompanyId
    {
        get
        {
            var companyIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("CompanyId")?.Value;
            if (int.TryParse(companyIdClaim, out var companyId))
            {
                return companyId;
            }
            return null;
        }
    }
}
