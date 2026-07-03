using AcademicGateway.Application.Common.Behaviors;
using FluentAssertions;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AcademicGateway.Application.UnitTests.Common.Behaviors;

/// <summary>
/// A dummy request contract designed to validate the orchestration mechanics of the request validation pipeline behavior.
/// </summary>
public class TestRequest : IRequest<TestResponse>
{
    /// <summary>
    /// Gets or sets a sample text field parameter utilized during rule enforcement matrices.
    /// </summary>
    public string? PayloadText { get; set; }
}

/// <summary>
/// A dummy response payload contract returned upon successful execution of the request pipeline.
/// </summary>
public class TestResponse
{
    /// <summary>
    /// Gets or sets a value indicating completion confirmation state.
    /// </summary>
    public bool IsSuccess { get; set; }
}

/// <summary>
/// A lightweight test-double validator that executes cleanly with zero failures.
/// Bypasses Moq expression trees entirely to cleanly eliminate compiler error CS0854.
/// </summary>
public class PassingTestValidator : AbstractValidator<TestRequest>
{
    // Inheriting from AbstractValidator with no rules causes it to always pass successfully.
}

/// <summary>
/// A lightweight test-double validator that forces a predictable validation failure state.
/// Bypasses Moq expression trees entirely to cleanly eliminate compiler error CS0854.
/// </summary>
public class FailingTestValidator : AbstractValidator<TestRequest>
{
    /// <summary>
    /// Initializes a new instance of the failing validator with specified target error outcomes.
    /// </summary>
    public FailingTestValidator(string propertyName, string errorMessage)
    {
        RuleFor(x => x).Custom((_, context) =>
        {
            context.AddFailure(propertyName, errorMessage);
        });
    }
}

/// <summary>
/// Contains completely isolated, production-grade unit verification suites for the cross-cutting <see cref="ValidationBehavior{TRequest, TResponse}"/>.
/// Assures correct concurrent rule processing, fail-fast boundary short-circuiting, and aggregate validation error amalgamation.
/// </summary>
public class ValidationBehaviorTests
{
    /// <summary>
    /// Assures that if no structural validators are registered within the dependency engine for the incoming request type,
    /// the behavior optimizes execution by skipping context tracking loops and hands control straight to the downstream delegate.
    /// </summary>
    [Fact]
    public async Task Handle_GivenNoValidatorsAreRegistered_ShouldBypassValidationAndInvokeNextDelegate()
    {
        // Arrange
        var request = new TestRequest { PayloadText = "Unchecked Data Pipeline Stream" };
        var expectedResponse = new TestResponse { IsSuccess = true };

        var emptyValidatorsCollection = new List<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(emptyValidatorsCollection);

        var nextDelegateCallCount = 0;

        // Use an anonymous delegate to safely ignore MediatR versioning parameter changes
        RequestHandlerDelegate<TestResponse> nextDelegateWrapper = delegate
        {
            nextDelegateCallCount++;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var behavioralResult = await behavior.Handle(
            request,
            nextDelegateWrapper,
            TestContext.Current.CancellationToken);

        // Assert
        behavioralResult.Should().NotBeNull();
        behavioralResult.IsSuccess.Should().BeTrue();
        nextDelegateCallCount.Should().Be(1);
    }

    /// <summary>
    /// Assures that when multiple validators are registered and all return successful execution receipts with zero structural failures,
    /// the request proceeds cleanly through the pipeline block and execution hands off to the underlying delegate layer.
    /// </summary>
    [Fact]
    public async Task Handle_GivenValidatorsPassSuccessfullyWithZeroErrors_ShouldInvokeNextDelegateAndReturnPayload()
    {
        // Arrange
        var request = new TestRequest { PayloadText = "Valid Academic Record Structure" };
        var expectedResponse = new TestResponse { IsSuccess = true };

        var validator1 = new PassingTestValidator();
        var validator2 = new PassingTestValidator();

        var validatorsList = new List<IValidator<TestRequest>> { validator1, validator2 };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validatorsList);

        var nextDelegateCallCount = 0;

        RequestHandlerDelegate<TestResponse> nextDelegateWrapper = delegate
        {
            nextDelegateCallCount++;
            return Task.FromResult(expectedResponse);
        };

        // Act
        var behavioralResult = await behavior.Handle(
            request,
            nextDelegateWrapper,
            TestContext.Current.CancellationToken);

        // Assert
        behavioralResult.Should().NotBeNull();
        behavioralResult.IsSuccess.Should().BeTrue();
        nextDelegateCallCount.Should().Be(1);
    }

    /// <summary>
    /// Assures that if a validator captures an invalid data setup parameter, the middleware intercepts the transaction loop,
    /// throws a comprehensive <see cref="ValidationException"/>, and guarantees that downstream infrastructure blocks are never reached.
    /// </summary>
    [Theory]
    [InlineData("Title", "Title parameter length constraint violated.")]
    [InlineData("Description", "Description text contains unauthorized terminology markup.")]
    [InlineData("ProviderId", "Provider reference tracker context must not be empty.")]
    public async Task Handle_GivenSingleValidatorFails_ShouldThrowValidationExceptionAndShortCircuitPipeline(
        string? targetPropertyName,
        string? structuralErrorMessage)
    {
        // Arrange
        var request = new TestRequest { PayloadText = "Malformed Payload Blueprint" };

        var validator = new FailingTestValidator(
            targetPropertyName ?? "GenericProperty",
            structuralErrorMessage ?? "Validation rule broken.");

        var validatorsList = new List<IValidator<TestRequest>> { validator };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validatorsList);

        var nextDelegateCallCount = 0;

        RequestHandlerDelegate<TestResponse> nextDelegateWrapper = delegate
        {
            nextDelegateCallCount++;
            return Task.FromResult(new TestResponse { IsSuccess = false });
        };

        // Act
        Func<Task> behavioralAction = async () => await behavior.Handle(
            request,
            nextDelegateWrapper,
            TestContext.Current.CancellationToken);

        // Assert
        var validationThrownException = await behavioralAction.Should().ThrowAsync<ValidationException>();
        validationThrownException.And.Errors.Should().HaveCount(1);

        var compiledErrorDetail = validationThrownException.And.Errors.First();
        compiledErrorDetail.PropertyName.Should().Be(targetPropertyName);
        compiledErrorDetail.ErrorMessage.Should().Be(structuralErrorMessage);

        nextDelegateCallCount.Should().Be(0);
    }

    /// <summary>
    /// Assures that when multiple independent validators discover structural violations concurrently, the cross-cutting behavior
    /// flattens and amalgamates all distinct failure items together into a unified collection inside the thrown exception structure.
    /// </summary>
    [Fact]
    public async Task Handle_GivenMultipleIndependentValidatorsFailConcurrently_ShouldAmalgamateAllErrorsIntoUnifiedExceptionPayload()
    {
        // Arrange
        var request = new TestRequest { PayloadText = "Severely Damaged Business Request Message" };

        var validator1 = new FailingTestValidator("Email", "Email syntax mapping format is incorrect.");
        var validator2 = new FailingTestValidator("PostalCode", "Postal code layout context is unrecognized.");
        var validator3 = new FailingTestValidator("AgeLimit", "Candidate profile does not satisfy age limit parameters.");

        var validatorsList = new List<IValidator<TestRequest>> { validator1, validator2, validator3 };
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validatorsList);

        var nextDelegateCallCount = 0;

        RequestHandlerDelegate<TestResponse> nextDelegateWrapper = delegate
        {
            nextDelegateCallCount++;
            return Task.FromResult(new TestResponse { IsSuccess = false });
        };

        // Act
        Func<Task> behavioralAction = async () => await behavior.Handle(
            request,
            nextDelegateWrapper,
            TestContext.Current.CancellationToken);

        // Assert
        var thrownValidationWrapper = await behavioralAction.Should().ThrowAsync<ValidationException>();
        thrownValidationWrapper.And.Errors.Should().HaveCount(3);

        var compiledErrorsList = thrownValidationWrapper.And.Errors.ToList();
        compiledErrorsList.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage == "Email syntax mapping format is incorrect.");
        compiledErrorsList.Should().Contain(e => e.PropertyName == "PostalCode" && e.ErrorMessage == "Postal code layout context is unrecognized.");
        compiledErrorsList.Should().Contain(e => e.PropertyName == "AgeLimit" && e.ErrorMessage == "Candidate profile does not satisfy age limit parameters.");

        nextDelegateCallCount.Should().Be(0);
    }
}