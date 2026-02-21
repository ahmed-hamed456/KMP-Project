using FluentValidation;
using SearchService.Application.DTOs;

namespace SearchService.Application.Validators;

public class SearchRequestValidator : AbstractValidator<SearchRequestDto>
{
    public SearchRequestValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Search query is required")
            .MinimumLength(2)
            .WithMessage("Search query must be at least 2 characters")
            .MaximumLength(200)
            .WithMessage("Search query must not exceed 200 characters");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .Must(x => new[] { "relevance", "createdAt", "updatedAt", "title" }.Contains(x.ToLower()))
            .WithMessage("SortBy must be one of: relevance, createdAt, updatedAt, title");

        When(x => x.Filters != null, () =>
        {
            RuleFor(x => x.Filters!.FromDate)
                .LessThanOrEqualTo(x => x.Filters!.ToDate)
                .When(x => x.Filters!.FromDate.HasValue && x.Filters!.ToDate.HasValue)
                .WithMessage("FromDate must be before or equal to ToDate");
        });
    }
}
