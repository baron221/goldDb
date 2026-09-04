using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        // A direct/manual order can target a real catalog product (DirectProductId/
        // DirectProductSetId) OR a free-text one that isn't in the catalog at all
        // (DirectProductName - see OrderManualRegisterDialog) - CartItemIds is only
        // required when NONE of those three are present.
        RuleFor(x => x.CartItemIds)
            .NotEmpty()
            .When(x => !x.DirectProductId.HasValue && !x.DirectProductSetId.HasValue && string.IsNullOrEmpty(x.DirectProductName))
            .WithMessage("주문할 상품을 선택해주세요.");
    }
}

public class UpdateOrderStatusDtoValidator : AbstractValidator<UpdateOrderStatusDto>
{
    public UpdateOrderStatusDtoValidator()
    {
        RuleFor(x => x.Status).NotEmpty().WithMessage("변경할 상태 코드를 입력해주세요.");
    }
}

public class SaveOrderStatementDtoValidator : AbstractValidator<SaveOrderStatementDto>
{
    public SaveOrderStatementDtoValidator()
    {
        RuleFor(x => x.SnapshotData).NotEmpty().WithMessage("명세서 스냅샷 데이터가 없습니다.");
    }
}
