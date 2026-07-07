using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class CommonCodeSaveRequestValidator : AbstractValidator<CommonCodeSaveRequest>
{
    public CommonCodeSaveRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("코드 명칭은 필수 입력 항목입니다.");
        RuleFor(x => x.Code).NotEmpty().WithMessage("코드 값은 필수 입력 항목입니다.");
    }
}
