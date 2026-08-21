using Moq;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using ApexWorld_Backend.Features.Users.Services;
using ApexWorld_Backend.Features.Users.Models;
using ApexWorld_Backend.Features.Users.DTOs;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Models;
using Microsoft.Extensions.Options;

namespace ApexWorld.UnitTests.Authentication;

public class AuthServiceTests
{
    private readonly Mock<IRepository<User>> _userRepoMock;
    private readonly Mock<IRepository<RefreshToken>> _refreshTokenRepoMock;
    private readonly Mock<IRepository<RevokedToken>> _revokedTokenRepoMock;
    private readonly Mock<IAuditService> _auditServiceMock;
    private readonly IOptions<JwtSettings> _jwtSettingsOptions;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IRepository<User>>();
        _refreshTokenRepoMock = new Mock<IRepository<RefreshToken>>();
        _revokedTokenRepoMock = new Mock<IRepository<RevokedToken>>();
        _auditServiceMock = new Mock<IAuditService>();
        
        var jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-very-long-secret-key-for-testing-purposes-123",
            ExpiryMinutes = 60,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        _jwtSettingsOptions = Options.Create(jwtSettings);

        _sut = new AuthService(
            _userRepoMock.Object,
            _refreshTokenRepoMock.Object,
            _revokedTokenRepoMock.Object,
            _jwtSettingsOptions,
            _auditServiceMock.Object
        );
    }

    [Fact]
    public async Task RegisterBuyerAsync_WhenUsernameIsTaken_ShouldThrowException()
    {
        // Arrange
        var request = new RegisterBuyerDto { Username = "existingUser", Password = "password" };
        var existingUsers = new List<User> { new Buyer { Username = "existingUser" } };
        
        _userRepoMock.Setup(repo => repo.GetAllAsync())
                     .ReturnsAsync(existingUsers);

        // Act
        Func<Task> act = async () => await _sut.RegisterBuyerAsync(request);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Username is already taken.");
    }

    [Fact]
    public async Task RegisterBuyerAsync_WhenValidRequest_ShouldCreateBuyerAndLogAudit()
    {
        // Arrange
        var request = new RegisterBuyerDto 
        { 
            Username = "newUser", 
            Password = "password123",
            FullName = "New User",
            Email = "new@example.com"
        };
        
        _userRepoMock.Setup(repo => repo.GetAllAsync())
                     .ReturnsAsync(new List<User>()); // No existing users

        _userRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Buyer>()))
                     .ReturnsAsync((Buyer b) => b);

        _auditServiceMock.Setup(a => a.LogAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.RegisterBuyerAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("newUser");
        result.Email.Should().Be("new@example.com");
        
        _userRepoMock.Verify(repo => repo.AddAsync(It.Is<Buyer>(b => b.Username == "newUser")), Times.Once);
        _auditServiceMock.Verify(a => a.LogAsync("Register", "Buyer", It.IsAny<string>(), "Registered new buyer: newUser", It.IsAny<string>()), Times.Once);
    }
}
