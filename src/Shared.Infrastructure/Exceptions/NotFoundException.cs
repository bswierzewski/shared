namespace Shared.Infrastructure.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException()
        : base("The requested resource was not found.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// for a specific entity type and key.
    /// </summary>
    /// <param name="name">The name of the entity type.</param>
    /// <param name="key">The key of the entity that was not found.</param>
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}
