using Moq;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System;
using ApexWorld_Backend.Features.Users.Services;
using ApexWorld_Backend.Features.Users.Models;
using ApexWorld_Backend.Features.Users.DTOs;
using ApexWorld_Backend.Common.Interfaces;

namespace ApexWorld.UnitTests.Features.Users;

public class UserTests
{
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly UserService _sut; // System Under Test

    public UserTests()
    {
        _userRepoMock = new Mock<IRepository<User>>();
        _sut = new UserService(_userRepoMock.Object);
    }

    [Fact]
    public async Task GetBuyerProfileAsync_WhenUserExists_ShouldReturnBuyerProfileDto()
    {
        // Arrange
        int userId = 1;
        var existingUser = new User
        {
            Id = userId,
            Username = "johndoe",
            FullName = "John Doe",
            Email = "john@example.com",
            PhoneNumber = "1234567890",
            City = "New York",
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };
        var usersList = new List<User> { existingUser };
        
        _userRepoMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(usersList);

        // Act
        var result = await _sut.GetBuyerProfileAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
        _userRepoMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>()), Times.Once);
    }

    [Fact]
    public async Task GetBuyerProfileAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        int userId = 99;
        _userRepoMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(new List<User>()); // Empty list

        // Act
        var result = await _sut.GetBuyerProfileAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateBuyerProfileAsync_WhenUserExists_ShouldUpdateAndReturnTrue()
    {
        // Arrange
        int userId = 1;
        var existingUser = new User { Id = userId, FullName = "Old Name" };
        var updateDto = new UpdateBuyerProfileDto
        {
            FullName = "New Name",
            Email = "new@example.com",
            PhoneNumber = "0987654321",
            City = "Boston"
        };
        
        _userRepoMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(new List<User> { existingUser });
                     
        _userRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateBuyerProfileAsync(userId, updateDto);

        // Assert
        result.Should().BeTrue();
        existingUser.FullName.Should().Be("New Name");
        existingUser.City.Should().Be("Boston");
        _userRepoMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
    }

    [Fact]
    public async Task DeleteBuyerAccountAsync_WhenUserExists_ShouldSoftDeleteAndReturnTrue()
    {
        // Arrange
        int userId = 1;
        var existingUser = new User { Id = userId, IsDeleted = false };
        
        _userRepoMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<User, bool>>>()))
                     .ReturnsAsync(new List<User> { existingUser });

        _userRepoMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
                     .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteBuyerAccountAsync(userId);

        // Assert
        result.Should().BeTrue();
        existingUser.IsDeleted.Should().BeTrue();
        _userRepoMock.Verify(repo => repo.UpdateAsync(existingUser), Times.Once);
    }
}
