using FluentValidation;

namespace ModulerERP.CRM.Application.Features.Leads.Commands;

public class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");

        RuleFor(x => x.Phone)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone must not exceed 20 characters");

        RuleFor(x => x.Company)
            .MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Company));

        RuleFor(x => x.Source)
            .MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Source));
    }
}
