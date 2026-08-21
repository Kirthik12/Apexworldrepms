using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Roles.Services;
using ApexWorld_Backend.Features.Roles.Models;
using ApexWorld_Backend.Common.Interfaces;

namespace ApexWorld.UnitTests.Features.Roles;

public class RolesTests
{
    private readonly Mock<IRepository<Role>> _roleRepoMock;
    private readonly RoleService _sut;

    public RolesTests()
    {
        _roleRepoMock = new Mock<IRepository<Role>>();
        _sut = new RoleService(_roleRepoMock.Object);
    }

    [Fact]
    public async Task GetRoleByIdAsync_WhenValidId_ShouldReturnRole()
    {
        var roleId = 1;
        var expectedRole = new Role { Id = 1, RoleName = "Admin" };
        _roleRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(expectedRole);
        var result = await _sut.GetRoleByIdAsync(roleId);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateRoleAsync_WhenValidName_ShouldCreateAndReturnRole()
    {
        var roleName = "User";
        var expectedRole = new Role { Id = 2, RoleName = roleName };
        _roleRepoMock.Setup(r => r.AddAsync(It.IsAny<Role>())).ReturnsAsync(expectedRole);
        var result = await _sut.CreateRoleAsync(roleName);
        result.Should().NotBeNull();
    }
}
