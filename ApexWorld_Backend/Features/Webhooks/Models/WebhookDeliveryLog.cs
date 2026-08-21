using ApexWorld_Backend.Common.Models;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Webhooks.Models
{
    public class WebhookDeliveryLog : BaseEntity
    {
        public int WebhookSubscriptionId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
