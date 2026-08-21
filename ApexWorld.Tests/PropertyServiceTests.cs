using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Property.Services;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace ApexWorld.Tests
{
    [TestFixture]
    public class PropertyServiceTests
    {
        private Mock<IRepository<Property>> _propertyRepoMock;
        private Mock<IReadOnlyRepository<Property>> _propertyReadOnlyRepoMock;
        private Mock<IRepository<PropertyCategory>> _categoryRepoMock;
        private Mock<IRepository<PropertyImage>> _imageRepoMock;
        private Mock<IAuditService> _auditServiceMock;
        private Mock<IPropertyCancellationSagaService> _sagaServiceMock;
        private Mock<ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService> _webhookServiceMock;
        private Microsoft.Extensions.Caching.Memory.IMemoryCache _cache;
        private PropertyService _propertyService;

        [SetUp]
        public void Setup()
        {
            _propertyRepoMock = new Mock<IRepository<Property>>();
            _propertyReadOnlyRepoMock = new Mock<IReadOnlyRepository<Property>>();
            _categoryRepoMock = new Mock<IRepository<PropertyCategory>>();
            _imageRepoMock = new Mock<IRepository<PropertyImage>>();
            _auditServiceMock = new Mock<IAuditService>();
            _sagaServiceMock = new Mock<IPropertyCancellationSagaService>();
            _webhookServiceMock = new Mock<ApexWorld_Backend.Features.Webhooks.Services.IWebhookDispatchService>();
            var adminNotificationMock = new Mock<ApexWorld_Backend.Features.Notifications.Services.IAdminNotificationService>();
            _cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());

            _propertyService = new PropertyService(
                _propertyRepoMock.Object,
                _propertyReadOnlyRepoMock.Object,
                _categoryRepoMock.Object,
                _imageRepoMock.Object,
                _sagaServiceMock.Object,
                _cache,
                _webhookServiceMock.Object,
                adminNotificationMock.Object
            );
        }

        [TearDown]
        public void TearDown()
        {
            _cache?.Dispose();
        }

        [Test]
        public async Task GetAllPropertiesAsync_ReturnsProperties()
        {
            // Arrange
            var properties = new System.Collections.Generic.List<Property> { new Property { Id = 1 } };
            _propertyReadOnlyRepoMock.Setup(r => r.GetAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Property, bool>>>(), It.IsAny<string>()))
                                     .ReturnsAsync(properties);

            // Act
            var result = await _propertyService.GetAllPropertiesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1));
        }
    }
}
