using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class UserCreateRequestValidator : AbstractValidator<UserCreateRequest>
{
    public UserCreateRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("아이디는 필수 입력 항목입니다.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("비밀번호는 필수 입력 항목입니다.")
            .MinimumLength(6).WithMessage("비밀번호는 최소 6자 이상이어야 합니다.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("이름은 필수 입력 항목입니다.");
        RuleFor(x => x.UserType).NotEmpty().WithMessage("사용자 유형은 필수 선택 항목입니다.");
    }
}

public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("이름은 필수 입력 항목입니다.");
        RuleFor(x => x.UserType).NotEmpty().WithMessage("사용자 유형은 필수 선택 항목입니다.");
    }
}
