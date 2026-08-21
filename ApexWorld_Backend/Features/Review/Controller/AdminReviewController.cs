using ApexWorld_Backend.Features.Review.Exceptions;
using ApexWorld_Backend.Features.Review.DTOs;
using ApexWorld_Backend.Features.Review.Services;
using System;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApexWorld_Backend.Modules.Review.Controllers
{
    [Tags("Admin - Reviews")]
    [Route("api/v1/AdminReview")]
    [ApiController]
    public class AdminReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public AdminReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllReviews([FromQuery] string? type)
        {
            var reviews = await _reviewService.GetAllReviewsAsync(type);
            return Ok(ApiResponse<object>.SuccessResponse(reviews, "Reviews fetched successfully."));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteReview(int id)
        {
            await _reviewService.DeleteReviewAsync(id);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Review deleted successfully."));
        }

        [HttpPatch("{id}/respond")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RespondToReview(int id, [FromBody] RespondToReviewDto request)
        {
            try
            {
                await _reviewService.RespondToReviewAsync(id, request.AdminResponse);
                return Ok(ApiResponse<string>.SuccessResponse("Review responded successfully."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
