using Moq;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApexWorld_Backend.Features.Dashboard.Controllers;
using ApexWorld_Backend.Features.Dashboard.Models;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld.Core.Common;

namespace ApexWorld.UnitTests.Features.Dashboard;

public class DashboardTests
{
    private readonly Mock<IRepository<DashboardMetric>> _repositoryMock;
    private readonly DashboardMetricsController _sut;

    public DashboardTests()
    {
        _repositoryMock = new Mock<IRepository<DashboardMetric>>();
        _sut = new DashboardMetricsController(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllMetrics_ShouldReturnOkWithAllMetrics()
    {
        // Arrange
        var expectedMetrics = new List<DashboardMetric>
        {
            new DashboardMetric { Id = 1, Key = "TotalUsers", Value = 1500, Category = "Users", Trend = "Up", DisplayName = "Total Users" },
            new DashboardMetric { Id = 2, Key = "Revenue", Value = 50000, Category = "Sales", Trend = "Down", DisplayName = "Monthly Revenue" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedMetrics);

        // Act
        var result = await _sut.GetAllMetrics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        
        apiResponse.Success.Should().BeTrue();
        
        var returnedMetrics = apiResponse.Data as IEnumerable<DashboardMetric>;
        returnedMetrics.Should().NotBeNull();
        returnedMetrics.Should().HaveCount(2);
        returnedMetrics.Should().BeEquivalentTo(expectedMetrics);
    }

    [Fact]
    public async Task GetMetricById_WhenMetricExists_ShouldReturnOkWithMetric()
    {
        // Arrange
        int metricId = 3;
        var expectedMetric = new DashboardMetric 
        { 
            Id = metricId, 
            Key = "ActiveSessions", 
            Value = 350, 
            Category = "Engagement", 
            Trend = "Stable", 
            DisplayName = "Active Sessions" 
        };

        _repositoryMock.Setup(r => r.GetByIdAsync(metricId)).ReturnsAsync(expectedMetric);

        // Act
        var result = await _sut.GetMetricById(metricId);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        
        apiResponse.Success.Should().BeTrue();
        
        var returnedMetric = apiResponse.Data as DashboardMetric;
        returnedMetric.Should().NotBeNull();
        returnedMetric.Should().BeEquivalentTo(expectedMetric);
    }

    [Fact]
    public async Task FilterMetrics_WhenCategoryProvided_ShouldReturnFilteredMetrics()
    {
        // Arrange
        var categoryFilter = "Sales";
        var allMetrics = new List<DashboardMetric>
        {
            new DashboardMetric { Id = 1, Key = "TotalUsers", Value = 1500, Category = "Users", Trend = "Up", DisplayName = "Total Users" },
            new DashboardMetric { Id = 2, Key = "Revenue", Value = 50000, Category = "Sales", Trend = "Down", DisplayName = "Monthly Revenue" },
            new DashboardMetric { Id = 3, Key = "NewOrders", Value = 120, Category = "Sales", Trend = "Up", DisplayName = "New Orders" }
        };

        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(allMetrics);

        // Act
        var result = await _sut.FilterMetrics(category: categoryFilter, trend: null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
        
        apiResponse.Success.Should().BeTrue();
        
        var filteredMetrics = apiResponse.Data as List<DashboardMetric>;
        filteredMetrics.Should().NotBeNull();
        filteredMetrics.Should().HaveCount(2);
        filteredMetrics.Should().AllSatisfy(m => m.Category.Should().Be(categoryFilter));
    }
}
