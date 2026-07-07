using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class CustomerCreateRequestValidator : AbstractValidator<CustomerCreateRequest>
{
    public CustomerCreateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("이름은 필수 입력 항목입니다.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("연락처는 필수 입력 항목입니다.");
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("올바른 이메일 형식이 아닙니다.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}

public class CustomerUpdateRequestValidator : AbstractValidator<CustomerUpdateRequest>
{
    public CustomerUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("이름은 필수 입력 항목입니다.");
        RuleFor(x => x.Phone).NotEmpty().WithMessage("연락처는 필수 입력 항목입니다.");
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("올바른 이메일 형식이 아닙니다.")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }
}
