using FluentValidation;
using GoldbApi.Models;

namespace GoldbApi.Validators;

public class ArticleValidator : AbstractValidator<Article>
{
    public ArticleValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("제목은 필수 입력 항목입니다.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("내용은 필수 입력 항목입니다.");
        RuleFor(x => x.Author).NotEmpty().WithMessage("작성자는 필수 입력 항목입니다.");
    }
}
