using System.Linq;
using ApexWorld_Backend.Features.Payment.DTOs;
using ApexWorld_Backend.Common.Interfaces;

namespace ApexWorld_Backend.Features.Payment.Rules
{
    public class ValidPaymentMethodRule : IRule<PaymentInitiateRequestDto>
    {
        public string ErrorMessage => "Invalid payment method. Accepted methods are Razorpay, NetBanking, DebitCreditCard, card, upi.";

        public bool IsSatisfiedBy(PaymentInitiateRequestDto request)
        {
            var allowedMethods = new[]
            {
                "Razorpay", "razorpay",
                "NetBanking", "netbanking",
                "DebitCreditCard", "card",
                "upi", "UPI"
            };
            return allowedMethods.Contains(request.PaymentMethod);
        }
    }
}

