using System;

namespace ApexWorld_Backend.Features.Review.Exceptions
{
    public class ReviewNotAllowedException : Exception
    {
        public ReviewNotAllowedException() : base("An error occurred.") { }
        public ReviewNotAllowedException(string message) : base(message) { }
        public ReviewNotAllowedException(string message, Exception inner) : base(message, inner) { }
    }
}
