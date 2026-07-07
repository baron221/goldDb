using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("아이디를 입력해주세요.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("비밀번호를 입력해주세요.");
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("아이디는 필수 입력 항목입니다.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("비밀번호는 필수 입력 항목입니다.")
            .MinimumLength(6).WithMessage("비밀번호는 최소 6자 이상이어야 합니다.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("이름은 필수 입력 항목입니다.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("이메일은 필수 입력 항목입니다.")
            .EmailAddress().WithMessage("올바른 이메일 형식이 아닙니다.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("전화번호는 필수 입력 항목입니다.");
    }
}

public class FindIdRequestValidator : AbstractValidator<FindIdRequest>
{
    public FindIdRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("이름을 입력해주세요.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("이메일을 입력해주세요.")
            .EmailAddress().WithMessage("올바른 이메일 형식이 아닙니다.");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("아이디를 입력해주세요.");
        RuleFor(x => x.Email).NotEmpty().WithMessage("이메일을 입력해주세요.")
            .EmailAddress().WithMessage("올바른 이메일 형식이 아닙니다.");
    }
}

public class PasswordResetActionRequestValidator : AbstractValidator<PasswordResetActionRequest>
{
    public PasswordResetActionRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage("유효하지 않은 토큰입니다.");
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("새 비밀번호를 입력해주세요.")
            .MinimumLength(6).WithMessage("비밀번호는 최소 6자 이상이어야 합니다.");
    }
}
