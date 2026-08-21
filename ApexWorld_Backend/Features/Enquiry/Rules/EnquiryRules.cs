using ApexWorld_Backend.Features.Enquiry.DTOs;

namespace ApexWorld_Backend.Features.Enquiry.Rules
{
    public interface IEnquiryRule
    {
        bool IsSatisfiedBy(EnquiryRequestDto request);
        string ErrorMessage { get; }
    }

    public class ValidContactRule : IEnquiryRule
    {
        public string ErrorMessage => "Buyer name and at least one contact method (Email or Phone) must be provided.";

        public bool IsSatisfiedBy(EnquiryRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.BuyerName)) return false;
            
            bool hasPhone = !string.IsNullOrWhiteSpace(request.Phone);
            bool hasEmail = !string.IsNullOrWhiteSpace(request.Email);
            
            return hasPhone || hasEmail;
        }
    }

    public class ValidMessageRule : IEnquiryRule
    {
        public string ErrorMessage => "An enquiry message must be provided.";

        public bool IsSatisfiedBy(EnquiryRequestDto request)
        {
            return !string.IsNullOrWhiteSpace(request.Message);
        }
    }
}

