using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ApexWorld_Backend.Data;
using ApexWorld_Backend.Features.Webhooks.Models;
using ApexWorld_Backend.Common.Models;
using ApexWorld.Core.Common;

namespace ApexWorld_Backend.Features.Webhooks.Controllers
{
    [ApiController]
    [Route("api/v1/webhook-subscriptions")]
    [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
    [Tags("Admin - Webhooks")]
    public class WebhookSubscriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public WebhookSubscriptionsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public class CreateSubscriptionDto
        {
            public string EndpointUrl { get; set; } = string.Empty;
            public string Secret { get; set; } = string.Empty;
            public string EventTypes { get; set; } = "*";
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionDto request)
        {
            var sub = new WebhookSubscription
            {
                EndpointUrl = request.EndpointUrl,
                Secret = request.Secret,
                EventTypes = request.EventTypes
            };

            _dbContext.WebhookSubscriptions.Add(sub);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<WebhookSubscription>.SuccessResponse(sub, "Webhook subscription created successfully."));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var subs = await _dbContext.WebhookSubscriptions.ToListAsync();
            return Ok(ApiResponse<object>.SuccessResponse(subs, "Fetched subscriptions."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sub = await _dbContext.WebhookSubscriptions.FindAsync(id);
            if (sub == null) return NotFound(ApiResponse<object>.ErrorResponse("Subscription not found."));

            _dbContext.WebhookSubscriptions.Remove(sub);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Subscription deleted."));
        }
    }
}
