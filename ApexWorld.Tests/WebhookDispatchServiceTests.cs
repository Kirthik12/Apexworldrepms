using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Webhooks.Models;
using ApexWorld_Backend.Features.Webhooks.Services;
using Hangfire;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace ApexWorld.Tests
{
    [TestFixture]
    public class WebhookDispatchServiceTests
    {
        private Mock<IRepository<WebhookSubscription>> _subscriptionRepoMock;
        private Mock<IRepository<WebhookDeliveryLog>> _logRepoMock;
        private Mock<IBackgroundJobClient> _backgroundJobClientMock;
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private WebhookDispatchService _webhookService;

        [SetUp]
        public void Setup()
        {
            _subscriptionRepoMock = new Mock<IRepository<WebhookSubscription>>();
            _logRepoMock = new Mock<IRepository<WebhookDeliveryLog>>();
            _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _httpClientFactoryMock.Setup(x => x.CreateClient("WebhookDispatcher")).Returns(httpClient);

            _webhookService = new WebhookDispatchService(_subscriptionRepoMock.Object, _logRepoMock.Object, _backgroundJobClientMock.Object, _httpClientFactoryMock.Object);
        }

        [Test]
        public async Task EnqueueEventAsync_WithMatchingSubscription_EnqueuesHangfireJob()
        {
            // Arrange
            var subscriptions = new List<WebhookSubscription>
            {
                new WebhookSubscription
                {
                    Id = 1,
                    EndpointUrl = "https://example.com/webhook",
                    EventTypes = "Booking.Created",
                    Secret = "test-secret",
                    IsActive = true
                }
            };
            
            // Mock GetAsync to return the subscriptions
            _subscriptionRepoMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<WebhookSubscription, bool>>>(), It.IsAny<string>()))
                .ReturnsAsync(subscriptions);

            var payload = new { BookingId = 1, Status = "Created" };

            // Act
            await _webhookService.EnqueueEventAsync("Booking.Created", payload);

            // Assert
            _backgroundJobClientMock.Verify(x => x.Create(
                It.IsAny<Hangfire.Common.Job>(),
                It.IsAny<Hangfire.States.EnqueuedState>()), 
                Times.Once); 
        }

        [Test]
        public async Task DispatchWebhookAsync_ValidSubscription_SendsHttpRequestAndLogsSuccess()
        {
            // Arrange
            var sub = new WebhookSubscription
            {
                Id = 1,
                EndpointUrl = "https://example.com/webhook",
                EventTypes = "*",
                Secret = "test-secret",
                IsActive = true
            };
            
            _subscriptionRepoMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(sub);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("OK")
                });

            string payloadJson = "{\"test\":\"data\"}";
            WebhookDeliveryLog? savedLog = null;
            _logRepoMock.Setup(repo => repo.AddAsync(It.IsAny<WebhookDeliveryLog>()))
                .Callback<WebhookDeliveryLog>(log => savedLog = log)
                .ReturnsAsync((WebhookDeliveryLog log) => log);

            // Act
            await _webhookService.DispatchWebhookAsync(sub.Id, "Test.Event", payloadJson);

            // Assert
            Assert.That(savedLog, Is.Not.Null);
            Assert.That(savedLog.IsSuccess, Is.True);
            Assert.That(savedLog.StatusCode, Is.EqualTo(200));
            
            // Verify HTTP client was called
            _httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => 
                    req.Method == HttpMethod.Post && 
                    req.RequestUri!.ToString() == "https://example.com/webhook" &&
                    req.Headers.Contains("X-ApexWorld-Signature")),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
