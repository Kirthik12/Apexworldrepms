using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Wishlist.Services;
using ApexWorld_Backend.Features.Wishlist.Models;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Common.Interfaces;

namespace ApexWorld.UnitTests.Features.Wishlist;

public class WishlistTests
{
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Wishlist.Models.Wishlist>> _wishlistRepoMock;
    private readonly Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>> _propertyRepoMock;
    private readonly WishlistService _sut;

    public WishlistTests()
    {
        _wishlistRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Wishlist.Models.Wishlist>>();
        _propertyRepoMock = new Mock<IRepository<ApexWorld_Backend.Features.Property.Models.Property>>();

        _sut = new WishlistService(_wishlistRepoMock.Object, _propertyRepoMock.Object);
    }

    [Fact]
    public async Task GetWishlistPropertiesAsync_WhenValidBuyerId_ShouldReturnProperties()
    {
        // Arrange
        var buyerIdStr = "1";
        var expectedWishlists = new List<ApexWorld_Backend.Features.Wishlist.Models.Wishlist>
        {
            new ApexWorld_Backend.Features.Wishlist.Models.Wishlist
            {
                Id = 10,
                BuyerId = 1,
                PropertyId = 100,
                Property = new ApexWorld_Backend.Features.Property.Models.Property { Id = 100, Title = "Test Property" }
            }
        };

        _wishlistRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<ApexWorld_Backend.Features.Wishlist.Models.Wishlist, bool>>>(), "Property,Property.Images"))
            .ReturnsAsync(expectedWishlists);

        // Act
        var result = await _sut.GetWishlistPropertiesAsync(buyerIdStr);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.First().Id.Should().Be(100);
        result.First().Title.Should().Be("Test Property");
    }

    [Fact]
    public async Task AddToWishlistAsync_WhenPropertyNotFound_ShouldThrowException()
    {
        // Arrange
        var buyerIdStr = "1";
        var propertyId = 999;

        _propertyRepoMock
            .Setup(r => r.GetByIdAsync(propertyId))
            .ReturnsAsync((ApexWorld_Backend.Features.Property.Models.Property)null);

        // Act
        Func<Task> act = async () => await _sut.AddToWishlistAsync(buyerIdStr, propertyId);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Property not found.");
    }

    [Fact]
    public async Task RemoveFromWishlistAsync_WhenItemExists_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var buyerIdStr = "1";
        var propertyId = 100;
        var existingWishlist = new List<ApexWorld_Backend.Features.Wishlist.Models.Wishlist>
        {
            new ApexWorld_Backend.Features.Wishlist.Models.Wishlist { Id = 10, BuyerId = 1, PropertyId = 100 }
        };

        _wishlistRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<ApexWorld_Backend.Features.Wishlist.Models.Wishlist, bool>>>(), ""))
            .ReturnsAsync(existingWishlist);

        _wishlistRepoMock
            .Setup(r => r.DeleteAsync(It.IsAny<ApexWorld_Backend.Features.Wishlist.Models.Wishlist>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RemoveFromWishlistAsync(buyerIdStr, propertyId);

        // Assert
        result.Should().BeTrue();
        _wishlistRepoMock.Verify(r => r.DeleteAsync(It.Is<ApexWorld_Backend.Features.Wishlist.Models.Wishlist>(w => w.Id == 10)), Times.Once);
    }
}
