namespace GoldbApi.DTOs;

public class ApiResponse<T>
{

    public int Code { get; set; } = 20000;

    public T? Data { get; set; }

    public string Message { get; set; } = "success";

    public static ApiResponse<T> Success(T data) => new ApiResponse<T> { Data = data };

    public static ApiResponse<T> Failure(string message, int code = 60204) => new ApiResponse<T> { Code = code, Message = message, Data = default };
}

public class PagedResult<T>
{

    public List<T> Items { get; set; } = new();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}

public class LoginRequest
{

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{

    public string Token { get; set; } = string.Empty;
}

public class UserInfoResponse
{

    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string[] Roles { get; set; } = Array.Empty<string>();

    public string Name { get; set; } = string.Empty;

    public string Avatar { get; set; } = string.Empty;

    public string Introduction { get; set; } = string.Empty;

    public int? CompanyId { get; set; }

    public string? CompanyType { get; set; }

    public string? CompanyName { get; set; }

    public int? LogisticsCompanyId { get; set; }

    public string? LastLoginIp { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public int LoginCount { get; set; }
}

public class UserListItemResponse
{

    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public string UserType { get; set; } = "ADMIN";

    public string? CompanyName { get; set; }

    public string? CompanyCategory { get; set; }

    public string? LogisticsCompanyName { get; set; }

    public bool IsDirectManagement { get; set; }

    public bool IsApproved { get; set; }

    public string? LastLoginIp { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public int LoginCount { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class UserDetailResponse
{

    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Ssn { get; set; }

    public string? ZipCode { get; set; }

    public string? AddressBase { get; set; }

    public string? AddressDetail { get; set; }

    public string? Avatar { get; set; }

    public int? AvatarId { get; set; }

    public string? Introduction { get; set; }

    public bool SmsAllowed { get; set; }

    public bool KakaoAllowed { get; set; }

    public bool EmailAllowed { get; set; }

    public string UserType { get; set; } = "ADMIN";

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public string? CompanyCategory { get; set; }

    public string? LogisticsCompanyName { get; set; }

    public string? LastLoginIp { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public int LoginCount { get; set; }

    public List<UserEmailDto> Emails { get; set; } = new();

    public List<UserPhoneDto> Phones { get; set; } = new();

    public List<UserPhotoDto> Photos { get; set; } = new();

    public List<string> Roles { get; set; } = new();
}

public class UserCreateRequest
{

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string UserType { get; set; } = "ADMIN";

    public int? CompanyId { get; set; }
}

public class UserUpdateRequest
{

    public string Name { get; set; } = string.Empty;

    public string? Ssn { get; set; }

    public string? ZipCode { get; set; }

    public string? AddressBase { get; set; }

    public string? AddressDetail { get; set; }

    public string? Avatar { get; set; }

    public int? AvatarId { get; set; }

    public string? Introduction { get; set; }

    public bool SmsAllowed { get; set; }

    public bool KakaoAllowed { get; set; }

    public bool EmailAllowed { get; set; }

    public string UserType { get; set; } = "ADMIN";

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public List<UserEmailDto> Emails { get; set; } = new();

    public List<UserPhoneDto> Phones { get; set; } = new();

    public List<UserPhotoDto> Photos { get; set; } = new();

    public List<string> Roles { get; set; } = new();
}

public class UserEmailDto
{

    public string Email { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}

public class UserPhoneDto
{

    public string PhoneNumber { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
}

public class UserPhotoDto
{

    public string PhotoUrl { get; set; } = string.Empty;

    public int? AttachmentId { get; set; }

    public int SortOrder { get; set; }
}

public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Ssn { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string AddressBase { get; set; } = string.Empty;
    public string AddressDetail { get; set; } = string.Empty;
    
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyBusinessNumber { get; set; } = string.Empty;
    public string CompanyBusinessType { get; set; } = string.Empty;
    public string CompanyBusinessCategory { get; set; } = string.Empty;
    public string CompanyPhone { get; set; } = string.Empty;
    public string CompanyZipCode { get; set; } = string.Empty;
    public string CompanyAddressBase { get; set; } = string.Empty;
    public string CompanyAddressDetail { get; set; } = string.Empty;

    public string UserType { get; set; } = "RETAIL";
    public string LogisticsCode { get; set; } = string.Empty;
    public bool SmsAllowed { get; set; } = true;
    public string Introduction { get; set; } = string.Empty;
}

public class FindIdRequest
{

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

public class PasswordResetActionRequest
{

    public string Token { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}

public class DevUserItem
{

    public string Username { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
}

public class DevUserListResponse
{

    public List<DevUserItem> Retail { get; set; } = new();

    public List<DevUserItem> Logistics { get; set; } = new();

    public List<DevUserItem> Manufacturer { get; set; } = new();
}

public class UserPersonalSettingDto
{

    public string Size { get; set; } = "s12";

    public bool TagsView { get; set; } = true;

    public bool FixedHeader { get; set; } = true;

    public bool SidebarLogo { get; set; } = true;

    public bool SecondMenuPopup { get; set; } = false;
}
