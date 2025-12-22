using System.Linq.Expressions;
using Shared.Abstractions.Abstractions;

namespace Shared.Infrastructure;

/// <summary>
/// A static utility for generating configuration keys based on IOptions definitions.
/// </summary>
public static class OptionsExtensions
{
    /// <summary>
    /// Generates a configuration key in the format "SectionName__PropertyName" using a lambda expression.
    /// </summary>
    /// <typeparam name="TOptions">The options type implementing IOptions.</typeparam>
    /// <param name="selector">A lambda expression selecting the property (e.g., x => x.ConnectionString).</param>
    /// <returns>The combined configuration key.</returns>
    public static string For<TOptions>(Expression<Func<TOptions, object?>> selector)
        where TOptions : IOptions
    {
        var propertyName = GetMemberName(selector);
        return $"{TOptions.SectionName}:{propertyName}";
    }

    private static string GetMemberName<T>(Expression<Func<T, object?>> expression)
    {
        MemberExpression? memberExpression = null;

        if (expression.Body.NodeType == ExpressionType.MemberAccess)
        {
            memberExpression = expression.Body as MemberExpression;
        }
        else if (expression.Body.NodeType == ExpressionType.Convert)
        {
            var unaryExpression = expression.Body as UnaryExpression;
            memberExpression = unaryExpression?.Operand as MemberExpression;
        }

        if (memberExpression is null)
        {
            throw new ArgumentException("Expression must be a property access.", nameof(expression));
        }

        return memberExpression.Member.Name;
    }
}