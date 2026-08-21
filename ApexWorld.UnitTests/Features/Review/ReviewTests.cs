using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Review.Services;
using ApexWorld_Backend.Features.Review.Models;
using ApexWorld_Backend.Features.Review.DTOs;
using ApexWorld_Backend.Features.Review.Exceptions;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Common.Interfaces;

namespace ApexWorld.UnitTests.Features.Review;

public class ReviewTests
{
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Review.Models.Review>> _reviewRepoMock;
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Booking.Models.Booking>> _bookingRepoMock;
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>> _propertyRepoMock;
    private readonly ReviewService _sut;

    public ReviewTests()
    {
        _reviewRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Review.Models.Review>>();
        _bookingRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Booking.Models.Booking>>();
        _propertyRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>>();

        _sut = new ReviewService(
            _reviewRepoMock.Object,
            _bookingRepoMock.Object,
            _propertyRepoMock.Object
        );
    }

    [Fact]
    public async Task AddPlatformReviewAsync_WhenValidInput_ShouldAddReviewAndReturnId()
    {
        // Arrange
        var buyerId = "1";
        var dto = new CreatePlatformReviewDto
        {
            Rating = 5,
            Comment = "Great platform",
            Tags = new List<string> { "Easy", "Fast" }
        };

        var addedReview = new ApexWorld_Backend.Features.Review.Models.Review { Id = 10 };
        _reviewRepoMock.Setup(r => r.AddAsync(It.IsAny<ApexWorld_Backend.Features.Review.Models.Review>()))
            .ReturnsAsync(addedReview);

        // Act
        var result = await _sut.AddPlatformReviewAsync(buyerId, dto);

        // Assert
        result.Should().Be(10);
        _reviewRepoMock.Verify(r => r.AddAsync(It.Is<ApexWorld_Backend.Features.Review.Models.Review>(rev =>
            rev.BuyerId == 1 &&
            rev.ReviewType == "Platform" &&
            rev.Rating == 5 &&
            rev.Comment == "Great platform" &&
            rev.Tags == "Easy, Fast")), Times.Once);
    }

    [Fact]
    public async Task AddPropertyReviewAsync_WhenBuyerHasNoBookings_ShouldThrowReviewNotAllowedException()
    {
        // Arrange
        var buyerId = "1";
        var dto = new CreatePropertyReviewDto
        {
            PropertyId = 100,
            Rating = 4,
            Comment = "Nice place"
        };

        _bookingRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<ApexWorld_Backend.Features.Booking.Models.Booking, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<ApexWorld_Backend.Features.Booking.Models.Booking>());

        // Act
        Func<Task> act = async () => await _sut.AddPropertyReviewAsync(buyerId, dto);

        // Assert
        await act.Should().ThrowAsync<ReviewNotAllowedException>()
            .WithMessage("You can only review properties you have purchased or visited.");
    }

    [Fact]
    public async Task AddPropertyReviewAsync_WhenBuyerHasBookings_ShouldAddReviewAndReturnId()
    {
        // Arrange
        var buyerId = "1";
        var dto = new CreatePropertyReviewDto
        {
            PropertyId = 100,
            Rating = 4,
            Comment = "Nice place",
            Photos = new List<string> { "photo1.jpg" }
        };

        var bookings = new List<ApexWorld_Backend.Features.Booking.Models.Booking>
        {
            new ApexWorld_Backend.Features.Booking.Models.Booking { Id = 1, BuyerId = 1, PropertyId = 100, Status = "Completed" }
        };

        _bookingRepoMock.Setup(b => b.GetAsync(It.IsAny<Expression<Func<ApexWorld_Backend.Features.Booking.Models.Booking, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(bookings);

        var addedReview = new ApexWorld_Backend.Features.Review.Models.Review { Id = 20 };
        _reviewRepoMock.Setup(r => r.AddAsync(It.IsAny<ApexWorld_Backend.Features.Review.Models.Review>()))
            .ReturnsAsync(addedReview);

        // Act
        var result = await _sut.AddPropertyReviewAsync(buyerId, dto);

        // Assert
        result.Should().Be(20);
        _reviewRepoMock.Verify(r => r.AddAsync(It.Is<ApexWorld_Backend.Features.Review.Models.Review>(rev =>
            rev.BuyerId == 1 &&
            rev.ReviewType == "Property" &&
            rev.PropertyId == 100 &&
            rev.Rating == 4 &&
            rev.Comment == "Nice place" &&
            rev.Photos == "photo1.jpg")), Times.Once);
    }
}
