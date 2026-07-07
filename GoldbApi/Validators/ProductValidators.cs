using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("제품명은 필수 입력 항목입니다.");
        RuleFor(x => x.Purity).NotEmpty().WithMessage("함량은 필수 선택 항목입니다.");
        RuleFor(x => x.CategoryLarge).NotEmpty().WithMessage("대분류는 필수 선택 항목입니다.");
        RuleFor(x => x.Colors).NotEmpty().WithMessage("제품 컬러는 필수 선택 항목입니다.");
        RuleFor(x => x.FactoryPrice).GreaterThanOrEqualTo(0).WithMessage("공장도가는 0 이상이어야 합니다.");
        RuleFor(x => x.Weight).GreaterThanOrEqualTo(0).WithMessage("기준 중량은 0 이상이어야 합니다.");
    }
}
