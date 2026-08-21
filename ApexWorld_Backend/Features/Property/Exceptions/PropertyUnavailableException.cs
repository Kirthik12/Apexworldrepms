using System;

namespace ApexWorld_Backend.Features.Property.Exceptions
{
    public class PropertyUnavailableException : Exception
    {
        public PropertyUnavailableException() : base("Property is unavailable.") { }
        public PropertyUnavailableException(string message) : base(message) { }
        public PropertyUnavailableException(int id) : base("Property with ID " + id + " is unavailable. ") { }
    }
}
