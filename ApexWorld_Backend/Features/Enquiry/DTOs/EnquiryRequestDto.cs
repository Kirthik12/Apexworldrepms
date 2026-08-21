namespace ApexWorld_Backend.Features.Enquiry.DTOs{
    public class EnquiryRequestDto
    {
        public string BuyerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ResolveEnquiryDto
    {
        public string AdminResponse { get; set; } = string.Empty;
    }
}

