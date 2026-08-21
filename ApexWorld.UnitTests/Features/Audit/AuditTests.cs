using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Audit.Services;
using ApexWorld_Backend.Features.Audit.Models;
using ApexWorld_Backend.Features.Audit.Exceptions;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Audit.DTOs;

namespace ApexWorld.UnitTests.Features.Audit;

public class AuditTests
{
    private readonly Mock<IRepository<AuditLog>> _auditRepoMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly AuditService _sut;

    public AuditTests()
    {
        _auditRepoMock = new Mock<IRepository<AuditLog>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _sut = new AuditService(
            _auditRepoMock.Object,
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task LogAsync_WhenValidInput_ShouldAddAuditLog()
    {
        // Arrange
        var action = "Create";
        var entityType = "Booking";
        var entityId = "123";
        var details = "Created a booking";
        
        _currentUserServiceMock.Setup(c => c.UserId).Returns("test-user-id");
        _auditRepoMock.Setup(r => r.AddAsync(It.IsAny<AuditLog>())).ReturnsAsync(new AuditLog());

        // Act
        await _sut.LogAsync(action, entityType, entityId, details);

        // Assert
        _auditRepoMock.Verify(r => r.AddAsync(It.Is<AuditLog>(a => 
            a.Action == action &&
            a.EntityType == entityType &&
            a.EntityId == entityId &&
            a.Details == details &&
            a.UserId == "test-user-id"
        )), Times.Once);
    }

    [Fact]
    public async Task LogAsync_WhenInvalidAction_ShouldThrowAuditLogFailedException()
    {
        // Arrange
        var action = ""; // Invalid action
        var entityType = "Booking";
        var entityId = "123";
        var details = "Created a booking";

        // Act
        Func<Task> act = async () => await _sut.LogAsync(action, entityType, entityId, details);

        // Assert
        await act.Should().ThrowAsync<AuditLogFailedException>();
    }

    [Fact]
    public async Task GetAuditLogsAsync_ShouldReturnPagedLogsAndTotalCount()
    {
        // Arrange
        var logs = new List<AuditLog>
        {
            new AuditLog { Id = 1, Action = "Create", EntityType = "Booking", EntityId = "1", UserId = "user1", Details = "details", CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
            new AuditLog { Id = 2, Action = "Update", EntityType = "Property", EntityId = "2", UserId = "user2", Details = "details", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new AuditLog { Id = 3, Action = "Delete", EntityType = "User", EntityId = "3", UserId = "user3", Details = "details", CreatedAt = DateTime.UtcNow.AddMinutes(-1) }
        };

        _auditRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(logs);

        // Act - Page 1, Size 2
        var result = await _sut.GetAuditLogsAsync(pageNumber: 1, pageSize: 2);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
        // Should be ordered descending by CreatedAt
        result.Items.First().Id.Should().Be(3);
        result.Items.Last().Id.Should().Be(2);
    }
}
