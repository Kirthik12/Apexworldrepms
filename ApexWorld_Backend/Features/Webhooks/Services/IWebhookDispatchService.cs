using System.Threading.Tasks;

namespace ApexWorld_Backend.Features.Webhooks.Services
{
    public interface IWebhookDispatchService
    {
        Task EnqueueEventAsync(string eventType, object payload);
        Task DispatchWebhookAsync(int subscriptionId, string eventType, string payloadJson);
    }
}
