using ApexWorld_Backend.Features.Payment.Exceptions;
using ApexWorld_Backend.Features.Payment.Models;
using ApexWorld_Backend.Features.Payment.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Property.Models; // TODO: Fix specific usings
using ApexWorld_Backend.Common.Interfaces;
using ApexWorld_Backend.Common.Exceptions;

using System.Linq;
using BookingEntity = ApexWorld_Backend.Features.Booking.Models.Booking;
using Hangfire;
using ApexWorld_Backend.Features.Loan.Models; // Added for LoanApplication
using PropertyEntity = ApexWorld_Backend.Features.Property.Models.Property;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using ApexWorld_Backend.Features.Notifications.Services;
using ApexWorld_Backend.Features.Notifications.DTOs;

namespace ApexWorld_Backend.Features.Payment.Services{
    public class PaymentService : ApexWorld_Backend.Features.Payment.Services.IPaymentService
    {
        private readonly IRepository<PaymentRecord> _paymentRepo;
        private readonly IRepository<BookingEntity> _bookingRepo;
        private readonly IRepository<LoanApplication> _loanRepo;
        private readonly IRepository<PropertyEntity> _propertyRepo;
        private readonly IAuditService _auditService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobClient _backgroundJobs;
        private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IRuleEngine<PaymentInitiateRequestDto> _ruleEngine;
        private readonly IBuyerNotificationService _buyerNotificationService;

        public PaymentService(
            IRepository<PaymentRecord> paymentRepo, 
            IRepository<BookingEntity> bookingRepo, 
            IRepository<LoanApplication> loanRepo,
            IRepository<PropertyEntity> propertyRepo,
            IAuditService auditService,
            IUnitOfWork unitOfWork,
            IBackgroundJobClient backgroundJobs,
            System.Net.Http.IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IRuleEngine<PaymentInitiateRequestDto> ruleEngine,
            ApexWorld_Backend.Common.Resilience.ExponentialBackoffRetryPolicy retryPolicy,
            ApexWorld_Backend.Features.BackgroundJobs.Services.IDeadLetterQueueService dlqService,
            IBuyerNotificationService buyerNotificationService)
        {
            _paymentRepo = paymentRepo;
            _bookingRepo = bookingRepo;
            _loanRepo = loanRepo;
            _propertyRepo = propertyRepo;
            _auditService = auditService;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _ruleEngine = ruleEngine;
            _retryPolicy = retryPolicy;
            _dlqService = dlqService;
            _buyerNotificationService = buyerNotificationService;
        }

        private readonly ApexWorld_Backend.Common.Resilience.ExponentialBackoffRetryPolicy _retryPolicy;
        private readonly ApexWorld_Backend.Features.BackgroundJobs.Services.IDeadLetterQueueService _dlqService;

        public async Task<PaymentInitiateResponseDto> InitiatePaymentAsync(string buyerId, PaymentInitiateRequestDto request)
        {
            var ruleResult = _ruleEngine.Evaluate(request);
            if (!ruleResult.IsSuccess)
            {
                throw new InvalidPaymentMethodException(string.Join(", ", ruleResult.Errors));
            }

            var booking = await _bookingRepo.GetByIdAsync(request.BookingId);
            if (booking == null || booking.BuyerId != int.Parse(buyerId))
            {
                throw new Exception("Booking not found or does not belong to the user.");
            }

            var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
            if (property == null || !property.IsAvailable || property.Status == "Booked")
            {
                throw new Exception("This property is no longer available because it has already been booked.");
            }

            // Check if there is an existing loan application for this booking
            var existingLoan = (await _loanRepo.GetAsync(l => l.BookingId == request.BookingId && (l.Status == "Pending" || l.Status == "Approved"))).FirstOrDefault();
            if (existingLoan != null)
            {
                throw new Exception("A loan application has already been initiated or approved for this booking. Cannot initiate payment.");
            }

            if (booking.Status != "Reserved" && booking.Status != "Pending" && booking.Status != "PendingPayment" && booking.Status != "Interested")
            {
                throw new Exception("Booking is not in a valid state for PaymentRecord.");
            }

            // Check for existing pending Razorpay/online payment to enforce idempotency
            var existingPendingPayment = (await _paymentRepo.GetAsync(p => p.BookingId == request.BookingId && p.Status == "Pending" && p.TransactionId != null && p.TransactionId.StartsWith("plink_"))).FirstOrDefault();
            if (existingPendingPayment != null)
            {
                var keyId = _configuration["Razorpay:KeyId"];
                var keySecret = _configuration["Razorpay:KeySecret"];
                if (!string.IsNullOrEmpty(keyId) && !string.IsNullOrEmpty(keySecret))
                {
                    try
                    {
                        RazorpayClient client = new RazorpayClient(keyId, keySecret);
                        PaymentLink paymentLink = client.PaymentLink.Fetch(existingPendingPayment.TransactionId);
                        
                        return new PaymentInitiateResponseDto
                        {
                            PaymentRecordId = existingPendingPayment.Id,
                            Status = existingPendingPayment.Status,
                            PaymentLinkUrl = paymentLink["short_url"].ToString(),
                            TransactionId = existingPendingPayment.TransactionId
                        };
                    }
                    catch (Exception)
                    {
                        // Fall back to creating a new one if fetch fails
                    }
                }
            }

            var PaymentRecord = new PaymentRecord
            {
                BookingId = request.BookingId,
                PropertyId = booking.PropertyId,
                BuyerId = int.Parse(buyerId),
                Amount = 10000m, // Fixed token advance amount
                PaymentMethod = request.PaymentMethod,
                Status = "Pending"
            };

            string? paymentLinkUrl = null;

            if (request.PaymentMethod.Equals("Razorpay", StringComparison.OrdinalIgnoreCase) ||
                request.PaymentMethod.Equals("netbanking", StringComparison.OrdinalIgnoreCase) ||
                request.PaymentMethod.Equals("card", StringComparison.OrdinalIgnoreCase) ||
                request.PaymentMethod.Equals("upi", StringComparison.OrdinalIgnoreCase))
            {
                var keyId = _configuration["Razorpay:KeyId"];
                var keySecret = _configuration["Razorpay:KeySecret"];

                if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
                {
                    throw new Exception("Razorpay failed. Please retry with NetBanking or DebitCreditCard.");
                }

                RazorpayClient client = new RazorpayClient(keyId, keySecret);
                
                Dictionary<string, object> paymentLinkRequest = new Dictionary<string, object>();
                paymentLinkRequest.Add("amount", 10000 * 100); // amount in paisa
                paymentLinkRequest.Add("currency", "INR");
                paymentLinkRequest.Add("accept_partial", false);
                paymentLinkRequest.Add("description", $"Payment for Booking ID {request.BookingId}");
                
                var buyerDisplayName = !string.IsNullOrWhiteSpace(request.BuyerName) ? request.BuyerName : "Buyer";
                Dictionary<string, object> customer = new Dictionary<string, object>();
                customer.Add("name", buyerDisplayName);
                if (!string.IsNullOrEmpty(request.PhoneNumber))
                {
                    customer.Add("contact", request.PhoneNumber);
                }
                paymentLinkRequest.Add("customer", customer);
                
                // Only enable SMS notify when a contact number is present
                paymentLinkRequest.Add("notify", new Dictionary<string, object> {
                    { "sms", !string.IsNullOrEmpty(request.PhoneNumber) },
                    { "email", false }
                });
                
                paymentLinkRequest.Add("reminder_enable", false);
                paymentLinkRequest.Add("callback_url", $"https://localhost:57954/buyer-dashboard/payment-success?bookingId={request.BookingId}");
                paymentLinkRequest.Add("callback_method", "get");
                paymentLinkRequest.Add("notes", new Dictionary<string, object> {
                    { "booking_id", request.BookingId }
                });

                try
                {
                    PaymentLink paymentLink = await _retryPolicy.ExecuteAsync(
                        () => Task.FromResult(client.PaymentLink.Create(paymentLinkRequest)), 
                        "RazorpayPaymentLinkCreate");
                    
                    PaymentRecord.TransactionId = paymentLink["id"].ToString();
                    paymentLinkUrl = paymentLink["short_url"].ToString();
                    await _paymentRepo.AddAsync(PaymentRecord);
                }
                catch (Exception)
                {
                    throw new Exception("Razorpay failed. Please retry with NetBanking or DebitCreditCard.");
                }
            }
            else
            {
                // Fallback processing for NetBanking and DebitCreditCard
                PaymentRecord.Status = "Success";
                PaymentRecord.TransactionId = "Manual_" + Guid.NewGuid().ToString("N").Substring(0, 8);
                
                await _paymentRepo.AddAsync(PaymentRecord);

                booking.Status = "Paid";
                booking.PaymentReference = PaymentRecord.TransactionId;
                await _bookingRepo.UpdateAsync(booking);
                
                if (property != null)
                {
                    property.Status = "Booked";
                    property.IsAvailable = false;
                    await _propertyRepo.UpdateAsync(property);
                }
                
                await _auditService.LogAsync("PaymentSuccess", "PaymentRecord", PaymentRecord.Id.ToString(), $"Fallback {request.PaymentMethod} payment successful.", buyerId);
            }

            if (request.PaymentMethod.Equals("Razorpay", StringComparison.OrdinalIgnoreCase) ||
                request.PaymentMethod.Equals("netbanking", StringComparison.OrdinalIgnoreCase) ||
                request.PaymentMethod.Equals("card", StringComparison.OrdinalIgnoreCase) ||
                request.PaymentMethod.Equals("upi", StringComparison.OrdinalIgnoreCase))
            {
                await _auditService.LogAsync("PaymentInitiated", "PaymentRecord", PaymentRecord.Id.ToString(), $"PaymentRecord of Rs 10,000 initiated by {request.BuyerName}", buyerId);
            }

            return new PaymentInitiateResponseDto
            {
                PaymentRecordId = PaymentRecord.Id,
                Status = PaymentRecord.Status,
                PaymentLinkUrl = paymentLinkUrl,
                TransactionId = PaymentRecord.TransactionId
            };
        }

        public async Task<PaymentRecord> ProcessWebhookAsync(string transactionId, string status, int bookingId)
        {
            var PaymentRecord = (await _paymentRepo.GetAsync(p => p.BookingId == bookingId)).FirstOrDefault();
            
            if (PaymentRecord == null)
            {
                throw new Exception("PaymentRecord record not found.");
            }

            if (PaymentRecord.Status == "Success")
            {
                return PaymentRecord; // Idempotency check: Already processed successfully
            }

            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                throw new Exception("Booking not found.");
            }

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                PaymentRecord.TransactionId = transactionId;

                if (status.Equals("success", StringComparison.OrdinalIgnoreCase))
                {
                    PaymentRecord.Status = "Success";
                    
                    booking.Status = "Paid";
                    booking.PaymentReference = transactionId;
                    
                    await _bookingRepo.UpdateAsync(booking);
                    
                    var property = await _propertyRepo.GetByIdAsync(booking.PropertyId);
                    if (property != null)
                    {
                        if (!property.IsAvailable || property.Status == "Booked")
                        {
                            throw new Exception("This property is no longer available because it has already been booked.");
                        }
                        property.Status = "Booked";
                        property.IsAvailable = false;
                        await _propertyRepo.UpdateAsync(property);
                    }

                    await _auditService.LogAsync("PaymentSuccess", "PaymentRecord", PaymentRecord.Id.ToString(), $"PaymentRecord successful. Transaction: {transactionId}", "System");
                    
                    await _buyerNotificationService.CreateBuyerNotificationAsync(new CreateBuyerNotificationDto
                    {
                        BuyerId = booking.BuyerId,
                        Title = "Payment Successful",
                        Message = $"Your payment for booking {bookingId} was successful! View your receipt here.",
                        Category = "Payments",
                        ActionText = "View Receipt",
                        ActionUrl = $"/api/v1/BuyerBooking/{bookingId}",
                        RelatedEntityType = "Payment",
                        RelatedEntityId = PaymentRecord.Id
                    });
                }
                else
                {
                    PaymentRecord.Status = "Failed";
                    booking.Status = "PaymentFailed";
                    
                    await _bookingRepo.UpdateAsync(booking);
                    await _auditService.LogAsync("PaymentFailed", "PaymentRecord", PaymentRecord.Id.ToString(), $"PaymentRecord failed. Transaction: {transactionId}", "System");

                    await _buyerNotificationService.CreateBuyerNotificationAsync(new CreateBuyerNotificationDto
                    {
                        BuyerId = booking.BuyerId,
                        Title = "Payment Failed",
                        Message = $"Your payment for booking {bookingId} failed. Please try again.",
                        Category = "Payments",
                        ActionText = "Retry Payment",
                        ActionUrl = $"/api/v1/BuyerBooking/{bookingId}",
                        RelatedEntityType = "Payment",
                        RelatedEntityId = PaymentRecord.Id
                    });
                }

                await _paymentRepo.UpdateAsync(PaymentRecord);

                await _unitOfWork.CommitTransactionAsync();

                return PaymentRecord;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();

                // Edge Case 5: Saga Pattern - Compensation Transaction
                // If PaymentRecord succeeded at gateway but DB update failed (e.g. concurrency error on booking), we must refund
                if (status.Equals("success", StringComparison.OrdinalIgnoreCase))
                {
                    _backgroundJobs.Enqueue(() => RefundPayment(transactionId, bookingId, null));
                }

                throw new Exception("Error processing webhook. Transaction rolled back and compensation initiated if necessary.", ex);
            }
        }

        // Hangfire Job for Compensation Transaction
        public async Task RefundPayment(string transactionId, int bookingId, decimal? specificAmount = null)
        {
            try 
            {
                // Check if this transaction is a manual/fallback payment
                if (transactionId.StartsWith("Manual_"))
                {
                    var refundText = specificAmount.HasValue ? $"Amount: Rs. {specificAmount.Value}" : "Full Amount";
                    await _auditService.LogAsync("PaymentRefunded", "PaymentRecord", transactionId, $"Manual Refund Processed: {refundText} for booking {bookingId}.", "System");
                    return;
                }

                var keyId = _configuration["Razorpay:KeyId"];
                var keySecret = _configuration["Razorpay:KeySecret"];

                if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
                {
                    throw new Exception("Razorpay credentials not configured.");
                }

                RazorpayClient client = new RazorpayClient(keyId, keySecret);
                string paymentId = transactionId;

                // If transactionId is a payment link ID, resolve the actual payment ID
                if (transactionId.StartsWith("plink_"))
                {
                    try
                    {
                        var plink = client.PaymentLink.Fetch(transactionId);
                        var paymentsObj = plink["payments"];
                        if (paymentsObj != null)
                        {
                            var paymentsJson = paymentsObj.ToString();
                            using (var doc = System.Text.Json.JsonDocument.Parse(paymentsJson))
                            {
                                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                                {
                                    var firstPayment = doc.RootElement[0];
                                    if (firstPayment.TryGetProperty("payment_id", out System.Text.Json.JsonElement payIdProp))
                                    {
                                        paymentId = payIdProp.GetString() ?? transactionId;
                                    }
                                    else if (firstPayment.TryGetProperty("id", out System.Text.Json.JsonElement idProp))
                                    {
                                        paymentId = idProp.GetString() ?? transactionId;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await _auditService.LogAsync("RefundFailed", "PaymentRecord", transactionId, $"Failed to resolve payment ID for link: {ex.Message}", "System");
                    }
                }

                // If refund amount is zero, do not call Razorpay Refund API
                if (specificAmount.HasValue && specificAmount.Value == 0)
                {
                    await _auditService.LogAsync("PaymentRefunded", "PaymentRecord", transactionId, $"Retention: 100% cancellation charge applied, refund amount is Rs 0.", "System");
                    return;
                }

                Dictionary<string, object> refundRequest = new Dictionary<string, object>();
                if (specificAmount.HasValue)
                {
                    refundRequest.Add("amount", (int)(specificAmount.Value * 100)); // amount in paise
                }

                // Call Razorpay Refund API
                Refund refund = client.Payment.Fetch(paymentId).Refund(refundRequest);
                string refundId = refund["id"]?.ToString() ?? "Unknown";

                var finalRefundText = specificAmount.HasValue ? $"Amount: Rs. {specificAmount.Value}" : "Full Amount";
                await _auditService.LogAsync("PaymentRefunded", "PaymentRecord", transactionId, $"Razorpay Refund Success: Refund ID {refundId} ({finalRefundText}) for booking {bookingId}.", "System");
            } 
            catch (Exception ex)
            {
                if (ex is System.IO.IOException || ex is System.Net.Http.HttpRequestException)
                {
                    throw; // Let Hangfire retry transient errors
                }
                else
                {
                    // Move to dead letter queue for manual review
                    await _dlqService.EnqueueAsync(new ApexWorld_Backend.Features.BackgroundJobs.Models.DeadLetterMessage
                    {
                        OriginalQueue = "default",
                        Payload = System.Text.Json.JsonSerializer.Serialize(new { TransactionId = transactionId, BookingId = bookingId, Amount = specificAmount }),
                        Exception = ex.ToString(),
                        Timestamp = DateTime.UtcNow
                    });
                    
                    await _auditService.LogAsync("RefundFailed", "PaymentRecord", transactionId, $"Refund failed for booking {bookingId}: {ex.Message}", "System");
                }
            }
        }

        public async Task<List<PaymentRecord>> GetAdminPaymentsAsync()
        {
            var payments = await _paymentRepo.GetAsync(p => true, "Booking,Booking.Property");
            return payments.ToList();
        }

        public async Task<PaymentRecord?> VerifyPaymentAsync(string paymentLinkId)
        {
            var keyId = _configuration["Razorpay:KeyId"];
            var keySecret = _configuration["Razorpay:KeySecret"];
            if (string.IsNullOrEmpty(keyId) || string.IsNullOrEmpty(keySecret))
            {
                throw new Exception("Razorpay keys not configured.");
            }

            RazorpayClient client = new RazorpayClient(keyId, keySecret);
            PaymentLink paymentLink = client.PaymentLink.Fetch(paymentLinkId);
            if (paymentLink == null) return null;

            string status = paymentLink["status"]?.ToString() ?? "";
            
            int bookingId = 0;
            try
            {
                var notes = paymentLink["notes"];
                if (notes != null)
                {
                    var bookingIdStr = paymentLink["notes"]["booking_id"]?.ToString();
                    if (!string.IsNullOrEmpty(bookingIdStr))
                    {
                        int.TryParse(bookingIdStr, out bookingId);
                    }
                }
            }
            catch {}

            if (bookingId == 0)
            {
                var record = (await _paymentRepo.GetAsync(p => p.TransactionId == paymentLinkId)).FirstOrDefault();
                if (record != null)
                {
                    bookingId = record.BookingId;
                }
            }

            if (bookingId > 0)
            {
                string paymentStatus = (status == "paid") ? "Success" : "Failed";
                return await ProcessWebhookAsync(paymentLinkId, paymentStatus, bookingId);
            }

            return null;
        }
    }
}
