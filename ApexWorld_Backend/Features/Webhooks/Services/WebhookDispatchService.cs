using System;
using ApexWorld_Backend.Common.Interfaces;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using ApexWorld_Backend.Data;
using ApexWorld_Backend.Features.Webhooks.Models;

namespace ApexWorld_Backend.Features.Webhooks.Services
{
    public class WebhookDispatchService : IWebhookDispatchService
    {
        private readonly IRepository<WebhookSubscription> _subscriptionRepo;
        private readonly IRepository<WebhookDeliveryLog> _logRepo;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IHttpClientFactory _httpClientFactory;

        public WebhookDispatchService(IRepository<WebhookSubscription> subscriptionRepo, IRepository<WebhookDeliveryLog> logRepo, IBackgroundJobClient backgroundJobClient, IHttpClientFactory httpClientFactory)
        {
            _subscriptionRepo = subscriptionRepo;
            _logRepo = logRepo;
            _backgroundJobClient = backgroundJobClient;
            _httpClientFactory = httpClientFactory;
        }

        public async Task EnqueueEventAsync(string eventType, object payload)
        {
            // Serialize payload once
            string payloadJson = JsonSerializer.Serialize(payload);

            // Find all active subscriptions interested in this event
            var subscriptions = await _subscriptionRepo
                .GetAsync(s => s.IsActive && (s.EventTypes == "*" || s.EventTypes.Contains(eventType)));

            foreach (var sub in subscriptions)
            {
                // Enqueue Hangfire job for each subscriber
                _backgroundJobClient.Enqueue(() => DispatchWebhookAsync(sub.Id, eventType, payloadJson));
            }
        }

        public async Task DispatchWebhookAsync(int subscriptionId, string eventType, string payloadJson)
        {
            // Note: This method is executed in background by Hangfire
            var subscription = await _subscriptionRepo.GetByIdAsync(subscriptionId);
            if (subscription == null || !subscription.IsActive) return;

            var client = _httpClientFactory.CreateClient("WebhookDispatcher");
            
            var request = new HttpRequestMessage(HttpMethod.Post, subscription.EndpointUrl);
            request.Content = new StringContent(payloadJson, Encoding.UTF8, "application/json");

            // Add Signature Header
            if (!string.IsNullOrEmpty(subscription.Secret))
            {
                string signature = ComputeSignature(payloadJson, subscription.Secret);
                request.Headers.Add("X-ApexWorld-Signature", signature);
            }
            request.Headers.Add("X-ApexWorld-Event", eventType);

            int statusCode = 500;
            bool isSuccess = false;
            string? responseMessage = null;

            try
            {
                var response = await client.SendAsync(request);
                statusCode = (int)response.StatusCode;
                isSuccess = response.IsSuccessStatusCode;
                responseMessage = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                responseMessage = ex.Message;
            }

            // Log attempt
            var log = new WebhookDeliveryLog
            {
                WebhookSubscriptionId = subscriptionId,
                EventType = eventType,
                Payload = payloadJson,
                StatusCode = statusCode,
                IsSuccess = isSuccess,
                ResponseMessage = responseMessage?.Length > 1000 ? responseMessage.Substring(0, 1000) : responseMessage
            };

            await _logRepo.AddAsync(log);

            // If it failed, throw an exception so Hangfire (or Polly) can retry
            if (!isSuccess)
            {
                throw new Exception($"Webhook delivery failed with status {statusCode}: {responseMessage}");
            }
        }

        private string ComputeSignature(string payload, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(payloadBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
