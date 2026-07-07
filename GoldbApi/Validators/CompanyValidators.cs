using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class CompanyCreateRequestValidator : AbstractValidator<CompanyCreateRequest>
{
    public CompanyCreateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("상호명은 필수 입력 항목입니다.");
        RuleFor(x => x.CEO).NotEmpty().WithMessage("대표자명은 필수 입력 항목입니다.");
        RuleFor(x => x.Category).NotEmpty().WithMessage("업체구분은 필수 선택 항목입니다.");
        RuleFor(x => x.Region).NotEmpty().WithMessage("지역은 필수 선택 항목입니다.");
    }
}

public class CompanyUpdateRequestValidator : AbstractValidator<CompanyUpdateRequest>
{
    public CompanyUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("상호명은 필수 입력 항목입니다.");
        RuleFor(x => x.CEO).NotEmpty().WithMessage("대표자명은 필수 입력 항목입니다.");
        RuleFor(x => x.Category).NotEmpty().WithMessage("업체구분은 필수 선택 항목입니다.");
        RuleFor(x => x.Region).NotEmpty().WithMessage("지역은 필수 선택 항목입니다.");
    }
}
