using ApexWorld.Core.Common;
using ApexWorld_Backend.Features.BackgroundJobs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hangfire;

namespace ApexWorld_Backend.Features.BackgroundJobs.Controller
{
    [Tags("System - Background Jobs")]
    [Route("api/v1/[controller]")]
    [Authorize(Roles = ApexWorld.Core.Common.Roles.Admin)]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DeadLetterQueueController : ControllerBase
    {
        private readonly IDeadLetterQueueService _dlqService;
        private readonly IBackgroundJobClient _backgroundJobs;

        public DeadLetterQueueController(IDeadLetterQueueService dlqService, IBackgroundJobClient backgroundJobs)
        {
            _dlqService = dlqService;
            _backgroundJobs = backgroundJobs;
        }

        [HttpGet]
        public async Task<IActionResult> GetUnresolvedMessages()
        {
            var messages = await _dlqService.GetUnresolvedMessagesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(messages, "Retrieved DLQ messages."));
        }

    }
}
