using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class RoleCreateRequestValidator : AbstractValidator<RoleCreateRequest>
{
    public RoleCreateRequestValidator()
    {
        RuleFor(x => x.Key).NotEmpty().WithMessage("권한 키는 필수 입력 항목입니다.");
        RuleFor(x => x.Name).NotEmpty().WithMessage("권한 명칭은 필수 입력 항목입니다.");
    }
}

public class RoleUpdateRequestValidator : AbstractValidator<RoleUpdateRequest>
{
    public RoleUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("권한 명칭은 필수 입력 항목입니다.");
    }
}

public class AssignMenusRequestValidator : AbstractValidator<AssignMenusRequest>
{
    public AssignMenusRequestValidator()
    {
        RuleFor(x => x.RoleKey).NotEmpty().WithMessage("권한 키가 없습니다.");
    }
}
