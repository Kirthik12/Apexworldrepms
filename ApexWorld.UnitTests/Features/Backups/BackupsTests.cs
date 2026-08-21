using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Backups.Controllers;
using ApexWorld_Backend.Features.Backups.Models;
using ApexWorld.Core.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ApexWorld.UnitTests.Features.Backups
{
    public class BackupsTests
    {
        private readonly Mock<IRepository<Backup>> _repositoryMock;
        private readonly BackupsController _sut;

        public BackupsTests()
        {
            _repositoryMock = new Mock<IRepository<Backup>>();
            _sut = new BackupsController(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllBackups_ShouldReturnSortedBackups()
        {
            // Arrange
            var backups = new List<Backup>
            {
                new Backup { Id = 1, BackupName = "Backup1", DateAndTime = new DateTime(2023, 1, 1) },
                new Backup { Id = 2, BackupName = "Backup2", DateAndTime = new DateTime(2023, 1, 3) },
                new Backup { Id = 3, BackupName = "Backup3", DateAndTime = new DateTime(2023, 1, 2) }
            };

            _repositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(backups);

            // Act
            var result = await _sut.GetAllBackups();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
            var returnedBackups = apiResponse.Data.Should().BeAssignableTo<IEnumerable<Backup>>().Subject.ToList();

            returnedBackups.Should().HaveCount(3);
            returnedBackups[0].Id.Should().Be(2); // Newest first
            returnedBackups[1].Id.Should().Be(3);
            returnedBackups[2].Id.Should().Be(1);

            _repositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetBackupById_WhenBackupExists_ShouldReturnBackup()
        {
            // Arrange
            int backupId = 5;
            var expectedBackup = new Backup { Id = backupId, BackupName = "ImportantBackup", Status = "Success" };

            _repositoryMock.Setup(repo => repo.GetByIdAsync(backupId)).ReturnsAsync(expectedBackup);

            // Act
            var result = await _sut.GetBackupById(backupId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
            var returnedBackup = apiResponse.Data.Should().BeOfType<Backup>().Subject;

            returnedBackup.Id.Should().Be(backupId);
            returnedBackup.BackupName.Should().Be("ImportantBackup");

            _repositoryMock.Verify(repo => repo.GetByIdAsync(backupId), Times.Once);
        }

        [Fact]
        public async Task CreateBackup_ShouldAddBackupAndReturnCreatedBackup()
        {
            // Arrange
            var newBackup = new Backup
            {
                BackupName = "DailyBackup",
                Type = "Incremental",
                Size = "15 GB"
            };

            var savedBackup = new Backup
            {
                Id = 10,
                BackupName = "DailyBackup",
                Type = "Incremental",
                Size = "15 GB"
            };

            _repositoryMock.Setup(repo => repo.AddAsync(newBackup)).ReturnsAsync(savedBackup);

            // Act
            var result = await _sut.CreateBackup(newBackup);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
            var returnedBackup = apiResponse.Data.Should().BeOfType<Backup>().Subject;

            returnedBackup.Id.Should().Be(10);
            returnedBackup.BackupName.Should().Be("DailyBackup");

            _repositoryMock.Verify(repo => repo.AddAsync(newBackup), Times.Once);
        }
    }
}
