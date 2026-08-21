using System;
using System.Collections.Generic;

namespace ApexWorld_Backend.Features.Wishlist.Exceptions
{
    public class WishlistValidationException : Exception
    {
        public List<string> Errors { get; }
        
        public WishlistValidationException(string message) : base(message) 
        { 
            Errors = new List<string> { message };
        }
        
        public WishlistValidationException(List<string> errors) : base("Validation failed: " + string.Join(", ", errors))
        {
            Errors = errors;
        }
    }
}
