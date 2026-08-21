using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Property.Services;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Property.DTOs;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.Notifications.DTOs;
using ApexWorld_Backend.Features.Webhooks.Services;

namespace ApexWorld.UnitTests.Features.Property;

public class PropertyServiceTests
{
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>> _propertyRepoMock;
    private readonly Mock<IReadOnlyRepository<ApexWorld_Backend.Features.Property.Models.Property>> _propertyReadOnlyRepoMock;
    private readonly Mock<IRepository<PropertyCategory>> _categoryRepoMock;
    private readonly Mock<IRepository<PropertyImage>> _imageRepoMock;
    private readonly Mock<IPropertyCancellationSagaService> _sagaServiceMock;
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<IWebhookDispatchService> _webhookServiceMock;
    private readonly Mock<IAdminNotificationService> _adminNotificationServiceMock;
    private readonly PropertyService _sut;

    public PropertyServiceTests()
    {
        _propertyRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>>();
        _propertyReadOnlyRepoMock = new Mock<IReadOnlyRepository<ApexWorld_Backend.Features.Property.Models.Property>>();
        _categoryRepoMock = new Mock<IRepository<PropertyCategory>>();
        _imageRepoMock = new Mock<IRepository<PropertyImage>>();
        _sagaServiceMock = new Mock<IPropertyCancellationSagaService>();
        _cacheMock = new Mock<IMemoryCache>();
        _webhookServiceMock = new Mock<IWebhookDispatchService>();
        _adminNotificationServiceMock = new Mock<IAdminNotificationService>();

        _sut = new PropertyService(
            _propertyRepoMock.Object,
            _propertyReadOnlyRepoMock.Object,
            _categoryRepoMock.Object,
            _imageRepoMock.Object,
            _sagaServiceMock.Object,
            _cacheMock.Object,
            _webhookServiceMock.Object,
            _adminNotificationServiceMock.Object
        );
    }

    [Fact]
    public async Task GetPropertyByIdAsync_WhenPropertyExists_ShouldReturnProperty()
    {
        // Arrange
        var propertyId = 1;
        var expectedProperties = new List<ApexWorld_Backend.Features.Property.Models.Property>
        {
            new ApexWorld_Backend.Features.Property.Models.Property { Id = propertyId, Title = "Test Property" }
        };

        _propertyReadOnlyRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<ApexWorld_Backend.Features.Property.Models.Property, bool>>>(), "Category,Images"))
            .ReturnsAsync(expectedProperties);

        // Act
        var result = await _sut.GetPropertyByIdAsync(propertyId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(propertyId);
        result.Title.Should().Be("Test Property");
    }

    [Fact]
    public async Task GetPropertyByIdAsync_WhenPropertyDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var propertyId = 999;
        
        _propertyReadOnlyRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<ApexWorld_Backend.Features.Property.Models.Property, bool>>>(), "Category,Images"))
            .ReturnsAsync(new List<ApexWorld_Backend.Features.Property.Models.Property>());

        // Act
        Func<Task> act = async () => await _sut.GetPropertyByIdAsync(propertyId);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Property not found");
    }

    [Fact]
    public async Task UpdatePropertyStatusAsync_WhenValidRequest_ShouldUpdateStatusAndTriggerWebhook()
    {
        // Arrange
        var propertyId = 1;
        var property = new ApexWorld_Backend.Features.Property.Models.Property
        {
            Id = propertyId,
            Title = "Old Title",
            Status = "Pending",
            IsAvailable = false
        };

        var expectedProperties = new List<ApexWorld_Backend.Features.Property.Models.Property> { property };
        _propertyReadOnlyRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<ApexWorld_Backend.Features.Property.Models.Property, bool>>>(), "Category,Images"))
            .ReturnsAsync(expectedProperties);

        var updateReq = new PropertyStatusUpdateDto
        {
            Status = "Available",
            IsAvailable = true
        };

        // Act
        var result = await _sut.UpdatePropertyStatusAsync(propertyId, updateReq);

        // Assert
        result.Status.Should().Be("Available");
        result.IsAvailable.Should().BeTrue();
        
        _propertyRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ApexWorld_Backend.Features.Property.Models.Property>()), Times.Once);
        _webhookServiceMock.Verify(w => w.EnqueueEventAsync("Property.StatusChanged", It.IsAny<ApexWorld_Backend.Features.Property.Models.Property>()), Times.Once);
        _adminNotificationServiceMock.Verify(a => a.BroadcastNotificationAsync(It.IsAny<BroadcastNotificationDto>(), It.IsAny<int>()), Times.Once);
    }
}
