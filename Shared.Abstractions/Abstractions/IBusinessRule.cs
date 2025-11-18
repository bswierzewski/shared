namespace Shared.Abstractions.Abstractions;

/// <summary>
/// Interface for business rules that can be validated.
/// Business rules represent domain constraints and invariants that must be maintained
/// to ensure the integrity and consistency of the domain model.
/// </summary>
public interface IBusinessRule
{
    /// <summary>
    /// Gets the error message that describes what happens when the business rule is broken.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Determines whether the business rule is satisfied.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the business rule is satisfied; otherwise, <c>false</c>.
    /// </returns>
    bool IsBroken();
}