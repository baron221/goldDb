using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
{
    public CreateOrderDtoValidator()
    {
        RuleFor(x => x.CartItemIds).NotEmpty().WithMessage("주문할 상품을 선택해주세요.");
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
