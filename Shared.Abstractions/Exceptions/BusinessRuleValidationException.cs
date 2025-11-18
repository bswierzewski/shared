using Shared.Abstractions.Abstractions;

namespace Shared.Abstractions.Exceptions;

/// <summary>
/// Exception thrown when a business rule validation fails.
/// This exception is specifically used to indicate that a domain operation violated a business rule.
/// </summary>
public class BusinessRuleValidationException : DomainException
{
    /// <summary>
    /// Gets the business rule that was violated.
    /// </summary>
    public IBusinessRule BrokenRule { get; }

    /// <summary>
    /// Gets additional details about the business rule violation.
    /// </summary>
    public string Details { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleValidationException"/> class.
    /// </summary>
    /// <param name="brokenRule">The business rule that was violated.</param>
    public BusinessRuleValidationException(IBusinessRule brokenRule) 
        : base(brokenRule.Message)
    {
        BrokenRule = brokenRule;
        Details = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleValidationException"/> class with additional details.
    /// </summary>
    /// <param name="brokenRule">The business rule that was violated.</param>
    /// <param name="details">Additional details about the business rule violation.</param>
    public BusinessRuleValidationException(IBusinessRule brokenRule, string details) 
        : base($"{brokenRule.Message}. Details: {details}")
    {
        BrokenRule = brokenRule;
        Details = details;
    }

    /// <summary>
    /// Returns a string representation of the business rule validation exception.
    /// </summary>
    /// <returns>A formatted string containing the rule type and message.</returns>
    public override string ToString()
    {
        return $"Business rule of type '{BrokenRule.GetType().Name}' was violated. {Message}";
    }
}