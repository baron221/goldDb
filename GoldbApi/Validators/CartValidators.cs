using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class AddToCartDtoValidator : AbstractValidator<AddToCartDto>
{
    public AddToCartDtoValidator()
    {
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("수량은 1 이상이어야 합니다.");

        RuleFor(x => x)
            .Must(x => x.ProductId.HasValue || x.ProductSetId.HasValue)
            .WithMessage("제품 또는 제품 세트 정보가 필요합니다.");
    }
}
