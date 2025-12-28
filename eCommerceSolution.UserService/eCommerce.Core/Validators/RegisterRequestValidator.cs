using eCommerce.Core.DTO;
using FluentValidation;


namespace eCommerce.Core.Validators;
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(temp => temp.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email address format");

        RuleFor(temp => temp.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long");

        RuleFor(temp => temp.PersonName)
            .NotEmpty().WithMessage("Person name is required")
            .Length(1, 50).WithMessage("Person name cannot exceed 15 characters");

        RuleFor(temp => temp.Gender)
            .IsInEnum().WithMessage("Invalid gender option");
    }
}

