using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shared.Exceptions.Application.Commands.ThrowForbiddenException;
using Shared.Exceptions.Application.Commands.ThrowNotFoundException;
using Shared.Exceptions.Application.Commands.ThrowServerException;
using Shared.Exceptions.Application.Commands.ThrowUnauthorizedException;
using Shared.Exceptions.Application.Commands.ThrowValidationException;
using Shared.Infrastructure.Exceptions;
using ValidationException = Shared.Infrastructure.Exceptions.ValidationException;

namespace Shared.Exceptions.Tests.EndToEnd;

/// <summary>
/// Unit tests for exception throwing commands.
/// Verifies that each command correctly throws the expected exception type.
/// </summary>
public class ExceptionHandlingTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISender _sender;

    public ExceptionHandlingTests()
    {
        var services = new ServiceCollection();

        // Register FluentValidation validators from Application assembly
        services.AddValidatorsFromAssembly(typeof(Application.ApplicationAssembly).Assembly);

        // Register MediatR with handlers and ValidationBehavior from Application assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Application.ApplicationAssembly).Assembly);
            cfg.AddOpenBehavior(typeof(Shared.Infrastructure.Behaviors.ValidationBehavior<,>));
        });

        _serviceProvider = services.BuildServiceProvider();
        _sender = _serviceProvider.GetRequiredService<ISender>();
    }

    [Fact]
    public async Task ThrowValidationExceptionCommand_ShouldThrowValidationException()
    {
        // Arrange - Create command with invalid data to trigger validation
        var command = new ThrowValidationExceptionCommand(
            Title: null,      // Required field violation
            Content: "",      // MinimumLength(10) violation
            Tags: null        // NotEmpty violation
        );

        // Act & Assert - ValidationBehavior should throw before handler executes
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await _sender.Send(command));

        exception.Errors.Should().NotBeEmpty();
        exception.Errors.Should().ContainKey("Title");
        exception.Errors.Should().ContainKey("Content");
        exception.Errors.Should().ContainKey("Tags");

        exception.Errors["Title"].Should().Contain("The Title field is required.");
        exception.Errors["Content"].Should().Contain("The Content field must be at least 10 characters long.");
        exception.Errors["Tags"].Should().Contain("At least one tag is required.");
    }

    [Fact]
    public async Task ThrowNotFoundExceptionCommand_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new ThrowNotFoundExceptionCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            async () => await _sender.Send(command));

        exception.Message.Should().Contain("Resource");
        exception.Message.Should().Contain("00000000-0000-0000-0000-000000000000");
    }

    [Fact]
    public async Task ThrowUnauthorizedExceptionCommand_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var command = new ThrowUnauthorizedExceptionCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            async () => await _sender.Send(command));

        exception.Message.Should().Contain("authenticated");
    }

    [Fact]
    public async Task ThrowForbiddenExceptionCommand_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var command = new ThrowForbiddenExceptionCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenAccessException>(
            async () => await _sender.Send(command));

        exception.Message.Should().Contain("permission");
    }

    [Fact]
    public async Task ThrowServerExceptionCommand_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new ThrowServerExceptionCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _sender.Send(command));

        exception.Message.Should().Contain("unexpected error");
    }

    [Fact]
    public async Task ValidationException_ShouldHaveCorrectErrorStructure()
    {
        // Arrange - Create command with invalid data
        var command = new ThrowValidationExceptionCommand(
            Title: null,
            Content: "",
            Tags: null
        );

        // Act
        var exception = await Assert.ThrowsAsync<ValidationException>(
            async () => await _sender.Send(command));

        // Assert - Verify error structure matches ProblemDetails expectations
        exception.Errors.Should().BeOfType<Dictionary<string, string[]>>();
        exception.Errors.Should().NotBeEmpty();

        // Each error should have property name as key and array of messages as value
        foreach (var error in exception.Errors)
        {
            error.Key.Should().NotBeNullOrEmpty();
            error.Value.Should().NotBeNull();
            error.Value.Should().NotBeEmpty();
            error.Value.Should().AllBeOfType<string>();
        }
    }
}
