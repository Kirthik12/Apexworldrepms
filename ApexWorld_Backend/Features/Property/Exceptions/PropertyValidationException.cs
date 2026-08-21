using System;
using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Property.Exceptions
{
    public class PropertyValidationException : Exception
    {
        public List<string> Errors { get; }
        
        public PropertyValidationException(string message) : base(message) 
        { 
            Errors = new List<string> { message };
        }
        
        public PropertyValidationException(List<string> errors) : base("Validation failed: " + string.Join(", ", errors))
        {
            Errors = errors;
        }
    }
}
