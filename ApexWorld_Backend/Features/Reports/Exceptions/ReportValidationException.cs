using System;
using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Reports.Exceptions
{
    public class ReportValidationException : Exception
    {
        public List<string> Errors { get; }
        
        public ReportValidationException(string message) : base(message) 
        { 
            Errors = new List<string> { message };
        }
        
        public ReportValidationException(List<string> errors) : base("Validation failed: " + string.Join(", ", errors))
        {
            Errors = errors;
        }
    }
}
