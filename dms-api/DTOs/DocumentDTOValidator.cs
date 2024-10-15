using FluentValidation;

namespace DocumentManagementSystem.DTOs
{
    public class DocumentDTOValidator : AbstractValidator<DocumentDTO>
    {
        public DocumentDTOValidator()
        {
            RuleFor(x => x.Name)
               .NotEmpty().WithMessage("The name cannot be empty.")
               .MaximumLength(100).WithMessage("The name must not exceed 100 characters.");

            RuleFor(x => x.IsComplete)
                .NotNull().WithMessage("The completion status must be specified.");
        }
    }
}
