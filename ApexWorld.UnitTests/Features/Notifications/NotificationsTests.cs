using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.Notifications.Models;
using ApexWorld_Backend.Features.Notifications.DTOs;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using ApexWorld_Backend.Data;
using ApexWorld_Backend.Hubs;
using ApexWorld_Backend.Features.Users.Models;

namespace ApexWorld.UnitTests.Features.Notifications;

public class NotificationsTests
{
    private readonly Mock<IRepository<BuyerNotification>> _notificationRepoMock;
    private readonly BuyerNotificationService _buyerNotificationService;

    public NotificationsTests()
    {
        _notificationRepoMock = new Mock<IRepository<BuyerNotification>>();
        _buyerNotificationService = new BuyerNotificationService(_notificationRepoMock.Object);
    }

    [Fact]
    public async Task GetBuyerNotificationsAsync_WithCategoryAndUnreadOnly_ShouldFilterAndReturnCorrectDto()
    {
        // Arrange
        var buyerId = "100";
        var notifications = new List<BuyerNotification>
        {
            new BuyerNotification { Id = 1, BuyerId = 100, Title = "General Update", Category = "General", IsRead = false, CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new BuyerNotification { Id = 2, BuyerId = 100, Title = "Alert", Category = "Alert", IsRead = true, CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new BuyerNotification { Id = 3, BuyerId = 100, Title = "New Alert", Category = "Alert", IsRead = false, CreatedAt = DateTime.UtcNow }
        };

        _notificationRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<BuyerNotification, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync((Expression<Func<BuyerNotification, bool>> predicate, string include) =>
            {
                var compiled = predicate.Compile();
                return notifications.Where(compiled).ToList();
            });

        // Act
        var result = await _buyerNotificationService.GetBuyerNotificationsAsync(buyerId, "Alert", unreadOnly: true, pageNumber: 1, pageSize: 10);

        // Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(1);
        result.UnreadCount.Should().Be(2); 
        result.Items.Should().ContainSingle();
        result.Items.First().Title.Should().Be("New Alert");
        result.Items.First().Category.Should().Be("Alert");
    }

    [Fact]
    public async Task GetBuyerNotificationByIdAsync_WhenNotificationExists_ShouldReturnDto()
    {
        // Arrange
        var buyerId = "200";
        var notificationId = 5;
        var notification = new BuyerNotification { Id = 5, BuyerId = 200, Title = "Specific Msg" };

        _notificationRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<BuyerNotification, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync((Expression<Func<BuyerNotification, bool>> predicate, string include) =>
            {
                var compiled = predicate.Compile();
                return new List<BuyerNotification> { notification }.Where(compiled).ToList();
            });

        // Act
        var result = await _buyerNotificationService.GetBuyerNotificationByIdAsync(buyerId, notificationId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(5);
        result.Title.Should().Be("Specific Msg");
    }

    [Fact]
    public async Task GetBuyerNotificationByIdAsync_WhenNotificationDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        _notificationRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<BuyerNotification, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<BuyerNotification>());

        // Act
        Func<Task> act = async () => await _buyerNotificationService.GetBuyerNotificationByIdAsync("200", 999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage("Notification not found.");
    }

    [Fact]
    public async Task CreateBuyerNotificationAsync_WithValidData_ShouldAddAndReturnDto()
    {
        // Arrange
        var request = new CreateBuyerNotificationDto
        {
            BuyerId = 300,
            Title = "Welcome!",
            Message = "Glad to have you here.",
            Category = "System",
            ActionText = "View Profile",
            ActionUrl = "/profile"
        };

        _notificationRepoMock
            .Setup(r => r.AddAsync(It.IsAny<BuyerNotification>()))
            .Callback<BuyerNotification>(n => n.Id = 10)
            .ReturnsAsync(new BuyerNotification { Id = 10 });

        // Act
        var result = await _buyerNotificationService.CreateBuyerNotificationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.Title.Should().Be("Welcome!");
        result.Message.Should().Be("Glad to have you here.");
        result.Category.Should().Be("System");
        result.ActionText.Should().Be("View Profile");
        result.ActionUrl.Should().Be("/profile");
        
        _notificationRepoMock.Verify(r => r.AddAsync(It.Is<BuyerNotification>(n => n.Title == "Welcome!" && n.BuyerId == 300)), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_WhenCalled_ShouldSetIsReadToTrueAndCallUpdate()
    {
        // Arrange
        var notification = new BuyerNotification { Id = 50, BuyerId = 400, IsRead = false };
        _notificationRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<BuyerNotification, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<BuyerNotification> { notification });

        // Act
        await _buyerNotificationService.MarkAsReadAsync("400", 50);

        // Assert
        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        _notificationRepoMock.Verify(r => r.UpdateAsync(It.Is<BuyerNotification>(n => n.Id == 50 && n.IsRead == true)), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ShouldUpdateAllUnreadNotifications()
    {
        // Arrange
        var unread1 = new BuyerNotification { Id = 1, BuyerId = 500, IsRead = false };
        var unread2 = new BuyerNotification { Id = 2, BuyerId = 500, IsRead = false };
        _notificationRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<BuyerNotification, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<BuyerNotification> { unread1, unread2 });

        // Act
        await _buyerNotificationService.MarkAllAsReadAsync("500");

        // Assert
        unread1.IsRead.Should().BeTrue();
        unread2.IsRead.Should().BeTrue();
        _notificationRepoMock.Verify(r => r.UpdateAsync(It.IsAny<BuyerNotification>()), Times.Exactly(2));
    }

    // --- AdminNotificationService Tests ---

    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAdminNotificationsAsync_ShouldReturnPaginatedAndFilteredNotifications()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.AdminNotifications.AddRange(
            new AdminNotification { Id = 1, AdminId = 10, Title = "Sys Error", Message = "Error 1", Category = "Error", IsRead = false, CreatedAt = DateTime.UtcNow },
            new AdminNotification { Id = 2, AdminId = 10, Title = "Sys Warning", Message = "Warning 1", Category = "Warning", IsRead = true, CreatedAt = DateTime.UtcNow },
            new AdminNotification { Id = 3, AdminId = 10, Title = "Sys Info", Message = "Info 1", Category = "Info", IsRead = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var hubMock = new Mock<IHubContext<NotificationHub>>();
        var adminService = new AdminNotificationService(context, hubMock.Object);

        // Act
        var result = await adminService.GetAdminNotificationsAsync(10, "Error", unreadOnly: true, pageNumber: 1, pageSize: 5);

        // Assert
        result.Should().NotBeNull();
        result.TotalItems.Should().Be(1);
        result.UnreadCount.Should().Be(2);
        result.Items.Should().ContainSingle();
        result.Items.First().Category.Should().Be("Error");
    }

    [Fact]
    public async Task GetAdminNotificationByIdAsync_ShouldReturnCorrectDto()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.AdminNotifications.Add(new AdminNotification { Id = 5, AdminId = 20, Title = "Important Note", Message = "Msg" });
        await context.SaveChangesAsync();

        var hubMock = new Mock<IHubContext<NotificationHub>>();
        var adminService = new AdminNotificationService(context, hubMock.Object);

        // Act
        var result = await adminService.GetAdminNotificationByIdAsync(20, 5);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(5);
        result.Title.Should().Be("Important Note");
    }

    [Fact]
    public async Task MarkAsReadAsync_ForAdmin_ShouldUpdateRecord()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.AdminNotifications.Add(new AdminNotification { Id = 8, AdminId = 30, IsRead = false, Title = "T", Message = "M" });
        await context.SaveChangesAsync();

        var hubMock = new Mock<IHubContext<NotificationHub>>();
        var adminService = new AdminNotificationService(context, hubMock.Object);

        // Act
        await adminService.MarkAsReadAsync(30, 8);

        // Assert
        var updated = await context.AdminNotifications.FindAsync(8);
        updated!.IsRead.Should().BeTrue();
        updated.ReadAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_ForAdmin_ShouldUpdateAllUnread()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        context.AdminNotifications.AddRange(
            new AdminNotification { Id = 1, AdminId = 40, IsRead = false, Title = "T1", Message = "M1" },
            new AdminNotification { Id = 2, AdminId = 40, IsRead = false, Title = "T2", Message = "M2" },
            new AdminNotification { Id = 3, AdminId = 40, IsRead = true, Title = "T3", Message = "M3" }
        );
        await context.SaveChangesAsync();

        var hubMock = new Mock<IHubContext<NotificationHub>>();
        var adminService = new AdminNotificationService(context, hubMock.Object);

        // Act
        await adminService.MarkAllAsReadAsync(40);

        // Assert
        var allNotifications = await context.AdminNotifications.Where(n => n.AdminId == 40).ToListAsync();
        allNotifications.Should().AllSatisfy(n => n.IsRead.Should().BeTrue());
        allNotifications.Count(n => n.ReadAt != null).Should().Be(2); 
    }

    [Fact]
    public async Task BroadcastNotificationAsync_ToAllUsers_ShouldCreateNotificationsAndBroadcast()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        
        var admin = new Admin { Id = 1, Email = "admin@test.com", PasswordHash = "hash" };
        var admin2 = new Admin { Id = 2, Email = "admin2@test.com", PasswordHash = "hash" };
        var buyer = new Buyer { Id = 3, Email = "buyer@test.com", PasswordHash = "hash" };
        
        context.Admins.AddRange(admin, admin2);
        context.Buyers.Add(buyer);
        
        await context.SaveChangesAsync();

        var hubMock = new Mock<IHubContext<NotificationHub>>();
        var clientsMock = new Mock<IHubClients>();
        var clientProxyMock = new Mock<IClientProxy>();
        
        hubMock.Setup(h => h.Clients).Returns(clientsMock.Object);
        clientsMock.Setup(c => c.Groups(It.Is<IReadOnlyList<string>>(groups => groups.Count == 3))).Returns(clientProxyMock.Object);

        var adminService = new AdminNotificationService(context, hubMock.Object);
        var dto = new BroadcastNotificationDto
        {
            TargetAudience = "AllUsers",
            Title = "Global Update",
            Message = "Maintenance in 5 mins",
            Category = "System"
        };

        // Act
        await adminService.BroadcastNotificationAsync(dto, senderAdminId: 1);

        // Assert
        var adminNotifs = await context.AdminNotifications.ToListAsync();
        adminNotifs.Should().HaveCount(2);
        adminNotifs.Should().AllSatisfy(n => n.Title.Should().Be("Global Update"));

        var buyerNotifs = await context.BuyerNotifications.ToListAsync();
        buyerNotifs.Should().HaveCount(1);
        buyerNotifs.Should().AllSatisfy(n => n.Title.Should().Be("Global Update"));

        clientProxyMock.Verify(
            c => c.SendCoreAsync("ReceiveNotification", It.Is<object[]>(args => args.Length == 1), default),
            Times.Once);
    }
}
