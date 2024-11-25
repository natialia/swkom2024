using FluentValidation;

namespace DocumentManagementSystem.DTOs
{
    public class DocumentDTOValidator : AbstractValidator<DocumentDTO>
    {
        public DocumentDTOValidator()
        {
            RuleFor(x => x.Name)
               .NotEmpty().WithMessage("The name cannot be empty.")
               .MaximumLength(255).WithMessage("The name must not exceed 255 characters.")
               .Must(fileName => fileName == null || fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
               .WithMessage("Only PDF files are allowed.");
        }
    }
}
