using FluentValidation;

namespace ModulerERP.HR.Application.Features.Employees.Commands;

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(c => c.Dto.FirstName)
            .NotEmpty().WithMessage("First Name is required.")
            .MaximumLength(100);

        RuleFor(c => c.Dto.LastName)
            .NotEmpty().WithMessage("Last Name is required.")
            .MaximumLength(100);

        RuleFor(c => c.Dto.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(c => c.Dto.IdentityNumber)
            .NotEmpty().WithMessage("Identity Number is required.")
            .Length(11).When(c => c.Dto.Citizenship == Domain.Enums.CitizenshipType.TRNC).WithMessage("TRNC Identity Number must be 11 digits."); // Assumption

        RuleFor(c => c.Dto.CurrentSalary)
            .GreaterThan(0).WithMessage("Salary must be greater than zero.");
    }
}
