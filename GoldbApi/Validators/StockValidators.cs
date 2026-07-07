using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class CreateStockDtoValidator : AbstractValidator<CreateStockDto>
{
    public CreateStockDtoValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0).WithMessage("유효하지 않은 제품 ID입니다.")
            .When(x => x.ProductId.HasValue);
    }
}

public class UpdateStockDtoValidator : AbstractValidator<UpdateStockDto>
{
    public UpdateStockDtoValidator()
    {
        RuleFor(x => x.Status).NotEmpty().WithMessage("재고 상태를 입력해주세요.");
    }
}
