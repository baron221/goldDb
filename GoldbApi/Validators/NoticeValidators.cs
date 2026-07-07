using FluentValidation;
using GoldbApi.DTOs;

namespace GoldbApi.Validators;

public class CreateNoticeDtoValidator : AbstractValidator<CreateNoticeDto>
{
    public CreateNoticeDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("제목은 필수 입력 항목입니다.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("내용은 필수 입력 항목입니다.");
    }
}

public class UpdateNoticeDtoValidator : AbstractValidator<UpdateNoticeDto>
{
    public UpdateNoticeDtoValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("유효하지 않은 공지사항 ID입니다.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("제목은 필수 입력 항목입니다.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("내용은 필수 입력 항목입니다.");
    }
}
