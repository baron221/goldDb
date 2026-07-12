using System.ComponentModel;

namespace GoldbApi.Models;

[Description("사용자 정보")]
public class User : BaseModel
{

    [Description("사용자 ID")]
    public int Id { get; set; }

    [Description("로그인 계정")]
    public string Username { get; set; } = string.Empty;

    [Description("비밀번호")]
    public string Password { get; set; } = string.Empty;

    [Description("사용자 이름")]
    public string Name { get; set; } = string.Empty;

    [Description("주민등록번호")]
    public string? Ssn { get; set; }

    [Description("우편번호")]
    public string? ZipCode { get; set; }

    [Description("기본 주소")]
    public string? AddressBase { get; set; }

    [Description("상세 주소")]
    public string? AddressDetail { get; set; }

    [Description("아바타 이미지 URL")]
    public string Avatar { get; set; } = string.Empty;

    [Description("아바타 첨부파일 ID")]
    public int? AvatarId { get; set; }

    [Description("자기 소개")]
    public string Introduction { get; set; } = string.Empty;

    [Description("SMS 수신 동의 여부")]
    public bool SmsAllowed { get; set; } = false;

    [Description("카카오톡 수신 동의 여부")]
    public bool KakaoAllowed { get; set; } = false;

    [Description("이메일 수신 동의 여부")]
    public bool EmailAllowed { get; set; } = false;

    [Description("사용자 구분")]
    public string UserType { get; set; } = "ADMIN";

    [Description("비밀번호 재설정 토큰")]
    public string? ResetToken { get; set; }

    [Description("비밀번호 재설정 토큰 만료 시간")]
    public DateTime? ResetTokenExpires { get; set; }

    [Description("마지막 로그인 IP")]
    public string? LastLoginIp { get; set; }

    [Description("마지막 로그인 일시")]
    public DateTime? LastLoginAt { get; set; }

    [Description("로그인 횟수")]
    public int LoginCount { get; set; } = 0;

    public bool IsApproved { get; set; } = false;

    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    public ICollection<UserCompany> UserCompanies { get; set; } = new List<UserCompany>();

    public ICollection<UserPhoto> UserPhotos { get; set; } = new List<UserPhoto>();

    public ICollection<UserEmail> UserEmails { get; set; } = new List<UserEmail>();

    public ICollection<UserPhone> UserPhones { get; set; } = new List<UserPhone>();
}
