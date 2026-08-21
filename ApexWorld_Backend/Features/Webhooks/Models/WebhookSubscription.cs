using ApexWorld_Backend.Common.Models;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Webhooks.Models
{
    public class WebhookSubscription : BaseEntity
    {
        public string EndpointUrl { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty; 
        public string EventTypes { get; set; } = "*"; // e.g., "Booking.Created", "*"
        public bool IsActive { get; set; } = true;
    }
}
