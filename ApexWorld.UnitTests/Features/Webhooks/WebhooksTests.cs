using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Features.Webhooks.Models;
using ApexWorld_Backend.Features.Webhooks.Services;

namespace ApexWorld.UnitTests.Features.Webhooks;

public class WebhooksTests
{
    private readonly Mock<IRepository<WebhookSubscription>> _subscriptionRepoMock;
    private readonly Mock<IRepository<WebhookDeliveryLog>> _logRepoMock;
    private readonly Mock<IBackgroundJobClient> _backgroundJobClientMock;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly WebhookDispatchService _sut;

    public WebhooksTests()
    {
        _subscriptionRepoMock = new Mock<IRepository<WebhookSubscription>>();
        _logRepoMock = new Mock<IRepository<WebhookDeliveryLog>>();
        _backgroundJobClientMock = new Mock<IBackgroundJobClient>();
        
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("Success")
            });

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock.Setup(f => f.CreateClient("WebhookDispatcher")).Returns(httpClient);

        _sut = new WebhookDispatchService(_subscriptionRepoMock.Object, _logRepoMock.Object, _backgroundJobClientMock.Object, _httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task EnqueueEventAsync_WhenActiveSubscriptions_ShouldEnqueueHangfireJobs()
    {
        // Arrange
        var subscription = new WebhookSubscription
        {
            Id = 1,
            EndpointUrl = "https://example.com/webhook",
            EventTypes = "booking.created",
            IsActive = true
        };
        
        _subscriptionRepoMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<WebhookSubscription, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(new List<WebhookSubscription> { subscription });

        var payload = new { BookingId = 123, Status = "Pending" };

        // Act
        await _sut.EnqueueEventAsync("booking.created", payload);

        // Assert
        _backgroundJobClientMock.Verify(x => x.Create(
            It.Is<Job>(job => job.Method.Name == nameof(_sut.DispatchWebhookAsync) && (int)job.Args[0] == subscription.Id),
            It.IsAny<EnqueuedState>()), Times.Once);
    }

    [Fact]
    public async Task DispatchWebhookAsync_WhenValidSubscription_ShouldSendHttpRequestAndLog()
    {
        // Arrange
        var subscription = new WebhookSubscription
        {
            Id = 2,
            EndpointUrl = "https://example.com/webhook",
            EventTypes = "booking.approved",
            IsActive = true,
            Secret = "my-secret-key"
        };
        
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(subscription);
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<WebhookDeliveryLog>())).ReturnsAsync((WebhookDeliveryLog l) => l);

        var payloadJson = "{\"BookingId\": 123}";

        // Act
        await _sut.DispatchWebhookAsync(subscription.Id, "booking.approved", payloadJson);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => 
                req.Method == HttpMethod.Post &&
                req.Headers.Contains("X-ApexWorld-Event") &&
                req.Headers.Contains("X-ApexWorld-Signature")),
            ItExpr.IsAny<CancellationToken>()
        );

        _logRepoMock.Verify(r => r.AddAsync(It.Is<WebhookDeliveryLog>(l => 
            l.WebhookSubscriptionId == subscription.Id && 
            l.IsSuccess == true && 
            l.StatusCode == 200)), Times.Once);
    }

    [Fact]
    public async Task DispatchWebhookAsync_WhenHttpRequestFails_ShouldThrowExceptionAndLogFailure()
    {
        // Arrange
        var subscription = new WebhookSubscription
        {
            Id = 3,
            EndpointUrl = "https://example.com/webhook",
            EventTypes = "booking.rejected",
            IsActive = true
        };
        
        _subscriptionRepoMock.Setup(r => r.GetByIdAsync(3)).ReturnsAsync(subscription);
        _logRepoMock.Setup(r => r.AddAsync(It.IsAny<WebhookDeliveryLog>())).ReturnsAsync((WebhookDeliveryLog l) => l);

        var payloadJson = "{\"BookingId\": 456}";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server Error")
            });

        // Act
        Func<Task> act = async () => await _sut.DispatchWebhookAsync(subscription.Id, "booking.rejected", payloadJson);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*Webhook delivery failed with status 500*");

        _logRepoMock.Verify(r => r.AddAsync(It.Is<WebhookDeliveryLog>(l => 
            l.WebhookSubscriptionId == subscription.Id && 
            l.IsSuccess == false && 
            l.StatusCode == 500)), Times.Once);
    }
}
