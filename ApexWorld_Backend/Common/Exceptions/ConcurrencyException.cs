using System;

namespace ApexWorld_Backend.Common.Exceptions
{
    public class ConcurrencyException : Exception
    {
        public ConcurrencyException() : base("A concurrency error occurred.") { }
        public ConcurrencyException(string message) : base(message) { }
        public ConcurrencyException(string message, Exception inner) : base(message, inner) { }
    }
}
