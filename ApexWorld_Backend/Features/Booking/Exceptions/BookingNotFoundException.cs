using System;

namespace ApexWorld_Backend.Features.Booking.Exceptions
{
    public class BookingNotFoundException : Exception
    {
        public BookingNotFoundException() : base("Entity was not found.") { }
        public BookingNotFoundException(string message) : base(message) { }
        public BookingNotFoundException(int id) : base("Entity with ID " + id + " was not found.") { }
    }
}
