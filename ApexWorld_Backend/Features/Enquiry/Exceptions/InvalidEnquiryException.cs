using System;

namespace ApexWorld_Backend.Features.Enquiry.Exceptions
{
    public class InvalidEnquiryException : Exception
    {
        public InvalidEnquiryException() : base("An error occurred.") { }
        public InvalidEnquiryException(string message) : base(message) { }
        public InvalidEnquiryException(string message, Exception inner) : base(message, inner) { }
    }
}
