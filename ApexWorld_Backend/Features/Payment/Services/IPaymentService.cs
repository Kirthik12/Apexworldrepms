using ApexWorld_Backend.Features.Payment.DTOs;
using ApexWorld_Backend.Features.Payment.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApexWorld_Backend.Features.Property.Models; // TODO: Fix specific usings

namespace ApexWorld_Backend.Features.Payment.Services{
    public interface IPaymentService
    {
        Task<PaymentInitiateResponseDto> InitiatePaymentAsync(string buyerId, PaymentInitiateRequestDto request);
        Task<PaymentRecord> ProcessWebhookAsync(string transactionId, string status, int bookingId);
        Task<List<PaymentRecord>> GetAdminPaymentsAsync();
        Task RefundPayment(string transactionId, int bookingId, decimal? specificAmount = null);
        Task<PaymentRecord?> VerifyPaymentAsync(string paymentLinkId);
    }
}



