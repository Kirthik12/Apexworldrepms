using ApexWorld_Backend.Features.Enquiry.DTOs;
using ApexWorld_Backend.Features.Enquiry.Services;
using ApexWorld.Core.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApexWorld_Backend.Modules.Enquiry.Controllers
{
    [Tags("Shared - Enquiries")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EnquiryController : ControllerBase
    {
        private readonly IEnquiryService _enquiryService;

        public EnquiryController(IEnquiryService enquiryService)
        {
            _enquiryService = enquiryService;
        }

        [HttpPost]
        [AllowAnonymous] // Open to anyone (landing page)
        public async Task<IActionResult> SubmitEnquiry([FromBody] EnquiryRequestDto request)
        {
            try
            {
                var enquiry = await _enquiryService.SubmitEnquiryAsync(request);
                return Ok(ApiResponse<object>.SuccessResponse(enquiry));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }

    [Tags("Admin - Enquiries")]
    [Route("api/v1/admin/AdminEnquiry")]
    [ApiController]
    public class AdminEnquiryController : ControllerBase
    {
        private readonly IEnquiryService _enquiryService;

        public AdminEnquiryController(IEnquiryService enquiryService)
        {
            _enquiryService = enquiryService;
        }

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllEnquiries()
        {
            var enquiries = await _enquiryService.GetAdminEnquiriesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(enquiries));
        }

        [HttpPatch("{id}/resolve")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ResolveEnquiry(int id, [FromBody] ResolveEnquiryDto request)
        {
            try
            {
                await _enquiryService.ResolveEnquiryAsync(id, request.AdminResponse);
                return Ok(ApiResponse<string>.SuccessResponse("Enquiry resolved successfully."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> DeleteEnquiry(int id)
        {
            try
            {
                await _enquiryService.DeleteEnquiryAsync(id);
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Enquiry deleted successfully."));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
