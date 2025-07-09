using FlexArch.OutBox.Abstractions;
using FlexArch.OutBox.Abstractions.IModels;
using FlexArch.OutBox.Core.Middlewares;
using FlexArch.OutBox.Core.Options;
using FlexArch.OutBox.Persistence.EFCore.Models;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace FlexArch.OutBox.Tests.Middlewares;

/// <summary>
/// RetryMiddleware 单元测试
/// </summary>
public class RetryMiddlewareTests
{
    private readonly Mock<IOptions<RetryOptions>> _optionsMock;
    private readonly RetryMiddleware _middleware;

    public RetryMiddlewareTests()
    {
        _optionsMock = new Mock<IOptions<RetryOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(new RetryOptions
        {
            MaxRetryCount = 3,
            DelayInSeconds = 1
        });

        _middleware = new RetryMiddleware(_optionsMock.Object);
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RetryMiddleware(null!));
    }

    [Fact]
    public async Task InvokeAsync_WithNullMessage_ShouldThrowArgumentNullException()
    {
        // Arrange
        var next = new Mock<OutboxPublishDelegate>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _middleware.InvokeAsync(null!, next.Object));
    }

    [Fact]
    public async Task InvokeAsync_WithNullNext_ShouldThrowArgumentNullException()
    {
        // Arrange
        var message = new OutboxMessage { Type = "Test", Payload = "{}" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _middleware.InvokeAsync(message, null!));
    }

    [Fact]
    public async Task InvokeAsync_WithSuccessfulExecution_ShouldCallNextOnce()
    {
        // Arrange
        var message = new OutboxMessage { Type = "Test", Payload = "{}" };
        var nextMock = new Mock<OutboxPublishDelegate>();
        nextMock.Setup(x => x(It.IsAny<IOutboxMessage>())).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(message, nextMock.Object);

        // Assert
        nextMock.Verify(x => x(message), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithFailingExecution_ShouldRetryUpToMaxCount()
    {
        // Arrange
        var message = new OutboxMessage { Type = "Test", Payload = "{}" };
        var nextMock = new Mock<OutboxPublishDelegate>();
        nextMock.Setup(x => x(It.IsAny<IOutboxMessage>()))
               .ThrowsAsync(new InvalidOperationException("Test error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _middleware.InvokeAsync(message, nextMock.Object));

        // 验证调用了 1 次原始调用 + 3 次重试 = 4 次
        nextMock.Verify(x => x(message), Times.Exactly(4));
    }

    [Fact]
    public async Task InvokeAsync_WithTransientFailureThenSuccess_ShouldSucceedOnRetry()
    {
        // Arrange
        var message = new OutboxMessage { Type = "Test", Payload = "{}" };
        var nextMock = new Mock<OutboxPublishDelegate>();
        int callCount = 0;

        nextMock.Setup(x => x(It.IsAny<IOutboxMessage>()))
               .Returns(() =>
               {
                   callCount++;
                   if (callCount < 3) // 前两次失败，第三次成功
                   {
                       throw new InvalidOperationException("Transient error");
                   }
                   return Task.CompletedTask;
               });

        // Act
        await _middleware.InvokeAsync(message, nextMock.Object);

        // Assert
        nextMock.Verify(x => x(message), Times.Exactly(3));
    }
}
