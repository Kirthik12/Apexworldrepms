using System;

namespace ApexWorld_Backend.Features.Payment.Exceptions
{
    public class InvalidPaymentMethodException : Exception
    {
        public InvalidPaymentMethodException() : base("An error occurred.") { }
        public InvalidPaymentMethodException(string message) : base(message) { }
        public InvalidPaymentMethodException(string message, Exception inner) : base(message, inner) { }
    }
}
