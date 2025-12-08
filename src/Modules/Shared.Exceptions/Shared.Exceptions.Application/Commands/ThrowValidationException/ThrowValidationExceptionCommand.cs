using FluentValidation;
using MediatR;

namespace Shared.Exceptions.Application.Commands.ThrowValidationException;

/// <summary>
/// Command that throws a ValidationException with sample validation errors.
/// Used for testing exception handling in development environments.
/// </summary>
public record ThrowValidationExceptionCommand(
    string? Title,
    string? Content,
    string[]? Tags
) : IRequest<Unit>;


/// <summary>
/// Validator for ThrowValidationExceptionCommand that ensures validation errors are triggered.
/// </summary>
public class ThrowValidationExceptionCommandValidator : AbstractValidator<ThrowValidationExceptionCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThrowValidationExceptionCommandValidator"/> class.
    /// Configures validation rules for Title, Content, and Tags properties.
    /// </summary>
    public ThrowValidationExceptionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("The Title field is required.");

        RuleFor(x => x.Content)
            .MinimumLength(10)
            .WithMessage("The Content field must be at least 10 characters long.");

        RuleFor(x => x.Tags)
            .NotEmpty()
            .WithMessage("At least one tag is required.");
    }
}