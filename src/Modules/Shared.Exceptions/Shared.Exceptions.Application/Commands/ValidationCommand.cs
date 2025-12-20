using ErrorOr;
using FluentValidation;
using MediatR;

namespace Shared.Exceptions.Application.Commands;

/// <summary>
/// Command for testing FluentValidation functionality.
/// Demonstrates validation of 3 fields with 4 validation rules:
/// - Name: required (1 rule)
/// - Email: required + must be valid email format (2 rules)
/// - Age: required + must be >= 18 (2 rules)
/// Total: 4 validation rules for 3 properties
/// </summary>
public record ValidationCommand(
    string Name,
    string Email,
    int Age) : IRequest<ErrorOr<string>>;

/// <summary>
/// Validator for ValidationCommand using FluentValidation.
/// Demonstrates multiple validation rules with 4 total validations for 3 properties.
/// </summary>
public class ValidationCommandValidator : AbstractValidator<ValidationCommand>
{
    /// <summary>
    /// Test validation rules for ValidationCommand.
    /// </summary>
    public ValidationCommandValidator()
    {
        // Name validation - 1 rule
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required");

        // Email validation - 2 rules
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required");

        RuleFor(x => x.Email)
            .EmailAddress()
            .WithMessage("Email must be a valid email address")
            .When(x => !string.IsNullOrEmpty(x.Email));

        // Age validation - 2 rules
        RuleFor(x => x.Age)
            .NotEmpty()
            .WithMessage("Age is required");

        RuleFor(x => x.Age)
            .GreaterThanOrEqualTo(18)
            .WithMessage("Age must be at least 18 years old");
    }
}
