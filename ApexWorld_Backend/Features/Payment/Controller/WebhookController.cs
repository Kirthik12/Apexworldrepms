using ApexWorld_Backend.Features.Payment.Services;
using ApexWorld.Core.Common;
using ApexWorld_Backend.Common.Models;
using ApexWorld_Backend.Features.Property.Models;
using ApexWorld_Backend.Features.Booking.Models;
using ApexWorld_Backend.Features.Loan.Models;
using ApexWorld_Backend.Features.Review.Models;
using ApexWorld_Backend.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using System.IO;
using System.Text.Json;

namespace ApexWorld_Backend.Modules.PaymentRecord.Controllers
{
    [ApiController]
    [Route("api/v1/webhooks")]
    [AllowAnonymous] // Webhooks are usually called by external providers (Stripe, etc.) without JWT
    [Tags("System - Webhooks")]
    [ServiceFilter(typeof(ApexWorld_Backend.Filters.IdempotencyFilter))]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WebhookController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public WebhookController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        public class GenericPaymentPayload
        {
            public int BookingId { get; set; }
            public string TransactionId { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
        }

        [HttpPost("razorpay")]
        public async Task<IActionResult> HandleRazorpayWebhook()
        {
            string webhookSecret = _configuration["Razorpay:WebhookSecret"] ?? string.Empty;
            
            using var reader = new StreamReader(Request.Body);
            string payload = await reader.ReadToEndAsync();
            
            if (!Request.Headers.TryGetValue("X-Razorpay-Signature", out var signatureHeader))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Missing X-Razorpay-Signature header"));
            }
            string signature = signatureHeader.ToString();
            
            try
            {
                Utils.verifyWebhookSignature(payload, signature, webhookSecret);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid signature: " + ex.Message));
            }
            
            try
            {
                var json = JsonDocument.Parse(payload);
                var root = json.RootElement;
                
                string eventName = root.GetProperty("event").GetString() ?? "";
                
                // Razorpay Payment Link events use "payment_link.paid"
                if (eventName == "payment_link.paid" || eventName == "payment.captured")
                {
                    string paymentLinkId = "";
                    string status = "";
                    int bookingId = 0;

                    if (eventName == "payment_link.paid")
                    {
                        var paymentLinkEntity = root.GetProperty("payload").GetProperty("payment_link").GetProperty("entity");
                        paymentLinkId = paymentLinkEntity.GetProperty("id").GetString() ?? "";
                        status = paymentLinkEntity.GetProperty("status").GetString() ?? ""; // Usually "paid"
                        
                        // We also need bookingId from notes
                        if (paymentLinkEntity.TryGetProperty("notes", out var notes) && notes.TryGetProperty("booking_id", out var bookingIdProp))
                        {
                            if (bookingIdProp.ValueKind == JsonValueKind.Number)
                                bookingId = bookingIdProp.GetInt32();
                            else if (bookingIdProp.ValueKind == JsonValueKind.String)
                                int.TryParse(bookingIdProp.GetString(), out bookingId);
                        }
                    }
                    else if (eventName == "payment.captured")
                    {
                        var paymentEntity = root.GetProperty("payload").GetProperty("payment").GetProperty("entity");
                        paymentLinkId = paymentEntity.GetProperty("id").GetString() ?? ""; // transaction id
                        status = paymentEntity.GetProperty("status").GetString() ?? ""; // Usually "captured"
                        
                        if (paymentEntity.TryGetProperty("notes", out var notes) && notes.TryGetProperty("booking_id", out var bookingIdProp))
                        {
                            if (bookingIdProp.ValueKind == JsonValueKind.Number)
                                bookingId = bookingIdProp.GetInt32();
                            else if (bookingIdProp.ValueKind == JsonValueKind.String)
                                int.TryParse(bookingIdProp.GetString(), out bookingId);
                        }
                    }
                    
                    string paymentStatus = (status == "paid" || status == "captured") ? "Success" : "Failed";
                    if (bookingId > 0)
                    {
                        await _paymentService.ProcessWebhookAsync(paymentLinkId, paymentStatus, bookingId);
                    }
                }
                
                return Ok(ApiResponse<string>.SuccessResponse("Webhook processed successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message));
            }
        }
    }
}
