using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApexWorld_Backend.Features.Enquiry.Services;
using ApexWorld_Backend.Features.Enquiry.DTOs;
using ApexWorld_Backend.Features.Enquiry.Exceptions;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.Notifications.DTOs;
using EnquiryEntity = ApexWorld_Backend.Features.Enquiry.Models.Enquiry;

namespace ApexWorld.UnitTests.Features.Enquiry;

public class EnquiryTests
{
    private readonly Mock<IRepository<EnquiryEntity>> _enquiryRepoMock;
    private readonly Mock<IAdminNotificationService> _adminNotificationServiceMock;
    private readonly EnquiryService _sut;

    public EnquiryTests()
    {
        _enquiryRepoMock = new Mock<IRepository<EnquiryEntity>>();
        _adminNotificationServiceMock = new Mock<IAdminNotificationService>();
        _sut = new EnquiryService(_enquiryRepoMock.Object, _adminNotificationServiceMock.Object);
    }

    [Fact]
    public async Task SubmitEnquiryAsync_WhenValidRequest_ShouldAddEnquiryAndNotifyAdmin()
    {
        // Arrange
        var request = new EnquiryRequestDto
        {
            BuyerName = "John Doe",
            Phone = "1234567890",
            Email = "john@example.com",
            Message = "I am interested in this property."
        };

        _enquiryRepoMock.Setup(r => r.AddAsync(It.IsAny<EnquiryEntity>())).ReturnsAsync(new EnquiryEntity());
        _enquiryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<EnquiryEntity>()))
            .Returns(Task.CompletedTask);
        _adminNotificationServiceMock.Setup(n => n.BroadcastNotificationAsync(It.IsAny<BroadcastNotificationDto>(), It.IsAny<int>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SubmitEnquiryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.BuyerName.Should().Be(request.BuyerName);
        result.Status.Should().Be("New");

        _enquiryRepoMock.Verify(r => r.AddAsync(It.IsAny<EnquiryEntity>()), Times.Once);
        _adminNotificationServiceMock.Verify(n => n.BroadcastNotificationAsync(It.IsAny<BroadcastNotificationDto>(), 0), Times.Once);
    }

    [Fact]
    public async Task SubmitEnquiryAsync_WhenInvalidRequest_ShouldThrowInvalidEnquiryException()
    {
        // Arrange
        var request = new EnquiryRequestDto
        {
            BuyerName = "", // Invalid
            Phone = "123", // Invalid
            Email = "invalid", // Invalid
            Message = "" // Invalid
        };

        // Act
        Func<Task> act = async () => await _sut.SubmitEnquiryAsync(request);

        // Assert
        await act.Should().ThrowAsync<InvalidEnquiryException>();
        _enquiryRepoMock.Verify(r => r.AddAsync(It.IsAny<EnquiryEntity>()), Times.Never);
    }

    [Fact]
    public async Task ResolveEnquiryAsync_WhenValidId_ShouldUpdateStatusToResolved()
    {
        // Arrange
        var enquiryId = 1;
        var enquiry = new EnquiryEntity { Id = enquiryId, Status = "New" };
        _enquiryRepoMock.Setup(r => r.GetByIdAsync(enquiryId)).ReturnsAsync(enquiry);
        _enquiryRepoMock.Setup(r => r.UpdateAsync(It.IsAny<EnquiryEntity>())).Returns(Task.CompletedTask);

        // Act
        await _sut.ResolveEnquiryAsync(enquiryId);

        // Assert
        enquiry.Status.Should().Be("Resolved");
        _enquiryRepoMock.Verify(r => r.GetByIdAsync(enquiryId), Times.Once);
        _enquiryRepoMock.Verify(r => r.UpdateAsync(enquiry), Times.Once);
    }

    [Fact]
    public async Task ResolveEnquiryAsync_WhenInvalidId_ShouldThrowInvalidEnquiryException()
    {
        // Arrange
        var enquiryId = 999;
        _enquiryRepoMock.Setup(r => r.GetByIdAsync(enquiryId)).ReturnsAsync((EnquiryEntity?)null);

        // Act
        Func<Task> act = async () => await _sut.ResolveEnquiryAsync(enquiryId);

        // Assert
        await act.Should().ThrowAsync<InvalidEnquiryException>().WithMessage($"*not found*");
        _enquiryRepoMock.Verify(r => r.UpdateAsync(It.IsAny<EnquiryEntity>()), Times.Never);
    }
}
