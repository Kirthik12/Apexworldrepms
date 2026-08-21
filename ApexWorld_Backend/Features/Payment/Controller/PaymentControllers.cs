using ApexWorld_Backend.Features.Payment.Exceptions;
using ApexWorld_Backend.Features.Payment.DTOs;
using ApexWorld_Backend.Features.Payment.Services;
using System.Threading.Tasks;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApexWorld_Backend.Filters;
using ApexWorld_Backend.Common.Models;
using System;

namespace ApexWorld_Backend.Modules.PaymentRecord.Controllers
{
    [Tags("Buyer - Payments")]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ICurrentUserService _currentUserService;

        public PaymentController(IPaymentService paymentService, ICurrentUserService currentUserService)
        {
            _paymentService = paymentService;
            _currentUserService = currentUserService;
        }

        [HttpPost("initiate")]
        [Authorize(Roles = "Buyer")]
        [Idempotent]
        public async Task<IActionResult> InitiatePayment([FromBody] PaymentInitiateRequestDto request)
        {
            try
            {
                var buyerId = _currentUserService.UserId!;
                var response = await _paymentService.InitiatePaymentAsync(buyerId, request);
                return Ok(ApiResponse<object>.SuccessResponse(response));
            }
            catch (InvalidPaymentMethodException ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }

        [HttpPost("verify/{paymentLinkId}")]
        [Authorize(Roles = "Buyer")]
        public async Task<IActionResult> VerifyPayment(string paymentLinkId)
        {
            try
            {
                var record = await _paymentService.VerifyPaymentAsync(paymentLinkId);
                if (record == null)
                {
                    return NotFound(ApiResponse<string>.ErrorResponse("Payment record not found or could not be verified."));
                }
                return Ok(ApiResponse<object>.SuccessResponse(record));
            }
            catch (System.Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }

    [Tags("Admin - Payments")]
    [ApiController]
    [Route("api/v1/admin/[controller]")]
    public class AdminPaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public AdminPaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _paymentService.GetAdminPaymentsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(payments));
        }
    }
}
