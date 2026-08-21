using System;

namespace ApexWorld_Backend.Features.Loan.Exceptions
{
    public class MaxReapplicationReachedException : Exception
    {
        public MaxReapplicationReachedException() : base("An error occurred.") { }
        public MaxReapplicationReachedException(string message) : base(message) { }
        public MaxReapplicationReachedException(string message, Exception inner) : base(message, inner) { }
    }
}
