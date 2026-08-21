using System;

namespace ApexWorld_Backend.Features.Property.Exceptions
{
    public class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException() : base("Entity was not found.") { }
        public PropertyNotFoundException(string message) : base(message) { }
        public PropertyNotFoundException(int id) : base("Entity with ID " + id + " was not found.") { }
    }
}
