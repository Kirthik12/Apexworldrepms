using System;
using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Users.Exceptions
{
    public class AuthValidationException : Exception
    {
        public List<string> Errors { get; }
        
        public AuthValidationException(string message) : base(message) 
        { 
            Errors = new List<string> { message };
        }
        
        public AuthValidationException(List<string> errors) : base("Validation failed: " + string.Join(", ", errors))
        {
            Errors = errors;
        }
    }
}
