using System;
using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Review.Exceptions
{
    public class ReviewValidationException : Exception
    {
        public List<string> Errors { get; }
        
        public ReviewValidationException(string message) : base(message) 
        { 
            Errors = new List<string> { message };
        }
        
        public ReviewValidationException(List<string> errors) : base("Validation failed: " + string.Join(", ", errors))
        {
            Errors = errors;
        }
    }
}
