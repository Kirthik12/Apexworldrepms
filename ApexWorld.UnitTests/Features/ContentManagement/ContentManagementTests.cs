using Moq;
using Xunit;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ApexWorld_Backend.Features.ContentManagement.Controllers;
using ApexWorld_Backend.Features.ContentManagement.Models;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld.Core.Common;

namespace ApexWorld.UnitTests.Features.ContentManagement
{
    public class ContentManagementTests
    {
        private readonly Mock<IRepository<Content>> _repositoryMock;
        private readonly ContentsController _sut;

        public ContentManagementTests()
        {
            _repositoryMock = new Mock<IRepository<Content>>();
            _sut = new ContentsController(_repositoryMock.Object);
        }

        [Fact]
        public async Task GetAllContents_ShouldReturnOkWithContents()
        {
            // Arrange
            var expectedContents = new List<Content>
            {
                new Content { Id = 1, Section = "HomePage", Key = "WelcomeText", Value = "Welcome to ApexWorld!", ContentType = "Text", IsActive = true },
                new Content { Id = 2, Section = "AboutUs", Key = "Description", Value = "We are the best.", ContentType = "Text", IsActive = true }
            };

            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedContents);

            // Act
            var result = await _sut.GetAllContents();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
            response.Success.Should().BeTrue();
            
            var contents = response.Data.Should().BeAssignableTo<IEnumerable<Content>>().Subject;
            contents.Should().HaveCount(2);
            contents.First().Section.Should().Be("HomePage");
        }

        [Fact]
        public async Task GetContentById_WhenIdIsValid_ShouldReturnOkWithContent()
        {
            // Arrange
            var contentId = 42;
            var expectedContent = new Content { Id = contentId, Section = "Footer", Key = "ContactEmail", Value = "info@apexworld.com", ContentType = "Text", IsActive = true };

            _repositoryMock.Setup(r => r.GetByIdAsync(contentId)).ReturnsAsync(expectedContent);

            // Act
            var result = await _sut.GetContentById(contentId);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
            response.Success.Should().BeTrue();
            
            var content = response.Data.Should().BeOfType<Content>().Subject;
            content.Id.Should().Be(contentId);
            content.Key.Should().Be("ContactEmail");
        }

        [Fact]
        public async Task UpdateContent_WhenContentExists_ShouldUpdateAndReturnOk()
        {
            // Arrange
            var contentId = 99;
            var existingContent = new Content { Id = contentId, Section = "Header", Key = "LogoUrl", Value = "/images/old-logo.png", ContentType = "Image", IsActive = true };
            var updateRequest = new Content { Id = contentId, Section = "Header", Key = "LogoUrl", Value = "/images/new-logo.png", ContentType = "Image", IsActive = true };

            _repositoryMock.Setup(r => r.GetByIdAsync(contentId)).ReturnsAsync(existingContent);
            _repositoryMock.Setup(r => r.UpdateAsync(existingContent)).Returns(Task.CompletedTask);

            // Act
            var result = await _sut.UpdateContent(contentId, updateRequest);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeOfType<ApiResponse<object>>().Subject;
            response.Success.Should().BeTrue();
            
            var updatedContent = response.Data.Should().BeOfType<Content>().Subject;
            updatedContent.Value.Should().Be("/images/new-logo.png");
            
            _repositoryMock.Verify(r => r.UpdateAsync(existingContent), Times.Once);
        }
    }
}
