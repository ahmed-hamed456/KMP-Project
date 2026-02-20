using FluentValidation;
using DocumentManagement.Application.DTOs;

namespace DocumentManagement.Application.Validators;

public class CreateDocumentValidator : AbstractValidator<CreateDocumentDto>
{
    public CreateDocumentValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");
        
        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
        
        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required")
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters");
        
        RuleFor(x => x.Tags)
            .NotNull().WithMessage("Tags cannot be null");
        
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("DepartmentId is required");
    }
}
