using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Reports.Services;
using ApexWorld_Backend.Features.Reports.Models;
using ApexWorld_Backend.Common.Interfaces;
using System.Collections.Generic;

namespace ApexWorld.UnitTests.Features.Reports;

public class ReportsTests
{
    private readonly Mock<IRepository<Report>> _reportRepoMock;
    private readonly ReportService _sut;

    public ReportsTests()
    {
        _reportRepoMock = new Mock<IRepository<Report>>();
        _sut = new ReportService(_reportRepoMock.Object);
    }

    [Fact]
    public async Task GenerateReportAsync_WhenCalledWithValidRequest_ShouldAddReportToRepositoryAndReturnSuccess()
    {
        // Arrange
        var request = new ApexWorld_Backend.Features.Reports.DTOs.ReportRequestDto
        {
            ReportType = "Financial",
            StartDate = new DateTime(2023, 1, 1),
            EndDate = new DateTime(2023, 1, 31),
            ReportName = "Monthly Financial",
            Format = "PDF"
        };

        _reportRepoMock.Setup(r => r.AddAsync(It.IsAny<Report>()))
            .Callback<Report>(r => r.Id = 42)
            .ReturnsAsync((Report r) => r);

        // Act
        var result = await _sut.GenerateReportAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Financial report scheduled successfully.");
        result.ReportId.Should().Be(42);
        result.DataPayload.Should().Contain("\"ReportType\": \"Financial\"");

        _reportRepoMock.Verify(r => r.AddAsync(It.Is<Report>(x => 
            x.ReportType == "Financial" && 
            x.ReportName == "Monthly Financial" && 
            x.Status == "Scheduled")), Times.Once);
    }

    [Fact]
    public async Task GetReportsAsync_WhenCalledWithoutFilters_ShouldReturnAllReports()
    {
        // Arrange
        var reports = new List<Report>
        {
            new Report { Id = 1, ReportType = "Sales", Status = "Completed" },
            new Report { Id = 2, ReportType = "Booking", Status = "Scheduled" }
        };

        _reportRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var filter = new ApexWorld_Backend.Features.Reports.DTOs.ReportFilterDto();

        // Act
        var result = await _sut.GetReportsAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Id == 1 && r.ReportType == "Sales");
        result.Should().Contain(r => r.Id == 2 && r.ReportType == "Booking");
    }

    [Fact]
    public async Task GetReportsAsync_WhenCalledWithFilters_ShouldReturnFilteredReports()
    {
        // Arrange
        var reports = new List<Report>
        {
            new Report { Id = 1, ReportType = "Sales", Status = "Completed" },
            new Report { Id = 2, ReportType = "Booking", Status = "Scheduled" },
            new Report { Id = 3, ReportType = "Sales", Status = "Scheduled" }
        };

        _reportRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        var filter = new ApexWorld_Backend.Features.Reports.DTOs.ReportFilterDto 
        { 
            ReportType = "Sales", 
            Status = "Completed" 
        };

        // Act
        var result = await _sut.GetReportsAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboardStatsAsync_WhenCalled_ShouldReturnCorrectStats()
    {
        // Arrange
        var reports = new List<Report>
        {
            new Report { Id = 1, ReportType = ApexWorld_Backend.Features.Reports.DTOs.ReportTypes.Booking },
            new Report { Id = 2, ReportType = ApexWorld_Backend.Features.Reports.DTOs.ReportTypes.Booking },
            new Report { Id = 3, ReportType = ApexWorld_Backend.Features.Reports.DTOs.ReportTypes.Payment },
            new Report { Id = 4, ReportType = "Site-Visit" },
            new Report { Id = 5, ReportType = "Other" }
        };

        _reportRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(reports);

        // Act
        var result = await _sut.GetDashboardStatsAsync();

        // Assert
        result.Should().NotBeNull();
        result.TotalReports.Should().Be(5);
        result.BookingReports.Should().Be(2);
        result.PaymentReports.Should().Be(1);
        result.SiteVisitReports.Should().Be(1);
        result.LoanReports.Should().Be(0);
    }

    [Fact]
    public async Task GetReportByIdAsync_WhenReportExists_ShouldReturnReportDto()
    {
        // Arrange
        var report = new Report { Id = 10, ReportName = "Test Report", ReportType = "Loan" };
        _reportRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(report);

        // Act
        var result = await _sut.GetReportByIdAsync(10);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(10);
        result.ReportName.Should().Be("Test Report");
        result.ReportType.Should().Be("Loan");
    }

    [Fact]
    public async Task GetReportByIdAsync_WhenReportDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Report?)null);

        // Act
        var result = await _sut.GetReportByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteReportAsync_WhenReportExists_ShouldDeleteAndReturnTrue()
    {
        // Arrange
        var report = new Report { Id = 15 };
        _reportRepoMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(report);
        _reportRepoMock.Setup(r => r.DeleteAsync(report)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.DeleteReportAsync(15);

        // Assert
        result.Should().BeTrue();
        _reportRepoMock.Verify(r => r.DeleteAsync(report), Times.Once);
    }

    [Fact]
    public async Task DeleteReportAsync_WhenReportDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        _reportRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Report?)null);

        // Act
        var result = await _sut.DeleteReportAsync(99);

        // Assert
        result.Should().BeFalse();
        _reportRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Report>()), Times.Never);
    }

    [Fact]
    public async Task UpdateReportAsync_WhenReportExists_ShouldUpdateAndReturnUpdatedDto()
    {
        // Arrange
        var existingReport = new Report 
        { 
            Id = 20, 
            ReportName = "Old Name", 
            ReportType = "Sales",
            Status = "Draft"
        };
        
        var updateRequest = new ApexWorld_Backend.Features.Reports.DTOs.ReportRequestDto
        {
            ReportName = "New Name",
            ReportType = "Sales",
            Format = "Excel",
            ReportStatus = "Completed"
        };

        _reportRepoMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(existingReport);
        _reportRepoMock.Setup(r => r.UpdateAsync(existingReport)).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.UpdateReportAsync(20, updateRequest);

        // Assert
        result.Should().NotBeNull();
        result!.ReportName.Should().Be("New Name");
        result.Status.Should().Be("Completed");
        
        _reportRepoMock.Verify(r => r.UpdateAsync(It.Is<Report>(x => 
            x.Id == 20 && 
            x.ReportName == "New Name" && 
            x.Status == "Completed")), Times.Once);
    }

    [Fact]
    public async Task UpdateReportAsync_WhenReportDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var updateRequest = new ApexWorld_Backend.Features.Reports.DTOs.ReportRequestDto();
        _reportRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Report?)null);

        // Act
        var result = await _sut.UpdateReportAsync(99, updateRequest);

        // Assert
        result.Should().BeNull();
        _reportRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Report>()), Times.Never);
    }
}

public class DocumentGeneratorServiceTests
{
    private readonly DocumentGeneratorService _sut;

    public DocumentGeneratorServiceTests()
    {
        _sut = new DocumentGeneratorService();
    }

    [Fact]
    public void GenerateCsv_WithValidData_ShouldReturnByteArray()
    {
        // Arrange
        var data = new List<dynamic>
        {
            new { Id = 1, Name = "Test Report" },
            new { Id = 2, Name = "Another Report" }
        };

        // Act
        var result = _sut.GenerateCsv(data);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
    }
}
