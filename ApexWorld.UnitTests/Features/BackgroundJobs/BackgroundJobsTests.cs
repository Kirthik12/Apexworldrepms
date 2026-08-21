using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.BackgroundJobs.Services;
using ApexWorld_Backend.Features.BackgroundJobs.Models;
using ApexWorld_Backend.Common.Interfaces;
using Hangfire;

namespace ApexWorld.UnitTests.Features.BackgroundJobs;

public class BackgroundJobsTests
{
    private readonly Mock<IRepository<DeadLetterMessage>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobsMock;
    private readonly DeadLetterQueueService _sut;

    public BackgroundJobsTests()
    {
        _repositoryMock = new Mock<IRepository<DeadLetterMessage>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _backgroundJobsMock = new Mock<IBackgroundJobClient>();

        _sut = new DeadLetterQueueService(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _backgroundJobsMock.Object
        );
    }

    [Fact]
    public async Task EnqueueAsync_WhenCalled_ShouldAddMessageAndSave()
    {
        // Arrange
        var message = new DeadLetterMessage
        {
            Payload = "TestPayload",
            Exception = "TestException"
        };

        // Act
        await _sut.EnqueueAsync(message);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(message), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetUnresolvedMessagesAsync_WhenCalled_ShouldReturnUnresolvedMessages()
    {
        // Arrange
        var messages = new List<DeadLetterMessage>
        {
            new DeadLetterMessage { Id = 1, IsResolved = false },
            new DeadLetterMessage { Id = 2, IsResolved = false }
        };

        _repositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<DeadLetterMessage, bool>>>(), ""))
            .ReturnsAsync(messages);

        // Act
        var result = await _sut.GetUnresolvedMessagesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task ProcessDeadLetterQueueAsync_WhenRetriesExceeded_ShouldMarkAsResolvedAndAppendException()
    {
        // Arrange
        var messages = new List<DeadLetterMessage>
        {
            new DeadLetterMessage { Id = 1, IsResolved = false, RetryCount = 4, Exception = "Original Error" }
        };

        _repositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<DeadLetterMessage, bool>>>(), ""))
            .ReturnsAsync(messages);

        // Act
        await _sut.ProcessDeadLetterQueueAsync();

        // Assert
        var updatedMessage = messages.First();
        updatedMessage.RetryCount.Should().Be(5);
        updatedMessage.IsResolved.Should().BeTrue();
        updatedMessage.Exception.Should().Contain("Max retries exceeded.");
        updatedMessage.Exception.Should().Contain("Original Error");

        _repositoryMock.Verify(r => r.UpdateAsync(updatedMessage), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }
}
