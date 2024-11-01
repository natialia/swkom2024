using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dms_bl.Models;
using FluentValidation;

namespace dms_bl.Validators
{
    public class DocumentValidator: AbstractValidator<Document>
    {
        public DocumentValidator()
        {
            RuleFor(x => x.Name)
               .NotEmpty().WithMessage("The name cannot be empty.")
               .MaximumLength(100).WithMessage("The name must not exceed 100 characters.");
            RuleFor(x => x.FileType)
                .NotEmpty().WithMessage("Must have a filetype.");
            RuleFor(x => x.FileSize)
                .NotEmpty().WithMessage("Must have a file size.");
            RuleFor(x => x.FileSize)
                .Must(IsAValidSize).WithMessage("File size too big (>10 MB).");
        }

        private bool IsAValidSize(string fileSize)
        {
            if (int.TryParse(fileSize.Replace("kB", ""), out int sizeInKb))
            {
                return sizeInKb <= 10000; // Maximum of 10 MB (should be enough)
            }

            return false;
        }
    }
}
